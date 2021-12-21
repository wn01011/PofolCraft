using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chk
{
    public ChkCoord coord;
    public float seaLevel = 15f;

    //ChunkMesh
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private GameObject chunkObject;
    
    private int vertexIndex = 0;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Vector2> uvs = new List<Vector2>();

    public byte[,,] voxelMap = new byte[VD.ChunkWidth, VD.ChunkHeight, VD.ChunkWidth];

    public Queue<VoxelMod_Lobby> modifications = new Queue<VoxelMod_Lobby>();

    private World_Lobby world;
    private BuildingManager buildingMg = null;

    private bool _isActive;
    public bool isVoxelmapPopulated = false;

    //생성자 
    public Chk(ChkCoord _coord, World_Lobby _world, bool generateOnLoad)
    {
        coord = _coord;
        world = _world;
        isActive = true;
        buildingMg = GameObject.Find("BuildingManager").GetComponent<BuildingManager>();

        if (generateOnLoad)
            Init();
    }

    //청크가 생성될때마다 초기화 함 
    public void Init()
    {
        //Chk
        chunkObject = new GameObject("Chk " + coord.x + ", " + coord.z, typeof(MeshFilter), typeof(MeshRenderer));
        meshRenderer = chunkObject.GetComponent<MeshRenderer>();
        meshFilter = chunkObject.GetComponent<MeshFilter>();

        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);
        chunkObject.transform.position = new Vector3(coord.x * VD.ChunkWidth, 0f, coord.z * VD.ChunkWidth);

        PopulateVoxelMap();
        UpdateChunk();
    }

    //청크의 포지션 반환 
    public Vector3 chkPosition
    {
        get { return chunkObject.transform.position; }
    }

    //청크의 내부 좌표 기준으로 청크내부에 있는지 검사 / 내부면 true 반환
    private bool IsVoxelInChunk(int _x, int _y, int _z)
    {
        if (_x < 0 || _x > VD.ChunkWidth - 1 ||
          _y < 0 || _y > VD.ChunkHeight - 1 ||
           _z < 0 || _z > VD.ChunkWidth - 1)
            return false;
        else
            return true;
    }

    //벡터 위치와 바이트 ID 값을 받아서 해당 위치를 받은 바이트 ID로 변경하고, 주변 복셀 및 청크 업데이트
    public void EditVoxel(Vector3 pos, byte newID)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        if(voxelMap[xCheck, yCheck,zCheck] != 1 && voxelMap[xCheck, yCheck, zCheck] != 8)
        {
            //Debug.Log(voxelMap[xCheck, yCheck, zCheck]); ;
            voxelMap[xCheck, yCheck, zCheck] = newID;
        }

        UpdateSuroundingVoxels(xCheck, yCheck, zCheck);

        UpdateChunk();
        
    }

    public void EditVoxelMenuely(Vector3 pos, byte _id)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        if (voxelMap[xCheck, yCheck, zCheck] != 1)
        {
            //Debug.Log(voxelMap[xCheck, yCheck, zCheck]);
            voxelMap[xCheck, yCheck, zCheck] = _id;
        }

    }


    //받은 좌표에 각 면에 +1 을 한 위치가 복셀이 들어있는지르 판단하며 업데이트함 
    private void UpdateSuroundingVoxels(int x, int y, int z)
    {
        Vector3 thisVoxel = new Vector3(x, y, z);

        for (int p = 0; p < 6; p++)
        {
            Vector3 currentVoxel = thisVoxel + VD.faceChecks[p];
            if (!IsVoxelInChunk((int)currentVoxel.x, (int)currentVoxel.y, (int)currentVoxel.z))
            {
                world.GetChunkFromVector3(thisVoxel + chkPosition).UpdateChunk();
            }
        }

    }

    //복셀이 높이를 기준으로 복셀의 값을 배치하고, 청크 내에 복셀맵을 true로 변경함
    private void PopulateVoxelMap()
    {
        for (int y = 0; y < VD.ChunkHeight; y++)
        {
            for (int x = 0; x < VD.ChunkWidth; x++)
            {
                for (int z = 0; z < VD.ChunkWidth; z++)
                {
                    voxelMap[x, y, z] = world.GetVoxel(new Vector3(x, y, z) + chkPosition);
                    
                    if (y == 10 )
                    {
                        buildingMg.AddPreBuildPosList(new Vector3(x, y, z) + chkPosition);
                    }
                }
            }
        }
        isVoxelmapPopulated = true;
    }

    //모디피케이션 : 수정해야하는 복셀의 좌표를 클래스로 모아놓은 큐 
    //복셀모드 : 위치값과 id 값을  가지는 클래스 
    public void UpdateChunk()
    {
        while (modifications.Count > 0)
        {
            VoxelMod_Lobby v = modifications.Dequeue();
            Vector3 pos = v.position - chkPosition; //청크 내부의 값
            voxelMap[(int)pos.x, (int)pos.y, (int)pos.z] = v.id;
        }

        ClearMeshData();

        for (int y = 0; y < VD.ChunkHeight; y++)
        {
            for (int x = 0; x < VD.ChunkWidth; x++)
            {
                for (int z = 0; z < VD.ChunkWidth; z++)
                {
                    if (world.blockTypes[voxelMap[x, y, z]].IsSolid)
                    {
                            UpdateMeshData(new Vector3(x, y, z));
                    }
                }
            }
        }
        CreateMesh();
    }


