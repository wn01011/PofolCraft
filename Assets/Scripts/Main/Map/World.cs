using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class World : MonoBehaviour
{
    //private int seed = (int)Time.time;
    public BiomeAttributes biome = null;

    [SerializeField] private GameObject debugScreen = null;

    public Transform player = null;
    public Vector3 spawnPosition = Vector3.zero;
    //  x : 월드사이즈 청크 수 * 청크의 가로세로길이 의 절반 
    //  y : 청크 높이 +2f
    //  z : 월드사이즈 청크 수 * 청크의 가로세로길이 의 절반 

    public Material material = null;
    public Material waterMaterial = null;
    public MeshFilter waterFilter = null;
    public List<GameObject> waters = null;
    [SerializeField]
    private GameObject leafMesh = null;

    public List<GameObject> LeafList = new List<GameObject>();

    public BlockType[] blockTypes = null;

    public Chunk[,] chunks = new Chunk[VoxelData.WorldSizeInChunks, VoxelData.WorldSizeInChunks];

    public List<ChunkCoord> activeChunks = new List<ChunkCoord>();
    public ChunkCoord playerLastChunkCoord = null;
    public ChunkCoord playerChunkCoord = null;

    private List<ChunkCoord> chunksToCreate = new List<ChunkCoord>();
    private List<Chunk> chunksToUpdate = new List<Chunk>();

    //private bool isCreatingChunks;
    private bool applyingModifications = false;

    //청크 구조 수정
    Queue<VoxelMod> modifications = new Queue<VoxelMod>();


    private byte topBlock = 3;
    private byte middleBlock = 5;
    private int pulinOffset = 0;
    private int spawnPosX = 0;
    private int spawnPosZ = 0;
    public void SetBlock(byte _top, byte _middle)
    {
        topBlock = _top;
        middleBlock = _middle;
        pulinOffset = Random.Range(-100, 100);
        spawnPosX = Random.Range(VoxelData.ChunkWidth + 1, ((VoxelData.WorldSizeInChunks - 2) * VoxelData.ChunkWidth) - 1);
        spawnPosZ = Random.Range(VoxelData.ChunkWidth + 1, ((VoxelData.WorldSizeInChunks - 2) * VoxelData.ChunkWidth) - 1);
    }


    private void Start()
    {
        //Random.InitState(seed); // 같은 시드에서 같은 랜덤값 도출 
        StageData.SetStage();
        SetBiome();
        SetBlock(StageData.SetValues.topBlock, StageData.SetValues.middleBlock);
        spawnPosition = new Vector3(spawnPosX,
                                    biome.solidGroundHeight + biome.terrainHeight,
                                    spawnPosZ);

        playerLastChunkCoord = GetChunkCoordFromVector3(player.transform.position);
        playerChunkCoord = playerLastChunkCoord;
        GanerateWorld();
    }

    private void SetBiome()
    {
        biome.solidGroundHeight = VoxelData.ChunkHeight / 3;
        biome.terrainHeight = biome.solidGroundHeight;
    }

    private void Update()
    {
        playerChunkCoord = GetChunkCoordFromVector3(player.transform.position);

        if (!GetChunkCoordFromVector3(player.transform.position).Equals(playerLastChunkCoord))
        {
            CheckViewDistance();
            CheckLeafDistance();
        }
        //if (chunksToCreate.Count > 0 && !isCreatingChunks)
        //    StartCoroutine("CreateChunks");

        if (modifications.Count > 0 && !applyingModifications)
            StartCoroutine(ApplyModifications());

        if (chunksToCreate.Count > 0)
            CreateChunk();

        if (chunksToUpdate.Count > 0)
            UpdateChunks();

        if (Input.GetKeyDown(KeyCode.F3))
            debugScreen.SetActive(!debugScreen.activeSelf);
    }


    ChunkCoord GetChunkCoordFromVector3(Vector3 _pos)
    {
        int x = Mathf.FloorToInt(_pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(_pos.z / VoxelData.ChunkWidth);
        return new ChunkCoord(x, z);
    }

    public Chunk GetChunkFromVector3(Vector3 _pos)
    {
        int x = Mathf.FloorToInt(_pos.x / VoxelData.ChunkWidth);
        int z = Mathf.FloorToInt(_pos.z / VoxelData.ChunkWidth);
        return chunks[x, z];
    }
    private void GanerateWorld()
    {
        for (int x = ((VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks);
            x < ((VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks); x++)
        {
            for (int z = ((VoxelData.WorldSizeInChunks / 2) - VoxelData.ViewDistanceInChunks);
                z < ((VoxelData.WorldSizeInChunks / 2) + VoxelData.ViewDistanceInChunks); z++)
            {
                chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, true);
                activeChunks.Add(new ChunkCoord(x, z));
            }
        }

        while (modifications.Count > 0)
        {
            VoxelMod v = modifications.Dequeue();
            ChunkCoord c = GetChunkCoordFromVector3(v.position);

            if (chunks[c.x, c.z] == null)
            {
                chunks[c.x, c.z] = new Chunk(c, this, true);
                activeChunks.Add(c);
            }
            chunks[c.x, c.z].modificatrions.Enqueue(v);

            if (!chunksToUpdate.Contains(chunks[c.x, c.z]))
            {
                chunksToUpdate.Add(chunks[c.x, c.z]);
            }

        }

        for (int i = 0; i < chunksToUpdate.Count; i++)
        {
            chunksToUpdate[0].UpdateChunk();
            chunksToUpdate.RemoveAt(0);
        }

        player.position = spawnPosition;
        //CheckViewDistance();
    }

    private void CreateChunk()
    {
        ChunkCoord c = chunksToCreate[0];
        chunksToCreate.RemoveAt(0);
        activeChunks.Add(c);
        chunks[c.x, c.z].Init();

    }

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


    private IEnumerator ApplyModifications()
    {
        applyingModifications = true;
        int count = 0;


        while (modifications.Count > 0)
        {
            VoxelMod v = modifications.Dequeue();
            ChunkCoord c = GetChunkCoordFromVector3(v.position);

            if (chunks[c.x, c.z] == null)
            {
                chunks[c.x, c.z] = new Chunk(c, this, true);
                activeChunks.Add(c);
            }
            chunks[c.x, c.z].modificatrions.Enqueue(v);

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


    //private IEnumerator CreateChunks()
    //{
    //    isCreatingChunks = true;

    //    while (chunksToCreate.Count > 0)
    //    {
    //        chunks[chunksToCreate[0].x, chunksToCreate[0].z].Init();
    //        chunksToCreate.RemoveAt(0);
    //        yield return null;
    //    }
    //    isCreatingChunks = false;
    //}


    private void CheckViewDistance()
    {
        ChunkCoord coord = GetChunkCoordFromVector3(player.position);
        playerLastChunkCoord = playerChunkCoord;


        List<ChunkCoord> previouslyActiveChunks = new List<ChunkCoord>(activeChunks);

        for (int x = coord.x - VoxelData.ViewDistanceInChunks / 2; x < coord.x + VoxelData.ViewDistanceInChunks / 2; x++)
        {
            for (int z = coord.z - VoxelData.ViewDistanceInChunks / 2; z < coord.z + VoxelData.ViewDistanceInChunks / 2; z++)
            {
                if (IsChunkInWorld(new ChunkCoord(x, z)))
                {
                    ChunkCoord thisChunk = new ChunkCoord(x, z);

                    if (chunks[x, z] == null)
                    {
                        chunks[x, z] = new Chunk(new ChunkCoord(x, z), this, false);
                        chunksToCreate.Add(new ChunkCoord(x, z));
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
        foreach (ChunkCoord coords in previouslyActiveChunks)
        {
            chunks[coords.x, coords.z].isActive = false;
        }
    }
    private void CheckLeafDistance()
    {
        foreach (GameObject leaf in LeafList)
        {
            if ((leaf.transform.position  - player.position).magnitude <= VoxelData.WorldSizeInChunks)
                leaf.SetActive(true);
            else
                leaf.SetActive(false);
        }
    }

    public bool CheckForVoxel(Vector3 pos)
    {
        ChunkCoord thisChunk = new ChunkCoord(pos);

        if (!IsChunkInWorld(thisChunk) || pos.y < 0 || pos.y > VoxelData.ChunkHeight)
            return false;

        if (chunks[thisChunk.x, thisChunk.z] != null && chunks[thisChunk.x, thisChunk.z].isVoxelmapPopulated)
            return blockTypes[chunks[thisChunk.x, thisChunk.z].GetVoxelFromGlobalVector3(pos)].IsSolid;

        return blockTypes[GetVoxel(pos)].IsSolid;
    }





    public byte GetVoxel(Vector3 _pos)
    {
        int yPos = Mathf.FloorToInt(_pos.y);


        if (!IsVoxelInWorld(_pos))
            return 0;
        if (yPos == 0)
            return 1;
        // basic terrain pass
        int terrainHeight = Mathf.FloorToInt(biome.terrainHeight * Noise.Get2DPerlin(new Vector2(_pos.x, _pos.z), pulinOffset, biome.terrainScale)) + biome.solidGroundHeight;

        byte voxelValue = 0;

        if (yPos == terrainHeight)
            voxelValue = topBlock; //3
        else if (yPos < terrainHeight && yPos > terrainHeight - 4)
            voxelValue = middleBlock;//5;
        else if (yPos > terrainHeight)
            return 0;
        else
            voxelValue = 2;


        //2nd terrain pass

        if (voxelValue == 2)
        {
            foreach (Lode lode in biome.lodes)
            {
                if (yPos > lode.minHeight && yPos < lode.maxHeight)
                {
                    if (Noise.Get3DPerlin(_pos, lode.nouseOffset, lode.scale, lode.threshold))
                    {
                        voxelValue = lode.blockID;
                    }
                }
            }
        }

        //3rd terrain pass trees
        if (yPos == terrainHeight)
        {
            if (Noise.Get2DPerlin(new Vector2(_pos.x, _pos.z), 0, biome.treeZoneScale) > biome.treeZoneThreshold)
            {
                //voxelValue = 1;
                if (Noise.Get2DPerlin(new Vector2(_pos.x, _pos.z), 0, biome.treePlacementScale) > biome.treePlacemantThreshold)
                {
                    voxelValue = 6;
                    Structure.MakeTree(_pos, modifications, biome.minTreeHeight, biome.maxTreeHeight);
                    //modifications.Enqueue(new VoxelMod(new Vector3(_pos.x, _pos.y + 1, _pos.z),6));
                    MakeLeaf(_pos);

                }
            }
        }
        return voxelValue;
    }
    private void MakeLeaf(Vector3 _pos)
    {
        float RandomSize = Random.Range(0.8f, 2f);
        GameObject myLeaf = Instantiate(leafMesh, Vector3.zero, Quaternion.identity, transform.Find("Leafs").transform);
        myLeaf.transform.localScale *= RandomSize;
        myLeaf.transform.position = _pos + new Vector3(leafMesh.transform.localScale.x * 0.5f, biome.minTreeHeight, leafMesh.transform.localScale.z * 0.5f);
        LeafList.Add(myLeaf);
        myLeaf.SetActive(true);
    }
    private bool IsChunkInWorld(ChunkCoord coord)
    {
        if (coord.x > 0 && coord.x < VoxelData.WorldSizeInChunks - 1 &&
            coord.z > 0 && coord.z < VoxelData.WorldSizeInChunks - 1)
        {
            return true;
        }
        else
            return false;
    }
    bool IsVoxelInWorld(Vector3 pos)
    {
        if (pos.x >= 0 && pos.x < VoxelData.WorldSizeInVoxels &&
           pos.y >= 0 && pos.y < VoxelData.ChunkHeight &&
           pos.z >= 0 && pos.z < VoxelData.WorldSizeInVoxels)
        {
            return true;
        }
        else
            return false;
    }

}





[System.Serializable]
public class BlockType
{
    public string blockName;
    public bool IsSolid;
    public int hp;

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

public class VoxelMod
{
    public Vector3 position;
    public byte id;
    public VoxelMod()
    {
        position = new Vector3();
        id = 0;

    }
    public VoxelMod(Vector3 _position, byte _id)
    {
        position = _position;
        id = _id;

    }

}