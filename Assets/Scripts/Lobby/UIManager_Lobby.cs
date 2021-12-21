using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager_Lobby : MonoBehaviour
{
    public delegate void CallLobbyManager();
    private CallLobbyManager SetPlayerData = null;
    private CallLobbyManager SetSkillData = null;
    private Player_Lobby player = null;
    private Aim_Lobby aim = null;

    private Ray ray = new Ray();
    private RaycastHit raycastHit = new RaycastHit();
    public void AddFuncUIMg(CallLobbyManager _setdata, CallLobbyManager _setskill )
    {
        SetPlayerData += _setdata;
        SetSkillData += _setskill;
    }


    enum Panel { STORE, CONFIRM, CLOSE}
    //상점패널 : 저장,닫기 / 확인 패널 : 저장, 닫기, 
    enum Btn { SAVE, CLOSE, CONFIRM, CANCEL, NOSAVECONFIRM, NOSAVECANCEL}
    enum Off { SINGLE, ALL}


    
    [SerializeField] private List<GameObject> panelList = new List<GameObject>();
    [SerializeField] private List<Button> btnList = new List<Button>();
    [SerializeField] private Stack<GameObject> ActivePanel = new Stack<GameObject>();



    private void Start()
    {
        SetOnClickBtn();
        player = FindObjectOfType<Player_Lobby>();
        aim = FindObjectOfType<Aim_Lobby>();
    }
    private void Update()
    {
        ActiveStore();
    }


    private void SetOnClickBtn()
    {
        btnList[(int)Btn.SAVE].onClick.AddListener(() => OpenPanel((int)Panel.CONFIRM));
        btnList[(int)Btn.CONFIRM].onClick.AddListener(() => SkillSave());
        btnList[(int)Btn.CANCEL].onClick.AddListener(() => PanelOff(Off.SINGLE));

        btnList[(int)Btn.CLOSE].onClick.AddListener(() => OpenPanel((int)Panel.CLOSE));
        btnList[(int)Btn.NOSAVECONFIRM].onClick.AddListener(() => PanelOff(Off.ALL));
        btnList[(int)Btn.NOSAVECANCEL].onClick.AddListener(() => PanelOff(Off.SINGLE));
    }
    private void OpenPanel(int _idx)
    {
        panelList[_idx].SetActive(true);
        if(_idx == 0)
        {
            SetSkillData();
        }
        ActivePanel.Push(panelList[_idx]);
    }

    private void ActiveStore()
    {
        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        
        if (Physics.Raycast(ray, out raycastHit))
        {
            if (Input.GetMouseButtonDown(0) && raycastHit.collider.CompareTag("Building") && ActivePanel.Count==0)
            {
                OpenPanel(0);
                aim.CursorStateChange();
                if (player.GetIsCanMove())
                {
                    player.SetIsMove(false);
                }
            }
        }

    }

    private void SkillSave()
    {
        SetPlayerData();
        PanelOff(Off.ALL);
    }

    
    private void PanelOff(Off _i)
    {
        
        if (ActivePanel.Count != 0)
        {
            switch (_i)
            {
                case Off.SINGLE:
                    ActivePanel.Pop().SetActive(false);
                    break;
                case Off.ALL:
                    for (int i = ActivePanel.Count; i > 0; i--)
                    {
                        ActivePanel.Pop().SetActive(false);
                    }
                    break;
            }
        }
        aim.CursorStateChange();
        player.SetIsMove(true);
    }



}
