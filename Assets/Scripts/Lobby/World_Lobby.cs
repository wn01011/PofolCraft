using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World_Lobby : MonoBehaviour
{
    //private int seed = (int)Time.time;
    public BiomeAttributes biome = null;

    [SerializeField] private BuildingManager buildingMg = null;
    [SerializeField] private NpcManager npcMg = null;
    public List<Vector3> treePos = new List<Vector3>();
    

    public Transform player = null;
    public Vector3 spawnPosition = Vector3.zero;
    //  x : ��������� ûũ �� * ûũ�� ���μ��α��� �� ���� 
    //  y : ûũ ���� +2f
    //  z : ��������� ûũ �� * ûũ�� ���μ��α��� �� ���� 

    public Material material = null;
   
    [SerializeField]
    private GameObject leafMesh = null;

    public List<GameObject> LeafList = new List<GameObject>();
    public List<GameObject> treeList = new List<GameObject>();


    public BlockType_Lobby[] blockTypes = null;

    public Chk[,] chunks = new Chk[VD.WorldSizeInChunks, VD.WorldSizeInChunks];

    private List<ChkCoord> activeChunks = new List<ChkCoord>();
    public ChkCoord playerLastChunkCoord = null;
    public ChkCoord playerChunkCoord = null;

    private List<ChkCoord> chunksToCreate = new List<ChkCoord>();
    private List<Chk> chunksToUpdate = new List<Chk>();

    //private bool isCreatingChunks;
    private bool applyingModifications = false;

    //ûũ ���� ����
    Queue<VoxelMod_Lobby> modifications = new Queue<VoxelMod_Lobby>();


    private byte topBlock = 0;
    private byte middleBlock = 0;
    private int spawnPosX = 0;
    private int spawnPosZ = 0;



    public void SetBlock(byte _top, byte _middle)
    {
        topBlock = _top;
        middleBlock = _middle;
        spawnPosX = VD.WorldSizeInVoxels / 2;
        //    Random.Range(VD.ChunkWidth + 1, ((VD.WorldSizeInChunks - 2) * VD.ChunkWidth) - 1);
        spawnPosZ = VD.WorldSizeInVoxels / 2;
            //Random.Range(VD.ChunkWidth + 1, ((VD.WorldSizeInChunks - 2) * VD.ChunkWidth) - 1);
    }

    private void Start()
    {
        SetBlock(3, 5);
        spawnPosition = new Vector3(spawnPosX, VD.ChunkHeight, spawnPosZ);

        playerLastChunkCoord = GetChunkCoordFromVector3(player.transform.position);
        playerChunkCoord = playerLastChunkCoord;

        buildingMg.AddFuncToBuildingMg(GetChunkFromVector3);
        GanerateWorld();
        buildingMg.SetBuildings(3);
        SetWall();
        npcMg.AddFuncToNPCMg(CheckForVoxel, buildingMg.GetComponent<BuildingManager>);
        npcMg.NpcInit();
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.transform.position);

        CheckViewDistance();
        CheckLeafDistance();

        if (modifications.Count > 0 && !applyingModifications)
            StartCoroutine(ApplyModifications());

        if (chunksToCreate.Count > 0)
            CreateChunk();

        if (chunksToUpdate.Count > 0)
            UpdateChunks();
               
    }

    //ChkCoord : ûũ�� ��ǥ(����)�� ������ Ŭ����
    //������� ��ǥ�� �޾Ƽ� �ش� ��ġ�� ûũ�� ûũ�� ��ġ�� ����(2����)
    ChkCoord GetChunkCoordFromVector3(Vector3 _pos)
    {
        int x = Mathf.FloorToInt(_pos.x / VD.ChunkWidth);
        int z = Mathf.FloorToInt(_pos.z / VD.ChunkWidth);
        return new ChkCoord(x, z);
    }
    
    //������� ��ǥ�� �޾Ƽ� �ش� ��ġ�� �ִ� ûũ�� ��ȯ��
    public Chk GetChunkFromVector3(Vector3 _pos)
    {
        int x = Mathf.FloorToInt(_pos.x / VD.ChunkWidth);
        int z = Mathf.FloorToInt(_pos.z / VD.ChunkWidth);
        return chunks[x, z];
    }

    //���� �� ���� ���� 
    private void GanerateWorld()
    {
        //�ܼ� ûũ ����� ��Ƽ��ûũ����Ʈ�� ûũ ��ġ ���
        for (int x = 1; x < VD.WorldSizeInChunks-1; x++)
        {
            for (int z = 1; z < VD.WorldSizeInChunks-1; z++)
            {
                chunks[x, z] = new Chk(new ChkCoord(x, z), this, true);
                activeChunks.Add(chunks[x, z].coord);
            }
        }

        //������ ûũ�� ���� �� �۵� 
        //����� ��ǥ�� ûũ�� ��������� ���� �����ϰ� ��Ƽ�꿡 ����
        while (modifications.Count > 0)
        {
            VoxelMod_Lobby v = modifications.Dequeue();
            ChkCoord c = GetChunkCoordFromVector3(v.position);

            if (chunks[c.x, c.z] == null)
            {
                chunks[c.x, c.z] = new Chk(c, this, true);
                activeChunks.Add(c);
            }
            //���� ������ ûũ�� �ٽ� ��� ���� 
            chunks[c.x, c.z].modifications.Enqueue(v);

            //������Ʈ�� �� ûũ����Ʈ�� ��������� �ش� ûũ�� ����
            if (!chunksToUpdate.Contains(chunks[c.x, c.z]))
            {
                chunksToUpdate.Add(chunks[c.x, c.z]);
            }
        }

        //������Ʈ�� �� ûũ����Ʈ�� ��ü�� ���鼭 
        //�ش� ûũ�� ������Ʈ�ϰ� ����Ʈ���� ���� 
        for (int i = 0; i < chunksToUpdate.Count; i++)
        {
            chunksToUpdate[0].UpdateChunk();
            chunksToUpdate.RemoveAt(0);
        }

        player.position = spawnPosition;
    }


    //ûũ ����
    private void CreateChunk()
    {
        ChkCoord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        activeChunks.Add(c);
        chunks[c.x, c.z].Init();
    }


    //������Ʈ ���ɼ��� �ִ� ûũ�� ������Ʈ �� 
    private void UpdateChunks()
    {
        bool updated = false;
        int index = 0;

        while (!updated && index < chunksToUpdate.Count - 1)
        {
            if (chunksToUpdate[index].isVoxelmapPopulated)
            {
                chunksToUpdate[index].UpdateChunk();
                chunksToUpdate.RemoveAt(index);
                updated = true;
            }
            else
            {
                index++;
            }
        }
    }

    // �����ؾ��ϴ� ���� ��ǥ�� ���÷� �����ϰ� �����ϸ鼭 ���� �ݺ���
    private IEnumerator ApplyModifications()
    {
        applyingModifications = true;
        int count = 0;


        while (modifications.Count > 0)
        {
            VoxelMod_Lobby v = modifications.Dequeue();
            ChkCoord c = GetChunkCoordFromVector3(v.position);

            if (chunks[c.x, c.z] == null)
            {
                chunks[c.x, c.z] = new Chk(c, this, true);
                activeChunks.Add(c);
            }
            chunks[c.x, c.z].modifications.Enqueue(v);

            if (!chunksToUpdate.Contains(chunks[c.x, c.z]))
            {
                chunksToUpdate.Add(chunks[c.x, c.z]);
            }

            count++;
            if (count > 200)
            {
                count = 0;
                yield return null;
            }
        }
        applyingModifications = false;
    }

    //�÷��̾��� ���� ��ġ�� ������� ûũ�� ��ġ�� ���ؼ� �ֺ� ûũ�� Ȱ��ȭ��
    private void CheckViewDistance()
    {
        ChkCoord coord = GetChunkCoordFromVector3(new Vector3(0.5f,0.5f,0.5f));
        playerLastChunkCoord = playerChunkCoord;


        List<ChkCoord> previouslyActiveChunks = new List<ChkCoord>(activeChunks);

        for (int x = coord.x ; x < VD.WorldSizeInChunks; x++)
        {
            for (int z = coord.z; z < VD.WorldSizeInChunks; z++)
            {
                if (IsChunkInWorld(new ChkCoord(x, z)))
                {
                    ChkCoord thisChunk = new ChkCoord(x, z);

                    if (chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chk(new ChkCoord(x, z), this, false);
                        chunksToCreate.Add(chunks[x, z].coord);
                    }
                    else if (!chunks[x, z].isActive)
                    {
                        chunks[x, z].isActive = true;
                        activeChunks.Add(thisChunk);
                    }
                }



                for (int i = 0; i < previouslyActiveChunks.Count; i++)
                {
                    if (previouslyActiveChunks[i].x == x && previouslyActiveChunks[i].z == z)
                    {
                        previouslyActiveChunks.RemoveAt(i);
                    }
                }
            }
        }
        foreach (ChkCoord coords in previouslyActiveChunks)
        {
            chunks[coords.x, coords.z].isActive = false;
        }
    }

    //Ǯ���� �Ÿ��� ���� ���� Ŵ
    private void CheckLeafDistance()
    {
        foreach (GameObject leaf in LeafList)
        {
            if ((leaf.transform.position  - player.position).magnitude <= VD.WorldSizeInChunks)
                leaf.SetActive(true);
            else
                leaf.SetActive(false);
        }
    }

    //�ش� ��ǥ�� ûũ �������� ���� �˻��ϰ�, 
    //�ش� ��ġ�� ûũ�� �����ϸ鼭 ������ �����ϸ� �ش� ��ġ�� �ָ������� ��ȯ 
    //��ġ�� ���� �����̸� �ָ��� ��ȯ
    public bool CheckForVoxel(Vector3 pos)
    {
        ChkCoord thisChunk = new ChkCoord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VD.ChunkHeight)
            return false;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isVoxelmapPopulated)
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].IsSolid;

        return blockTypes[GetVoxel(pos)].IsSolid;
    }

    //������ ��ġ�� ������ ������ ä�� ���� 
    private void SetWall()
    {
        int y = 11;
        int x = 0;
        int z = 0;

        for (z = 4; z <= 43; z += 39)
        {
            for (y = 11; y > 0; y--)
            {
                for (x = 5; x < 43; x++)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    GetChunkFromVector3(pos).EditVoxel(pos, 1);
                }
            }
        }
        
        for(x = 4; x <= 43; x+= 39)
        {
            for (y = 11; y > 0 ; y--)
            {
                for(z = 5; z < 43; z++)
                {
                    Vector3 pos = new Vector3(x, y, z);
                    GetChunkFromVector3(pos).EditVoxel(pos, 1);
                }
            }
        }
    }



    //�ش���ġ�� ������ ���� id�� ��ȯ 
    public byte GetVoxel(Vector3 _pos)
    {
        int yPos = Mathf.FloorToInt(_pos.y);

        //�Ű����� �ڸ��� ���� ���ΰ� �ƴϸ� 0 ��ȯ
        if (!IsVoxelInWorld(_pos))
            return 0;
        
        //�Ű����� �ڸ��� ���̰� 0�̸� 1��ȯ -> ����
        if (yPos == 0)
            return 1;
        
        // basic terrain pass
        //������ ���̸� �������� �����̿� ���� �� ���� ��ġ
        int terrainHeight = 10;
        byte voxelValue = 0;

        if (yPos == terrainHeight)
            voxelValue = topBlock; //3 ǥ�� 1ĭ 
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
            voxelValue = middleBlock;//5; �׿� �߰�ĭ ��
        else if (yPos > terrainHeight) // ����ĭ�� ���κ� ���� -> �ƹ��͵� ���� 
            return 0;
        else
            voxelValue = 2;//��
        


        
        return voxelValue;
    }

  

    

    //�ش� ��ġ�� ������ ����
    

    //ûũ���� ��ǥ�� ���忡 ���ԵǴ��� �˻� : ���ԵǸ� true
    private bool IsChunkInWorld(ChkCoord coord)
    {
        if (coord.x > 0 && coord.x < VD.WorldSizeInChunks - 1 &&
            coord.z > 0 && coord.z < VD.WorldSizeInChunks - 1)
        {
            return true;
        }
        else
            return false;
    }
    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VD.WorldSizeInVoxels &&
           pos.y >= 0 && pos.y < VD.ChunkHeight &&
           pos.z >= 0 && pos.z < VD.WorldSizeInVoxels)
        {
            return true;
        }
        else
            return false;
    }

}





//��(����)�� Ÿ���� ������ Ŭ���� 
[System.Serializable]
public class BlockType_Lobby
{
    public string blockName;
    public bool IsSolid;

    [Header("Texture Values")]
    public int backFaceTexture;
    public int frontFaceTexture;
    public int topFaceTexture;
    public int bottomFaceTexture;
    public int leftFaceTexture;
    public int rightFaceTexture;
    public int GetTextureID(int _faceIdx)
    {
        switch (_faceIdx)
        {
            case 0:
                return backFaceTexture;
            case 1:
                return frontFaceTexture;
            case 2:
                return topFaceTexture;
            case 3:
                return bottomFaceTexture;
            case 4:
                return leftFaceTexture;
            case 5:
                return rightFaceTexture;
            default:
                Debug.Log("Error in GetTextureID, invaild face index");
                return 0;

        }
    }
}

//������ ������ ������ Ŭ����
public class VoxelMod_Lobby
{
    public Vector3 position;
    public byte id;
    public VoxelMod_Lobby()
    {
        position = new Vector3();
        id = 0;
    }
    public VoxelMod_Lobby(Vector3 _position, byte _id)
    {
        position = _position;
        id = _id;
    }

}