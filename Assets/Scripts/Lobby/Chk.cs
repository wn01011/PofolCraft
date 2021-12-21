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

    //������ 
    public Chk(ChkCoord _coord, World_Lobby _world, bool generateOnLoad)
    {
        coord = _coord;
        world = _world;
        isActive = true;
        buildingMg = GameObject.Find("BuildingManager").GetComponent<BuildingManager>();

        if (generateOnLoad)
            Init();
    }

    //ûũ�� �����ɶ����� �ʱ�ȭ �� 
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

    //ûũ�� ������ ��ȯ 
    public Vector3 chkPosition
    {
        get { return chunkObject.transform.position; }
    }

    //ûũ�� ���� ��ǥ �������� ûũ���ο� �ִ��� �˻� / ���θ� true ��ȯ
    private bool IsVoxelInChunk(int _x, int _y, int _z)
    {
        if (_x < 0 || _x > VD.ChunkWidth - 1 ||
          _y < 0 || _y > VD.ChunkHeight - 1 ||
           _z < 0 || _z > VD.ChunkWidth - 1)
            return false;
        else
            return true;
    }

    //���� ��ġ�� ����Ʈ ID ���� �޾Ƽ� �ش� ��ġ�� ���� ����Ʈ ID�� �����ϰ�, �ֺ� ���� �� ûũ ������Ʈ
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


    //���� ��ǥ�� �� �鿡 +1 �� �� ��ġ�� ������ ����ִ����� �Ǵ��ϸ� ������Ʈ�� 
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

    //������ ���̸� �������� ������ ���� ��ġ�ϰ�, ûũ ���� �������� true�� ������
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

    //��������̼� : �����ؾ��ϴ� ������ ��ǥ�� Ŭ������ ��Ƴ��� ť 
    //������� : ��ġ���� id ����  ������ Ŭ���� 
    public void UpdateChunk()
    {
        while (modifications.Count > 0)
        {
            VoxelMod_Lobby v = modifications.Dequeue();
            Vector3 pos = v.position - chkPosition; //ûũ ������ ��
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


//�޽� ������ Ŭ����
private void ClearMeshData()
    {
        vertexIndex = 0;
        vertices.Clear();
        triangles.Clear();
        uvs.Clear();
    }


    //�ش� ûũ�� �����ִ��� ��ȯ�ϰ� ���� �Ӽ� ����
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

    
    private bool CheckVoxel(Vector3 _pos) // �ش� ��ġ�� �ָ������� ��ȯ
    {
        int x = Mathf.FloorToInt(_pos.x);
        int y = Mathf.FloorToInt(_pos.y);
        int z = Mathf.FloorToInt(_pos.z);

        if (!IsVoxelInChunk(x, y, z))       // �ش� ��ġ�� ûũ�� ������ ��� ��� 
            return world.CheckForVoxel(_pos + chkPosition);

        return world.blockTypes[voxelMap[x, y, z]].IsSolid;
    }


    //�ش� ��ġ�� ûũ �������� ������ ���� ��ġ�� ���������͸� ��ȯ 
    public byte GetVoxelFromGlobalVector3(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int yCheck = Mathf.FloorToInt(pos.y);
        int zCheck = Mathf.FloorToInt(pos.z);

        xCheck -= Mathf.FloorToInt(chunkObject.transform.position.x);
        zCheck -= Mathf.FloorToInt(chunkObject.transform.position.z);

        return voxelMap[xCheck, yCheck, zCheck];
    }


    //�ش���ǥ���� 6���� �� ������Ʈ 
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
                vertexIndex += 4; // �ǳʰ��� ���� 
                
            }
        }
    }

    //ûũ�� �Ž� �׷��� 
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

//ûũ�� ��ǥ Ŭ���� : ûũ��ǥ�� 2�������� ������ ����
public class ChkCoord
{
    public int x;
    public int z;

    //�����ڷ� �ʱ�ȭ
    public ChkCoord()
    {
        x = 0;
        z = 0;
    }

    //�����ε� �Լ���

    //�� ûũ�� ��ǥ�� �Է� - �� ��ǥ�� �Ҵ�
    public ChkCoord(int _x, int _z)
    {
        x = _x;
        z = _z;
    }

    //float�� ���Ͱ��� �޾Ƽ� ûũ�� ��ġ�� ���� 
    public ChkCoord(Vector3 pos)
    {
        int xCheck = Mathf.FloorToInt(pos.x);
        int zCheck = Mathf.FloorToInt(pos.z);

        x = xCheck / VD.ChunkWidth;
        z = zCheck / VD.ChunkWidth;
    }

    //�Ű������� ûũ�� xy������ǥ�� ���� ûũ�� ���ؼ� bool ��ȯ 
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
