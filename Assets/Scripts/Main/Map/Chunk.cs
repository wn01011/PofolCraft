using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public ChunkCoord coord;
    public float seaLevel = 0f;

    //ChunkMesh
    private MeshRenderer meshRenderer;
    public MeshFilter meshFilter;
    public GameObject chunkObject;

    //WaterMesh
    private MeshRenderer waterRenderer;
    private MeshFilter waterFilter;
    private GameObject waterObject;

    public MeshCollider meshCollider;

    private int vertexIndex = 0;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();

    public byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    private List<Chunk> chunksToUpdate = new List<Chunk>();
    public Queue<VoxelMod> modificatrions = new Queue<VoxelMod>();

    private World world;

    private bool _isActive;
    public bool isVoxelmapPopulated = false;

    public Chunk(ChunkCoord _coord, World _world, bool generateOnLoad)
    {
        coord = _coord;
        world = _world;
        isActive = true;


        if (generateOnLoad)
            Init();
    }

    public void Init()
    {
        seaLevel = world.biome.solidGroundHeight * 0.3f;

        //Chunk
        chunkObject = new GameObject("Chunk " + coord.x + ", " + coord.z, typeof(MeshFilter), typeof(MeshRenderer));
        meshRenderer = chunkObject.GetComponent<MeshRenderer>();
        meshFilter = chunkObject.GetComponent<MeshFilter>();

        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VoxelData.ChunkWidth, 0f, coord.z * VoxelData.ChunkWidth);

        //Water
        waterObject = new GameObject("Water " + coord.x + ", " + coord.z, typeof(MeshFilter), typeof(MeshRenderer));
        waterRenderer = waterObject.GetComponent<MeshRenderer>();
        waterFilter = waterObject.GetComponent<MeshFilter>();

        waterRenderer.material = world.waterMaterial;
        waterObject.transform.SetParent(chunkObject.transform);
        waterObject.transform.localPosition = new Vector3(0.5f * VoxelData.ChunkWidth, world.biome.solidGroundHeight + seaLevel + 0.5f, 0.5f * VoxelData.ChunkWidth);
        waterObject.transform.localScale *= VoxelData.ChunkWidth * 0.1f;

        PopulateVoxelMap();
        UpdateChunk();
        //CreateMesh();
    }


    public Vector3 position
    {
        get { return chunkObject.transform.position; }
    }


    private bool IsVoxelInChunk(int _x, int _y, int _z)
    {
        if (_x < 0 || _x > VoxelData.ChunkWidth  - 1 ||
            _y < 0 || _y > VoxelData.ChunkHeight - 1 ||
            _z < 0 || _z > VoxelData.ChunkWidth  - 1)
            return false;
        else
            return true;
    }


    public void EditVoxel(Vector3 pos, byte newID)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);
        if(voxelMap[xCheck, yCheck, zCheck] != 1)
            voxelMap[xCheck, yCheck, zCheck] = newID;

        UpdateSuroundingVoxels(xCheck, yCheck, zCheck);

        UpdateChunk();
    }

    private void UpdateSuroundingVoxels(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);

        for (int p = 0; p < 6; p++)
        {
            Vector3 currentVoxel = thisVoxel + VoxelData.faceChecks[p];
            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                world.GetChunkFromVector3(thisVoxel + position).UpdateChunk();
            }
        }

    }


    private void PopulateVoxelMap()
    {
        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + position);
                }
            }
        }

        isVoxelmapPopulated = true;

    }

    public void UpdateChunk()
    {
        while (modificatrions.Count > 0)
        {
            VoxelMod v = modificatrions.Dequeue();
            Vector3 pos = v.position - position;
            voxelMap[(int)pos.x, (int)pos.y, (int)pos.z] = v.id;
        }


        ClearMeshData();

        for (int y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (int x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (int z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    if (world.blockTypes[voxelMap[x, y, z]].IsSolid)
                    {
                        world.blockTypes[voxelMap[x, y, z]].hp = 1;
                        UpdateMeshData(new Vector3(x, y, z));
                    }
                }
            }
        }

        CreateMesh();
    }

    private void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }

    public bool isActive
    {
        get { return _isActive; }
        set
        {
            _isActive = value;
            if (chunkObject != null)
                chunkObject.SetActive(value);
        }
    }


    public byte GetVoxelFromMap(Vector3 _pos)
    {
        _pos -= position;
        return voxelMap[(int)_pos.x, (int)_pos.y, (int)_pos.z];
    }

    public bool CheckVoxel(Vector3 _pos) // 해당 위치가 솔리드인지 반환
    {
        int x = Mathf.FloorToInt(_pos.x);
        int y = Mathf.FloorToInt(_pos.y);
        int z = Mathf.FloorToInt(_pos.z);

        if (!IsVoxelInChunk(x, y, z))       // 해당 위치가 청크의 범위를 벗아날 경우 
            return world.CheckForVoxel(_pos + position);


        return world.blockTypes[voxelMap[x, y, z]].IsSolid;
    }


    public byte GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);
        if (IsVoxelInChunk(xCheck, yCheck, zCheck))
            return voxelMap[xCheck, yCheck, zCheck];
        else
            return 0;
    }



    public void UpdateMeshData(Vector3 _pos)
    {
        for (int p = 0; p < 6; p++)
        {
            if (!CheckVoxel(_pos + VoxelData.faceChecks[p]))
            {
                byte blockID = voxelMap[(int)_pos.x, (int)_pos.y, (int)_pos.z];
                vertices.Add(_pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 0]]);
                vertices.Add(_pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 1]]);
                vertices.Add(_pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 2]]);
                vertices.Add(_pos + VoxelData.voxelVerts[VoxelData.voxelTris[p, 3]]);

                AddTexture(world.blockTypes[blockID].GetTextureID(p), uvs);

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                vertexIndex += 4;
            }
        }
    }


    public void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        //Change Vertex Color (Default white to Black), below seaLevel vert Color become Blue
        Color[] colors = new Color[vertices.Count];

        for (int i = 0; i < vertices.Count; ++i)
        {
            if (vertices[i].y <= world.biome.solidGroundHeight + seaLevel && vertices[i].y >= world.biome.solidGroundHeight)
            {
                colors[i] = Color.blue;
            }
            else
            {
                colors[i] = Color.black;
            }
        }
        mesh.colors = colors;
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        if (!meshFilter.gameObject.GetComponent<MeshCollider>())
        {
            meshFilter.gameObject.AddComponent<MeshCollider>();
        }
        meshFilter.gameObject.GetComponent<MeshCollider>().sharedMesh = null;
        meshFilter.gameObject.GetComponent<MeshCollider>().sharedMesh = meshFilter.mesh;



        waterObject.GetComponent<MeshFilter>().mesh = world.waterFilter.sharedMesh;
    }


    public void AddTexture(int _textureID, List<Vector2> _uvs)
    {
        float y = _textureID / VoxelData.TextureAtlasSizeInBlocks;
        float x = _textureID - (y * VoxelData.TextureAtlasSizeInBlocks);

        x *= VoxelData.NormalizeBlockTextureSize;
        y *= VoxelData.NormalizeBlockTextureSize;

        //y = 1f - y - VoxelData.NormalizeBlockTextureSize;

        _uvs.Add(new Vector2(x, y));
        _uvs.Add(new Vector2(x, y + VoxelData.NormalizeBlockTextureSize));
        _uvs.Add(new Vector2(x + VoxelData.NormalizeBlockTextureSize, y));
        _uvs.Add(new Vector2(x + VoxelData.NormalizeBlockTextureSize, y + VoxelData.NormalizeBlockTextureSize));

    }


}
public class ChunkCoord
{
    public int x;
    public int z;

    public ChunkCoord()
    {
        x = 0;
        z = 0;
    }

    public ChunkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    public ChunkCoord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VoxelData.ChunkWidth;
        z = zCheck / VoxelData.ChunkWidth;
    }

    public bool Equals(ChunkCoord other)
    {
        if (other == null)
            return false;
        else if (other.x == x && other.z == z)
            return true;
        else
            return false;
    }
}
