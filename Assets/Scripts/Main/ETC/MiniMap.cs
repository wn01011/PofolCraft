using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniMap : MonoBehaviour
{
    #region variables

    private float viewDistance = 0f;
    private GameObject miniEnemy = null;
    private GameObject miniBoss = null;
    public List<GameObject> enemies = new List<GameObject>();
    public List<GameObject> miniEnemies = new List<GameObject>();
    private SpawnManager spawnManager = null;
    private Player player = null;
    private Boss boss = null;
    private GameObject rotateFocus = null;

    #endregion

    void Start()
    {
        #region Initialization

        spawnManager = FindObjectOfType<SpawnManager>();
        player = FindObjectOfType<Player>();
        boss = FindObjectOfType<Boss>();
        rotateFocus = transform.GetChild(0).gameObject;

        miniEnemy = Resources.Load<GameObject>("Prefabs/MiniMap/MiniImage");
        miniBoss = Resources.Load<GameObject>("Prefabs/MiniMap/BossIcon");

        //Copy to spawnManager.monsterList<Monster> to enemiesList<GameObject>
        for(int i=0; i< spawnManager.monsterList.Count; ++i)
        {
            enemies.Add(spawnManager.monsterList[i].gameObject);
        }
        //Instantiate miniMap Icons by (enemies.Count)
        for(int i=0; i<enemies.Count; ++i)
        {
            GameObject myMini = Instantiate(miniEnemy);
            //rotate focus
            myMini.transform.SetParent(transform.GetChild(0));
            miniEnemies.Add(myMini);
        }
        miniBoss = Instantiate(miniBoss);
        miniBoss.transform.SetParent(transform.GetChild(0));
        viewDistance = VoxelData.ChunkWidth * VoxelData.ViewDistanceInChunks * 2;

        #endregion
    }

    void Update()
    {
        Positioning();
        RotateFocus();
    }
    
    //Icon Positioning In Screen, sync to world enem pos
    private void Positioning()
    {
        float width = gameObject.GetComponent<RectTransform>().sizeDelta.x;
        float height = gameObject.GetComponent<RectTransform>().sizeDelta.y;
        float ratioW = width / (viewDistance);
        float ratioH = height / (viewDistance);
        for(int i=0; i<miniEnemies.Count; ++i)
        {
            if (!enemies[i]) 
            {
                miniEnemies.RemoveAt(i);
                Destroy(miniEnemies[i]);
                continue;
            }
            

            miniEnemies[i].GetComponent<RectTransform>().anchoredPosition
                = new Vector2(ratioW * (enemies[i].transform.position.x - player.transform.position.x)
                            , ratioH * (enemies[i].transform.position.z - player.transform.position.z));
            if(Vector2.Distance(miniEnemies[i].GetComponent<RectTransform>().anchoredPosition, Vector2.zero) >= 100f)
            {
                miniEnemies[i].SetActive(false);
            }
            else
            {
                miniEnemies[i].SetActive(true);
            }
        }

        if (!boss)
        {
            Destroy(miniBoss);
            return;
        }
        miniBoss.GetComponent<RectTransform>().anchoredPosition
            = new Vector2(ratioW * (spawnManager.bossSpawnPos.x - player.transform.position.x), ratioH * (spawnManager.bossSpawnPos.z - player.transform.position.z));
        if(Vector2.Distance(miniBoss.GetComponent<RectTransform>().anchoredPosition, Vector2.zero) >= 100f)
        {
            //distance 100 / sqr2(=1.414f) = 70.72f; => calculate distance circular rim from focus
            miniBoss.GetComponent<RectTransform>().anchoredPosition = new Vector2(Mathf.Clamp(ratioW * (spawnManager.bossSpawnPos.x - player.transform.position.x), -70.72f, 70.72f), Mathf.Clamp(ratioH * (spawnManager.bossSpawnPos.z - player.transform.position.z), -70.72f, 70.72f));
        }
    }

    //Rotate when player y angle rotate
    private void RotateFocus()
    {
        rotateFocus.transform.rotation = Quaternion.Euler(0f, 0f, player.transform.rotation.eulerAngles.y);
        if(miniBoss)
            miniBoss.transform.rotation = Quaternion.identity;
    }
}
