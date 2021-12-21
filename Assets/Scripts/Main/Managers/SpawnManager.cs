using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnManager : MonoBehaviour
{
    #region variables

    private int spawnNum = 500;
    private int monsterTypeNum = 0;
    private Vector3 spawnPosition = Vector3.zero;
    [SerializeField]
    private GameObject[] monsterTypeAry = new GameObject[15];
    [SerializeField]

    public GameObject boss = null;
    public Vector3 bossSpawnPos = Vector3.zero;
    [SerializeField]
    private Text bossDistText = null;

    public List<Monster> monsterList = new List<Monster>();
    public List<GameObject> hpBarList = new List<GameObject>();
    
    private Player player = null;

    #endregion

    private void Awake()
    {
        SpawnMonster();
        player = FindObjectOfType<Player>();
    }

    private void Start()
    {
        GameObject hpBars = FindObjectOfType<Canvas>().transform.Find("HpBars").gameObject;
    }
    private void Update()
    {
        ViewDistanceCulling();
    }
    public enum EMonsterType
    {
        Cactus,
        CuteMushroom,
        Dragon,
        Littleboar,
        Magictree,
        Minimonsters,
        Momo,
        MonsterFlower,
        Penguin,
        Sheep,
        Snake,
        Teruteru,
        TonTon,
    }

    private void SpawnMonster()
    {
        EMonsterType monsterType = EMonsterType.TonTon;
        for(int i=0; i < spawnNum; ++i)
        {
            monsterTypeNum = Random.Range(0, 13);
            spawnPosition = GetRandomPos();
            monsterType = (EMonsterType)monsterTypeNum;
            GameObject myMonster = Instantiate(monsterTypeAry[monsterTypeNum], spawnPosition, Quaternion.identity, transform);
            myMonster.GetComponentInChildren<Monster>().monsterType = monsterTypeNum;
            monsterList.Add(myMonster.GetComponentInChildren<Monster>());
        }

        spawnPosition = GetRandomPos();
        bossSpawnPos = spawnPosition;
        GameObject myBoss = Instantiate(boss, spawnPosition, Quaternion.identity, transform);
        myBoss.name = "Boss";
        boss = myBoss;
    }

    private Vector3 GetRandomPos()
    {
        Vector3 ranPos = new Vector3(Random.Range(VoxelData.ChunkWidth, (VoxelData.WorldSizeInChunks - 2) * VoxelData.ChunkWidth),
                                        VoxelData.ChunkHeight - VoxelData.ChunkHeight * 0.3f,
                                        Random.Range(VoxelData.ChunkWidth, (VoxelData.WorldSizeInChunks - 2) * VoxelData.ChunkWidth));
        return ranPos;
    }

    //Cull Monster, hpBar when their position in backside of player
    private void ViewDistanceCulling()
    {
        float detectRange = VoxelData.ChunkWidth * VoxelData.ViewDistanceInChunks * 0.5f;
        for(int i = 0; i < monsterList.Count; ++i)
        {
            //if monster Die, remove monster at list and match hpBar idx; exception
            if (!monsterList[i]) { monsterList.RemoveAt(i); Destroy(hpBarList[i].gameObject) ; hpBarList.RemoveAt(i); continue; }
            
            //Cull out of Distance enemy, backside of player
            float playerMonsterDot = Vector3.Dot(player.transform.forward, player.transform.position - monsterList[i].transform.position);
            float dist = (Vector3.Distance(monsterList[i].transform.position, player.transform.position));

            //Monster LOD
            if (dist >= detectRange)
            {
                monsterList[i].gameObject.SetActive(false);
                hpBarList[i].SetActive(false);
            }
            else if(dist <= detectRange && dist > detectRange * 0.6f)
            {
                monsterList[i].gameObject.SetActive(true);
                monsterList[i].GetComponent<SkinnedMeshRenderer>().forceRenderingOff = true;

                if (playerMonsterDot < 0f)
                {
                    hpBarList[i].SetActive(true);
                }
                else
                    hpBarList[i].SetActive(false);
            }
            else if(dist <= detectRange * 0.6f)
            {
                monsterList[i].gameObject.SetActive(true);
                monsterList[i].GetComponent<SkinnedMeshRenderer>().forceRenderingOff = false;
                hpBarList[i].SetActive(true);
                if(playerMonsterDot >= 0f)
                {
                    monsterList[i].GetComponent<SkinnedMeshRenderer>().forceRenderingOff = true;
                    hpBarList[i].SetActive(false);
                }
            }

        }
        //Boss Activate
        float bossDist = Vector3.Distance(boss.transform.position, player.transform.position);
        if(bossDist >= detectRange)
        {
            boss.SetActive(false);
            bossDistText.gameObject.SetActive(true);
            bossDistText.text = "To Boss : " + ((int)bossDist).ToString("D") + "m";
        }
        else
        {
            bossDistText.gameObject.SetActive(false);
            boss.SetActive(true);
        }
        //bossDistTextUpdate
    }
}
