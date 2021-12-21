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
    //  x : 월드사이즈 청크 수 * 청크의 가로세로길이 의 절반 
    //  y : 청크 높이 +2f
    //  z : 월드사이즈 청크 수 * 청크의 가로세로길이 의 절반 

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

    //청크 구조 수정
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

    //ChkCoord : 청크의 좌표(순서)만 가지는 클래스
    //월드기준 좌표를 받아서 해당 위치의 청크의 청크간 위치를 구함(2차원)
    ChkCoord GetChunkCoordFromVector3(Vector3 _pos)
    {
        int x = Mathf.FloorToInt(_pos.x / VD.ChunkWidth);
        int z = Mathf.FloorToInt(_pos.z / VD.ChunkWidth);
        return new ChkCoord(x, z);
    }
    
    //월드기준 좌표를 받아서 해당 위치에 있는 청크를 반환함
    public Chk GetChunkFromVector3(Vector3 _pos)
    {
        int x = Mathf.FloorToInt(_pos.x / VD.ChunkWidth);
        int z = Mathf.FloorToInt(_pos.z / VD.ChunkWidth);
        return chunks[x, z];
    }

    //시작 시 월드 생성 
    private void GanerateWorld()
    {
        //단순 청크 만들고 액티브청크리스트에 청크 위치 기록
        for (int x = 1; x < VD.WorldSizeInChunks-1; x++)
        {
            for (int z = 1; z < VD.WorldSizeInChunks-1; z++)
            {
                chunks[x, z] = new Chk(new ChkCoord(x, z), this, true);
                activeChunks.Add(chunks[x, z].coord);
            }
        }

        //수정할 청크가 있을 때 작동 
        //모디의 좌표의 청크가 비어있으면 새로 생성하고 액티브에 넣음
        while (modifications.Count > 0)
        {
            VoxelMod_Lobby v = modifications.Dequeue();
            ChkCoord c = GetChunkCoordFromVector3(v.position);

            if (chunks[c.x, c.z] == null)
            {
                chunks[c.x, c.z] = new Chk(c, this, true);
                activeChunks.Add(c);
            }
            //넣은 수정한 청크는 다시 모디에 넣음 
            chunks[c.x, c.z].modifications.Enqueue(v);

            //업데이트를 할 청크리스트가 비어있으면 해당 청크를 넣음
            if (!chunksToUpdate.Contains(chunks[c.x, c.z]))
            {
                chunksToUpdate.Add(chunks[c.x, c.z]);
            }
        }

        //업데이트를 할 청크리스트를 전체를 돌면서 
        //해당 청크를 업데이트하고 리스트에서 지움 
        for (int i = 0; i < chunksToUpdate.Count; i++)
        {
            chunksToUpdate[0].UpdateChunk();
            chunksToUpdate.RemoveAt(0);
        }

        player.position = spawnPosition;
    }


    //청크 생성
    private void CreateChunk()
    {
        ChkCoord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        activeChunks.Add(c);
        chunks[c.x, c.z].Init();
    }


    //업데이트 가능성이 있는 청크를 업데이트 함 
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

    // 수정해야하는 복셀 좌표를 수시로 저장하고 수정하면서 뱉어냄을 반복함
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

    //플레이어의 현재 위치를 기반으로 청크의 위치를 구해서 주변 청크를 활성화함
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

    //풀입을 거리에 따라 껐다 킴
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

    //해당 좌표가 청크 내부인지 먼저 검사하고, 
    //해당 위치에 청크가 존재하면서 복셀이 존재하면 해당 위치가 솔리드인지 반환 
    //위치가 복셀 내부이면 솔리드 반환
    public bool CheckForVoxel(Vector3 pos)
    {
        ChkCoord thisChunk = new ChkCoord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VD.ChunkHeight)
            return false;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isVoxelmapPopulated)
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].IsSolid;

        return blockTypes[GetVoxel(pos)].IsSolid;
    }

    //지정된 위치에 임의의 복셀을 채워 넣음 
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



    //해당위치의 복셀에 넣을 id값 반환 
    public byte GetVoxel(Vector3 _pos)
    {
        int yPos = Mathf.FloorToInt(_pos.y);

        //매개변수 자리가 월드 내부가 아니면 0 반환
        if (!IsVoxelInWorld(_pos))
            return 0;
        
        //매개변수 자리의 높이가 0이면 1반환 -> 배드락
        if (yPos == 0)
            return 1;
        
        // basic terrain pass
        //지정한 높이를 기준으로 높낮이에 따라 블럭 종류 배치
        int terrainHeight = 10;
        byte voxelValue = 0;

        if (yPos == terrainHeight)
            voxelValue = topBlock; //3 표면 1칸 
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
            voxelValue = middleBlock;//5; 그외 중간칸 흙
        else if (yPos > terrainHeight) // 기준칸의 윗부분 공기 -> 아무것도 없음 
            return 0;
        else
            voxelValue = 2;//돌
        


        
        return voxelValue;
    }

  

    

    //해당 위치에 나무잎 생성
    

    //청크기준 좌표가 월드에 포함되는지 검사 : 포함되면 true
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





//블럭(복셀)의 타입을 가지는 클래스 
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

//복셀의 정보를 가지는 클래스
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