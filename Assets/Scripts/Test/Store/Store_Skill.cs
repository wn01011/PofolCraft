using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Store_Skill : MonoBehaviour
{
    public delegate void CallStoreMg(int a);
    public delegate int CallStoreMg_Gold();
    private CallStoreMg GoldCount = null;
    private CallStoreMg_Gold GetGold = null;
    public void AddFuncToStore_Skill(CallStoreMg _goldcount, CallStoreMg_Gold _getgold)
    {
        GoldCount = _goldcount;
        GetGold = _getgold;
    }

    [SerializeField] private Image Skill_icon = null;
    [SerializeField] private Text Skill_Name = null;
    [SerializeField] private Button Btn_L = null;
    [SerializeField] private Button Btn_R = null;
    [SerializeField] private Text Lv = null;


    private void Start()
    {
        Skill_Icon_Init();
    }

    private void Skill_Icon_Init()
    {
        Btn_L.onClick.AddListener(()=>SetSkillLv(0));
        Btn_R.onClick.AddListener(()=>SetSkillLv(1));
    }

    public Text GetSkillText() => Lv;
    public Image GetSkillImage() => Skill_icon;
    public Text GetSKillName() => Skill_Name;
    private void SetSkillLv(int _type)
    {
        int skillLv = 0;
        if(_type ==0)
        {
            skillLv = int.Parse(Lv.text);
            skillLv--;
            Lv.text = skillLv.ToString();
            
            if (skillLv <0)
            {
                skillLv = 0;
                Lv.text = skillLv.ToString();
            }
            else
            {
                GoldCount(_type);
            }
         
        }


        if(_type ==1)
        {
            if(GetGold() > 0)
            {
                skillLv = int.Parse(Lv.text);
                skillLv++;
                Lv.text = skillLv.ToString();
                GoldCount(_type);
            }
        }
    }

    


}
