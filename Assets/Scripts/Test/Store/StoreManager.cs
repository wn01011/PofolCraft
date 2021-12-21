using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreManager : MonoBehaviour
{

    enum Skillname { AtkUp, HpUp, AtkSpeedUp, SpeedUp, JumpUp, SplashDmgUp }


    private Sprite[] skill_IconList = null;  
    [SerializeField] private Image store_BgImg = null;
    [SerializeField] private GameObject skillBtnGo = null;
    [SerializeField] private GameObject playerProfilePanel = null;
    [SerializeField] private List<Store_Skill> skillList = new List<Store_Skill>();
    private GameObject skillGoPrefab = null;
    private List<Vector2> skillPosList = new List<Vector2>();
    private List<Text> skillTextList = new List<Text>();



    public List<Text> GetSkillTexts() => skillTextList;
    public List<Store_Skill> GetSkillList() => skillList;
    public GameObject GetPlayerProFilePanel() => playerProfilePanel;

    private int GetGold()=> int.Parse(playerProfilePanel.GetComponentInChildren<Text>().text);

    private void Start()
    {
        Store_Init();
        SetPosSkillList();
    }
    
    private void Store_Init()
    {
        store_BgImg.sprite = Resources.Load<Sprite>("Prefabs/UI/bg");
        skillGoPrefab = Resources.Load<GameObject>("Prefabs/UI/Skill");
        skill_IconList = Resources.LoadAll<Sprite>("Prefabs/UI/Icon_Skill");
    }

    //스킬 창 배치
    private void SetPosSkillList() 
    {
        float sizeX = store_BgImg.rectTransform.sizeDelta.x;
        float sizeY = store_BgImg.rectTransform.sizeDelta.y;

        float skill_Icon_SizeX = skillGoPrefab.GetComponent<RectTransform>().sizeDelta.x;
        float skill_Icon_SizeY = skillGoPrefab.GetComponent<RectTransform>().sizeDelta.y;

        for (int posY = -(int)((skill_Icon_SizeY * 1.5f) * 0.5f); posY <= sizeY; posY -= (int)(skill_Icon_SizeY * 1.25f))
        {
            for (float posX = (((skill_Icon_SizeX * 1.5f) * 0.5f)); posX < sizeX; posX += (skill_Icon_SizeX * 1.25f))
            {
                skillPosList.Add(new Vector2(posX, posY));
            }
        }

        for (int i = 0; i < 6; i++)
        {
            GameObject Go = Instantiate(skillGoPrefab, skillBtnGo.transform).gameObject;
            skillList.Add(Go.GetComponent<Store_Skill>());
            skillList[i].GetComponent<RectTransform>().anchoredPosition = skillPosList[i];
            skillList[i].AddFuncToStore_Skill(CountGold, GetGold);


            skillBtnGo.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 0f);
            skillTextList.Add(skillList[i].GetSkillText());

            SetSkillIcon(i);
            SetSkillName(i);
        }
    }

    private void SetSkillIcon(int _idx)
    {
        skillList[_idx].GetSkillImage().sprite = skill_IconList[_idx];
    }
    private void SetSkillName(int _idx)
    {
        skillList[_idx].GetSKillName().text = System.Enum.GetName(typeof(Skillname),_idx);
    }
    
  
    private void CountGold(int _type)
    {
        int gold = int.Parse(playerProfilePanel.GetComponentInChildren<Text>().text);
        if (_type == 0)
        {
            gold++;
        }
        if (_type == 1)
        {
            gold--;
            if(gold < 0)
            {
                gold = 0;
            }
        }
        playerProfilePanel.GetComponentInChildren<Text>().text = gold.ToString();
    }


}
