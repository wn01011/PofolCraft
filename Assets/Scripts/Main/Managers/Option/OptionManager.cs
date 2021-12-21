using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class OptionManager : MonoBehaviour
{
    #region SerializeField
    [Header("Panels")]
    [SerializeField]
    private GameObject soundPanel = null;
    [SerializeField]
    private GameObject mouseSensePanel = null;
    [SerializeField]
    private GameObject resolutionPanel = null;
    [SerializeField]
    private GameObject savePanel = null;
    [SerializeField]
    private GameObject exitPanel = null;
    [SerializeField]
    private GameObject optionPanel = null;
  
    #endregion

    private AimController aim = null;
    private GameManager gameManager = null;
    private List<GameObject> panels = new List<GameObject>();

    private bool isStop = false;

    private List<Resolution> resolutions = new List<Resolution>();
    private Toggle fullScreenBtn = null;
    private Dropdown resolutionDropdown = null;
    private int resolutionNum = 0;
    private FullScreenMode screenMode = FullScreenMode.ExclusiveFullScreen;

    public void Init()
    {
        aim = FindObjectOfType<AimController>();
        gameManager = FindObjectOfType<GameManager>();

        //set all panel's activate to false
        panels.Add(soundPanel);
        panels.Add(mouseSensePanel);
        panels.Add(resolutionPanel);
        panels.Add(savePanel);
        panels.Add(exitPanel);

        ClosePanels();

        //Set Resolution
        resolutionDropdown = resolutionPanel.GetComponentInChildren<Dropdown>();
        fullScreenBtn = resolutionPanel.GetComponentInChildren<Toggle>();
        InitResolution();

        SetStartOptions();

        optionPanel.SetActive(false);
    }

    private void Update()
    {
        TimeController();
    }

    private void SetStartOptions()
    {
        //Sound
        Slider[] soundSliders = soundPanel.GetComponentsInChildren<Slider>();
        //BGM
        soundSliders[0].value = gameManager.mainData.BGMVolume;
        //SFX
        soundSliders[1].value = gameManager.mainData.SFXVolume;

        //Mouse
        Slider[] mouseSliders = mouseSensePanel.GetComponentsInChildren<Slider>();
        //MouseX
        mouseSliders[0].value = gameManager.mainData.MouseX;
        //MouseY
        mouseSliders[1].value = gameManager.mainData.MouseY;
        //MouseTotal
        mouseSliders[2].value = gameManager.mainData.MouseTotal;

    }
    private void ClosePanels()
    {
        bool isAllclosed = true;
        for (int i = 0; i < panels.Count; ++i)
        {
            if (panels[i].activeSelf == true)
                isAllclosed = false;
            panels[i].SetActive(false);
        }

        if (isAllclosed)
        {
            isStop = !isStop;
            Time.timeScale = 1f;
            CursorLock();
            optionPanel.SetActive(false);
        }
    }


    #region OptionBtns SetActive
    public void SoundBtn()
    {
        soundPanel.SetActive(!soundPanel.activeSelf);
    }
    public void MouseSenseBtn()
    {
        mouseSensePanel.SetActive(!mouseSensePanel.activeSelf);
    }
    public void ResolutionBtn()
    {
        resolutionPanel.SetActive(!resolutionPanel.activeSelf);
    }
    public void SaveBtn()
    {
        savePanel.SetActive(!savePanel.activeSelf);
        SaveTextUpdate();
    }
    public void ExitBtn()
    {
        exitPanel.SetActive(!exitPanel.activeSelf);
    }
    public void CloseBtn()
    {
        ClosePanels();
    }
    #endregion

    #region Volume Change
    public void BGMChange(float _value)
    {
        SoundManager.Instance.BGMVolume = _value;
    }
    public void SFXChange(float _value)
    {
        SoundManager.Instance.SFXVolume = _value;
    }
    #endregion

    #region Mouse Change
    public void MouseXChange(float _value)
    {
        aim.xSensitivity = _value;
    }
    public void MouseYChange(float _value)
    {
        aim.ySensitivity = _value;
    }
    public void MouseTotalChange(float _value)
    {
        aim.totalSensitivity = _value;
    }

    #endregion

    #region TimeControl
    private void TimeController()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            isStop = !isStop;

            if(!optionPanel.activeSelf && !isStop)
            {
                isStop = true;
            }
            if (isStop)
            {
                optionPanel.SetActive(true);
                Time.timeScale = 0f;
                CursorFree();
            }
            else
            {
                optionPanel.SetActive(false);
                ClosePanels();
                Time.timeScale = 1f;
                CursorLock();
            }
        }
    }
    private void CursorLock()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    private void CursorFree()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    #endregion

    #region Resolution
    private void InitResolution()
    {
        resolutionDropdown.ClearOptions();
        for (int i = 0; i < Screen.resolutions.Length; ++i)
        {
            //if(Screen.resolutions[i].refreshRate == 60)
            {
                resolutions.Add(Screen.resolutions[i]);
            }
        }

        int optionNum = 0;
        for(int j = 0; j < resolutions.Count; ++j)
        {
            Dropdown.OptionData option = new Dropdown.OptionData();
            option.text = resolutions[j].width + "x" + resolutions[j].height + " " + resolutions[j].refreshRate + "hz";
            
            resolutionDropdown.options.Add(option);

            if(resolutions[j].width == Screen.width && resolutions[j].height == Screen.height)
            {
                resolutionDropdown.value = optionNum;
                resolutionNum = optionNum;
            }
            ++optionNum;
        }
        resolutionDropdown.RefreshShownValue();

        fullScreenBtn.isOn = Screen.fullScreenMode.Equals(FullScreenMode.FullScreenWindow) ? true : false;
    }
    public void DropboxOptionChage(int x)
    {
        resolutionNum = x;
    }
    public void OptionAcceptBtn()
    {
        Screen.SetResolution(resolutions[resolutionNum].width, resolutions[resolutionNum].height, screenMode);
        aim.SetScreenRatio();
    }
    public void FullScreenBtn(bool isFull)
    {
        screenMode = isFull ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
    }
    #endregion

    #region Save
    private void SaveTextUpdate()
    {
        Text saveText = savePanel.GetComponentInChildren<Text>();
        saveText.text = "ID : " + GameManager.id 
                      + "\n\nGold : "+ GameManager.gold 
                      + "\nLevel : " + GameManager.level
                      + "\nExp : "   + GameManager.exp
                      + "\nTime : "  + (int)GameManager.time;
    }
    public void Save()
    {
        gameManager.Save();
    }
    #endregion

    #region ExitGame
    public void Exit()
    {
        Save();
        SceneManager.LoadScene(0);
    }
    #endregion
}
