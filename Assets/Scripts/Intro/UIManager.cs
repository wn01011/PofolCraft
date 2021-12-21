using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class UIManager : MonoBehaviour
{
    public delegate void CallIntroMg_AcChk(string _id, string _pw);
    public delegate void CallIntroMg_Confirm();
    public delegate void CallIntroMg_Login(string _id, string _pw);
    public delegate void CallIntroMg_DeleteChk(string _id, string _pw);
    public delegate void CallIntroMg_DeleteChkConfirm();
    private CallIntroMg_AcChk JoinIdDupliCheck = null;
    private CallIntroMg_Confirm JoinConfirm = null;
    private CallIntroMg_Login Login = null;
    private CallIntroMg_DeleteChk DeleteChk = null;
    private CallIntroMg_DeleteChkConfirm DeleteChkConfirm = null;
    public void AddFuncUIManager(CallIntroMg_AcChk _joinidduplichk, CallIntroMg_Confirm _joinconfirm, 
                                 CallIntroMg_Login _login, CallIntroMg_DeleteChk _deletechk, CallIntroMg_DeleteChkConfirm _deletechkcon)
    {
        JoinIdDupliCheck += _joinidduplichk;
        JoinConfirm += _joinconfirm;
        Login += _login;
        DeleteChk += _deletechk;
        DeleteChkConfirm += _deletechkcon;
    }


    enum Panel { LOGIN, JOIN, DEL_ACCOUNT, DEL_CONFRIM, SCREEN}
    [SerializeField] private List<GameObject> PanelList = new List<GameObject>();
    enum Btn { LOGIN, JOIN, DUPLICHECK, JOINCONFIRM, DELETE, DELETECONFIRM, DELETERECONFIRM}
    [SerializeField] private List<Button> BtnList = new List<Button>();
    
    
    enum InField {LOGINID, LOGINPW,JOINID,JOINPW,DELETEID,DELETEPW }
    [SerializeField] private List<InputField> inputFieldList = new List<InputField>();
    
    [SerializeField] private Stack<GameObject> ActivePanel = new Stack<GameObject>();

    [SerializeField] private Image loginSeal = null;

    private ID_CheckText idchktxt = null;

    
    private void Start()
    {
        SetInputFields();
        SetOnClick();
        SetIdText();
    }


    private void Update()
    {
        InputCtrl();
    }

    private void SetInputFields()
    {
        for(int i = 0; i< PanelList.Count; i++)
        {
            for(int idx = 0; idx < PanelList[i].GetComponentsInChildren<InputField>().Length; idx++)
            {
                inputFieldList.Add(PanelList[i].GetComponentsInChildren<InputField>()[idx]);
            }
        }
    }


    private void SetOnClick()
    {
        SetOnClick_Join();
        SetOnClick_Login();
        SetOnClick_Delete();

    }
    private void SetOnClick_Join()
    {
        BtnList[(int)Btn.JOIN].onClick.AddListener(delegate { OnJoin(PanelList[(int)Panel.JOIN]); });
        BtnList[(int)Btn.DUPLICHECK].onClick.AddListener(() =>
            JoinIdDupliCheck(inputFieldList[(int)InField.JOINID].text, inputFieldList[(int)InField.JOINPW].text));
        BtnList[(int)Btn.JOINCONFIRM].onClick.AddListener(() => JoinConfirm());
        BtnList[(int)Btn.JOINCONFIRM].onClick.AddListener(() => PanelOff());
    }

    private void SetOnClick_Login()
    {
        BtnList[(int)Btn.LOGIN].onClick.AddListener(() => Login(inputFieldList[(int)InField.LOGINID].text.ToString(), inputFieldList[(int)InField.LOGINPW].text.ToString()));
    }

    private void SetOnClick_Delete()
    {
        BtnList[(int)Btn.DELETE].onClick.AddListener(delegate { OnJoin(PanelList[(int)Panel.DEL_ACCOUNT]); });
        BtnList[(int)Btn.DELETECONFIRM].onClick.AddListener(() => DeleteChk(inputFieldList[(int)InField.DELETEID].text.ToString(), inputFieldList[(int)InField.DELETEPW].text.ToString()));
        BtnList[(int)Btn.DELETECONFIRM].onClick.AddListener(() => DeleteChkConfirmPanel());
        BtnList[(int)Btn.DELETERECONFIRM].onClick.AddListener(() => DeleteChkConfirm());
        BtnList[(int)Btn.DELETERECONFIRM].onClick.AddListener(() => PanelOff());
    }


    //텍스트 세팅
    private void SetIdText()
    {
        idchktxt = PanelList[(int)Panel.JOIN].GetComponentInChildren<ID_CheckText>();
    }

    public Text Getidchktext()
    {
        return idchktxt.GetComponent<Text>();
    }


    //조작 관리
    private void InputCtrl()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            PanelOff();
        }
    }

    private void PanelOff()
    {
        if(ActivePanel.Count != 0)
        {
            for(int i = ActivePanel.Count; i > 0; i--)
            {
                ActivePanel.Pop().SetActive(false);
            }
            PanelList[(int)Panel.SCREEN].SetActive(false);
            idchktxt.gameObject.SetActive(false);
            foreach(InputField field in inputFieldList)
            {
                ResetText(field);
            }
        }
    }

    public void PanelOffAfterConfirm()
    {
        PanelOff();
    }

    //입력된 텍스트 공백화
    private void ResetText(InputField _inputFiled)
    {
        _inputFiled.text = "";
    }


    private void OnJoin(GameObject _obj)
    {
        if (!_obj.activeSelf)
        {
            _obj.SetActive(true);
            ActivePanel.Push(_obj);
            if(!PanelList[(int)Panel.SCREEN].activeSelf)
                PanelList[(int)Panel.SCREEN].SetActive(true);
        }
        else
        {
            _obj.SetActive(false);
            ActivePanel.Pop();
        }
    }

    public void DeleteChkConfirmPanel()
    {
        OnJoin(PanelList[(int)Panel.DEL_CONFRIM].gameObject);
    }

    public bool GetDeleteConfirm()
    {
        return PanelList[(int)Panel.DEL_CONFRIM].GetComponentInChildren<Toggle>().isOn;
    }

    public void ActiveLoginSeal()
    {
        loginSeal.gameObject.SetActive(true);
        loginSeal.gameObject.GetComponent<Animator>().SetTrigger("IsDone");
    }






}