//메쉬 데이터 클리어
private void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }


    //해당 청크가 켜져있는지 반환하고 껐다 켤수 있음
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

    
    private bool CheckVoxel(Vector3 _pos) // 해당 위치가 솔리드인지 반환
    {
        int x = Mathf.FloorToInt(_pos.x);
        int y = Mathf.FloorToInt(_pos.y);
        int z = Mathf.FloorToInt(_pos.z);

        if (!IsVoxelInChunk(x, y, z))       // 해당 위치가 청크의 범위를 벗어날 경우 
            return world.CheckForVoxel(_pos + chkPosition);

        return world.blockTypes[voxelMap[x, y, z]].IsSolid;
    }


    //해당 위치가 청크 기준으로 가지는 월드 위치의 복셀데이터를 반환 
    public byte GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        return voxelMap[xCheck, yCheck, zCheck];
    }


    //해당좌표기준 6개의 면 업데이트 
    private void UpdateMeshData(Vector3 _pos)
    {
        for (int p = 0; p < 6; p++)
        {
            if (!CheckVoxel(_pos + VD.faceChecks[p]))
            {
                byte blockID = voxelMap[(int)_pos.x, (int)_pos.y, (int)_pos.z];
                vertices.Add(_pos + VD.voxelVerts[VD.voxelTris[p, 0]]);
                vertices.Add(_pos + VD.voxelVerts[VD.voxelTris[p, 1]]);
                vertices.Add(_pos + VD.voxelVerts[VD.voxelTris[p, 2]]);
                vertices.Add(_pos + VD.voxelVerts[VD.voxelTris[p, 3]]);

                AddTexture(world.blockTypes[blockID].GetTextureID(p));
                
                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                vertexIndex += 4; // 건너가는 간격 
                
            }
        }
    }

    //청크의 매쉬 그려줌 
    private void CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        
        mesh.RecalculateNormals();
        meshFilter.mesh = mesh;
        if(!meshFilter.gameObject.GetComponent<MeshCollider>())
        {
            meshFilter.gameObject.AddComponent<MeshCollider>();
        }
    }


    private void AddTexture(int _textureID)
    {
        float y = _textureID / VD.TextureAtlasSizeInBlocks;
        float x = _textureID - (y * VD.TextureAtlasSizeInBlocks);

        x *= VD.NormalizeBlockTextureSize;
        y *= VD.NormalizeBlockTextureSize;

        //y = 1f - y - VD.NormalizeBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VD.NormalizeBlockTextureSize));
        uvs.Add(new Vector2(x + VD.NormalizeBlockTextureSize, y));
        uvs.Add(new Vector2(x + VD.NormalizeBlockTextureSize, y + VD.NormalizeBlockTextureSize));

    }


}

//청크의 좌표 클래스 : 청크좌표는 2차원으로 가지고 있음
public class ChkCoord
{
    public int x;
    public int z;

    //생성자로 초기화
    public ChkCoord()
    {
        x = 0;
        z = 0;
    }

    //오버로드 함수들

    //각 청크의 좌표값 입력 - 각 좌표에 할당
    public ChkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    //float인 벡터값을 받아서 청크의 위치를 지정 
    public ChkCoord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VD.ChunkWidth;
        z = zCheck / VD.ChunkWidth;
    }

    //매개변수의 청크의 xy정수좌표를 현재 청크와 비교해서 bool 반환 
    public bool Equals(ChkCoord other)
    {
        if (other == null)
            return false;
        else if (other.x == x && other.z == z)
            return true;
        else
            return false;
    }
}
