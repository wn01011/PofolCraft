using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestManager : MonoBehaviour
{
    //Bit-Flag
    //int 자료형 하나에 questClear 여부를 저장할 수 있어 저장을 효율적으로 할 수있다.
    // | : 둘 중 하나라도 참이면 참, & 둘 다 참이면 참, ^ 두개가 서로 다르면 참
    private enum EQuest { quest0 = 1, quest1 = 2, quest2 = 4, quest3 = 8, quest4 = 16 }

    private enum EMonstertype
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

    #region variables
    [SerializeField]
    private Text questText = null;

    private int curQuestMon = 0;
    private int curQuestMon2 = 0;
    private int curQuestSize = 0;
    private EQuest curQuestNum = EQuest.quest0;
    private GameObject coin = null;
    private Player player = null;
    #endregion

    private void Start()
    {
        coin = Resources.Load<GameObject>("Prefabs/Coin/Coin Gold");
        player = FindObjectOfType<Player>();

        SetMonsterNum();
        StartCoroutine(WaitUntilClearCo());
    }

    #region Set Quest Numbers (set monster type, quest progress using mainData.questClear & mainData.questProgress)
    private void SetMonsterNum()
    {
        //Set questMonsterNum 
        int monsterNum = Random.Range(0, System.Enum.GetValues(typeof(EMonstertype)).Length);
        //curQuestMon used to check that monster is actually Dying
        curQuestMon = monsterNum;

        while(monsterNum == curQuestMon)
        {
            monsterNum = Random.Range(0, System.Enum.GetValues(typeof(EMonstertype)).Length);
            curQuestMon2 = monsterNum;
        }
    }

    private void SetQuestNum()
    {
        int eQuestLength = System.Enum.GetValues(typeof(EQuest)).Length;

        for (int i = eQuestLength - 1; i > 0; --i)
        {
            if((GameManager.questClear & ((int)Mathf.Pow(2,i))) > 0)
            {
                curQuestNum = (EQuest)Mathf.Pow(2, i);
                break;
            }
        }
    }
    #endregion
    
    //actually Clear Quest Func
    private void QuestClear(EQuest _questNum)
    {
        GameManager.questClear |= (int)_questNum;
        curQuestNum = (EQuest)((int)curQuestNum << 1);
    }

    //Reset your quest Progress
    private void QuestReset()
    {
        GameManager.questClear = 0;
        GameManager.questProgress = 0;
    }

    //Check you clear quest 
    //if you clear, curQuestNum++;
    private bool ClearCheck(EQuest _questNum)
    {
        bool isClear = (GameManager.questClear & (int)_questNum) > 0 ? true : false;
        if(isClear)
        {
            QuestClear(_questNum);
        }
        return isClear;
    }

    private IEnumerator WaitUntilClearCo()
    {
        SetQuestNum();

        int curQuestNumto10 = (int)(Mathf.Log((float)curQuestNum,2f));
        for (int i = curQuestNumto10; i < System.Enum.GetValues(typeof(EQuest)).Length; ++i)
        {
            SetQuestText(curQuestNum);
            yield return new WaitUntil(() => ClearCheck(curQuestNum));
        }
    }

    public void CheckQuestProgress(int _monsterNum)
    {
        if (curQuestMon == _monsterNum || curQuestMon2 == _monsterNum)
        {
            ++GameManager.questProgress;
            if(curQuestSize == GameManager.questProgress)
            {
                GameManager.questProgress = 0;
                SetMonsterNum();
                QuestClear(curQuestNum);
                StartCoroutine(DropCoinCo((int)Mathf.Log((float)curQuestNum, 2f)));
            }
            SetQuestText(curQuestNum);
        }
    }

    public void SpawnGold(int _count)
    {
        for(int i = 0; i < _count * 5; ++i)
        {
            Instantiate(coin, player.transform.position + player.transform.forward, Quaternion.identity);
        }
    }
    private IEnumerator DropCoinCo(int _count)
    {
        _count *= 5;
        float dropTimer = 0.2f * _count + 0.1f;
        float instanceTimer = 0.2f;

        while (dropTimer >= 0f)
        {
            dropTimer -= Time.deltaTime;
            instanceTimer -= Time.deltaTime;
            if (instanceTimer <= 0f)
            {
                Instantiate(coin, player.transform.position + player.transform.forward * 3f + Vector3.up, Quaternion.identity);
                instanceTimer = 0.2f;
            }
            yield return null;
        }
    }

    #region QuestText

    private string GetQuestText(EQuest _questNum)
    {
        //get string at EMonstertype
        string questText = System.Enum.GetNames(typeof(EMonstertype))[curQuestMon];
        string mon2Text = System.Enum.GetNames(typeof(EMonstertype))[curQuestMon2];
        curQuestSize = (int)_questNum;
        questText += "이나 " + mon2Text + "를 " + curQuestSize + "마리 잡으세요\n진행도 (" 
                   + GameManager.questProgress + " / " + curQuestSize +")\n" 
                   + "보상 : " + (Mathf.Log((int)curQuestNum,2) + 1) * 5 +" gold";

        return questText;
    }

    private void SetQuestText(EQuest _questNum)
    {
        questText.text = GetQuestText(_questNum);
    }

    #endregion
}
