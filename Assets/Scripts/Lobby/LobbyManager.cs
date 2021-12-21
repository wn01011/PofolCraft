using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    private UIManager_Lobby uiMg = null;
    private GameManager gameMg = null;
    private StoreManager storeMg = null;

    private void Init_Btn()
    {
        uiMg.AddFuncUIMg(SavePlayerData, LoadPlayerDataText);
    }



    private void Start()
    {
        Init_Mg();
        Init_Btn();
        gameMg.Load();
    }

    private void Init_Mg()
    {
        gameMg = FindObjectOfType<GameManager>();
        uiMg = GetComponentInChildren<UIManager_Lobby>();
        storeMg = GetComponentInChildren<StoreManager>();
    }
    
    public void SavePlayerData()
    {
        GameManager.AtkUp = int.Parse(storeMg.GetSkillTexts()[0].text);
        GameManager.HpUp = int.Parse(storeMg.GetSkillTexts()[1].text);
        GameManager.AtkSpeedUp = int.Parse(storeMg.GetSkillTexts()[2].text);
        GameManager.SpeedUp = int.Parse(storeMg.GetSkillTexts()[3].text);
        GameManager.JumpUp = int.Parse(storeMg.GetSkillTexts()[4].text);
        GameManager.SplashDmgUp = int.Parse(storeMg.GetSkillTexts()[5].text);
        GameManager.gold = int.Parse(storeMg.GetPlayerProFilePanel().gameObject.GetComponentInChildren<UnityEngine.UI.Text>().text);

        SetStoreText();
    }

    public void LoadPlayerDataText()
    {
        SetStoreText();
    }
    private void SetStoreText()
    {
        storeMg.GetSkillTexts()[0].text = GameManager.AtkUp.ToString();
        storeMg.GetSkillTexts()[1].text = GameManager.HpUp.ToString();
        storeMg.GetSkillTexts()[2].text = GameManager.AtkSpeedUp.ToString();
        storeMg.GetSkillTexts()[3].text = GameManager.SpeedUp.ToString();
        storeMg.GetSkillTexts()[4].text = GameManager.JumpUp.ToString();
        storeMg.GetSkillTexts()[5].text = GameManager.SplashDmgUp.ToString();
        SetPlayerProfile();
    }


    public void SetPlayerProfile()
    {
        storeMg.GetPlayerProFilePanel().gameObject.GetComponentInChildren<UnityEngine.UI.Text>().text = GameManager.gold.ToString();
    }


    










}
