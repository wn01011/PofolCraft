using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Account
{
    public List<string> id = new List<string>();
    public List<string> pw = new List<string>();
}

class InputAccount
{
    public string id = null;
    public string pw = null;
}


public class AccountDataManager
{
    //user identifier
    public static string UID = null;

    //플레이어가 입력한 Account
    private InputAccount inputAccount = new InputAccount();

    //게임 데이터 파일이름 
    private readonly string accountDataFileName = "AccountData.json";
    private string filePath = null;

    //Json으로 넘겨줄 Account
    private Account accountList = new Account();

    //Id 및 Pw 판정을 위한 변수 / 사용가능여부 
    private bool useableId = false;
    private bool useableIdLength = false;
    private bool passableLoginId = false;
    private bool passableLoginPw = false;
    private bool deleteId = false;
    private bool deletePw = false;
    private int accountIdxInAccountList = 0;

    //생성자로 Start 기능을 대체
    public AccountDataManager() { SetfilePath(); }

    private void CreateUID(ref string _id)
    {
        UID = _id;
    }

    public void Initbools()
    {
        useableId = false;
        useableIdLength = false;
        passableLoginId = false;
        passableLoginPw = false;
        deleteId = false;
        deletePw = false;
        accountIdxInAccountList = 0;
    }


    //파일 경로
    private void SetfilePath()
    {
        filePath = Application.persistentDataPath;
        filePath += accountDataFileName;
    }


    //입력 데이터 받는 함수 
    public void SetInputAccount(string _id, string _pw)
    {
        inputAccount.id = _id;
        inputAccount.pw = _pw;
    }

    public void LoadAccountList_Join()
    {
        if (File.Exists(filePath))
        {
            DataManager.Single().LoadFile(filePath, ref accountList);
            Debug.Log("File Load is Sucess");   
            StartJoinCheck();
        }
        else
        {
            StartJoinCheck();
            Debug.Log("File is Empty => New File Create");
        }
    }
    public void LoadAccountList_Login()
    {
        if (File.Exists(filePath))
        {
            DataManager.Single().LoadFile(filePath, ref accountList);
            StartLoginCheck();
        }
        else
        {
            StartLoginCheck();
            Debug.Log("AccountList is Empty");
        }
    }

    public void LoadAccountList_Delete()
    {
        if(File.Exists(filePath))
        {
            DataManager.Single().LoadFile(filePath, ref accountList);
        }
        else
        {
            Debug.Log("AccountList is Empty");
        }
    }

    public void SaveAccountList(int _type)
    {
        Debug.Log(DataManager.Single().c++);
        if (_type == 0) //Login or Join
        {
            if (useableIdLength == true && useableId == true)
            {
                DataManager.Single().SaveFile(filePath, accountList);
            }
            else
            {
                Debug.Log("Failed Save");
            }
        }
        else if(_type ==1)
        {
            if (deleteId == true && deletePw == true)
            {
                DataManager.Single().SaveFile(filePath, accountList);
            }
            else
            {
                Debug.Log("Failed Save");
            }
        }

        
        
    }


   
       
    //파일 저장하기
    public void AddInputAccountInAccountList()
    {
        //아이디 길이가 적합하고, 아이디가 중복되지 않을 때
        if (useableIdLength == true && useableId == true)
        {
            accountList.id.Add(inputAccount.id);
            accountList.pw.Add(inputAccount.pw);
        }
        else
        {
            //조건 부적합
            Debug.Log("Fail SaveAccount(idlength/useableid) : " + useableIdLength + " / " + useableId);
        }
    }


    #region IntroScene

    #region Join
    //id 적합 판정 : 길이 및 중복여부
    public void StartJoinCheck()
    {
        IdLengthCheck();
        IdDupliCheck();
    }
    //ID 길이 체크 
    private void IdLengthCheck()
    {
        if (inputAccount.id.Length > 3)
        {
            useableIdLength = true;
        }
        else
        {
            useableIdLength = false;
        }
    }

    //id 중복 판정
    //id 길이 판정이 true 일 떄 accountList에 파일을 로드함
    //로드된 accountList가 비어있는지 검사하여 Id 중복 체크 실시 
    //비어있다면 바로 true 반영, 중복이라면 false, 같은 id가 없으면 true
    private void IdDupliCheck()
    {
        if(useableIdLength) 
        {
            //LoadAccountList_Join();
            if (accountList.id.Count > 0) 
            {
                for (int i = 0; i < accountList.id.Count; i++)
                {
                    if (accountList.id[i].CompareTo(inputAccount.id) != 0)
                    {
                        useableId = true;
                        Debug.Log(inputAccount.id);
                    }
                    else
                    {
                        useableId = false;
                        Debug.Log(inputAccount.id);
                        break;
                    }
                }
            }
            else
            {
                Debug.Log("accountList is Empty => useableId : "+ useableId.ToString()+ " For Save New accountList");
                useableId = true;
            }
        }
        else
        {
            useableId = false;
            Debug.Log("Fail IdLength(Before DupliCheck) : " + useableIdLength.ToString());
        }
    }

    #endregion

    #region Login
    //로그인

    //중복 검사 할 id 반환
    public bool GetDuplichkId()
    {
        return useableId;
    }
    //아이디 로그인 가능 여부 반환 
    public bool GetPassLogin()
    {
        bool passLogin = false;
        if (passableLoginId && passableLoginPw)
        {
            passLogin = true;
        }
        else
        {
            passLogin = false;
        }
        return passLogin;
    }

    public void StartLoginCheck()
    {
        if(File.Exists(filePath))
        {
            CheckLoginInfo(inputAccount);
        }
        else
        {
            Debug.Log("계정기록이 없습니다.");
        }
    }

    private void CheckLoginInfo(InputAccount _inputInfo)
    {
        if (_inputInfo.id != null && _inputInfo.pw != null)
        {
            for (int i = 0; i < accountList.id.Count; i++)
            {
                if (accountList.id[i].CompareTo(_inputInfo.id) == 0 &&
                   accountList.pw[i].CompareTo(_inputInfo.pw) == 0)
                {
                    passableLoginId = true;
                    passableLoginPw = true;
                    
                    Debug.Log("로그인 성공 " + passableLoginId.ToString() + " " + passableLoginPw.ToString());
                    CreateUID(ref _inputInfo.id);
                    break;
                }
                else
                {
                    passableLoginId = false;
                    passableLoginPw = false;
                    Debug.Log("로그인 실패 " + passableLoginId.ToString() + " " + passableLoginPw.ToString());
                }
            }
        }
        else
        {
            Debug.Log("로그인 정보 입력 창이 비어있습니다.");
        }
    }
    #endregion


    #region DeleteAccount
    //회원탈퇴

    //회원탈퇴 가능 여부 반환 
    public bool GetDeleteAccount()
    {
        bool deleteAccount = false;
        if(deleteId && deletePw)
        {
            deleteAccount = true;
        }
        else
        {
            deleteAccount = false;
        }
        return deleteAccount;
    }

    public void StartDeletedAccountCheck()
    {
        if(File.Exists(filePath))
        {
            CheckDeleteAccountInfo(inputAccount);
            Debug.Log("계정 리스트가 있습니다.");
        }
        else
        {
            Debug.Log("Can't Account Check, AccountList is Empty");
        }
    }

    private void CheckDeleteAccountInfo(InputAccount _inputInfo)
    {
        if (_inputInfo.id != null && _inputInfo.pw != null)
        {
            for (int i = 0; i < accountList.id.Count; i++)
            {
                if (accountList.id[i].CompareTo(_inputInfo.id) == 0 &&
                   accountList.pw[i].CompareTo(_inputInfo.pw) == 0)
                {
                    deleteId = true;
                    deletePw = true;
                    accountIdxInAccountList = i;
                    Debug.Log("계정을 삭제하시겠습니까? " + deleteId.ToString() + " " + deletePw.ToString());
                    break;
                }
                else
                {
                    deleteId = false;
                    deletePw = false;
                    Debug.Log("일치하는 계정이 없습니다. " + deleteId.ToString() + " " + deletePw.ToString());
                }
            }
        }
        else
        {
            Debug.Log("계정 정보 입력 창이 비어있습니다.");
        }
    }

    public void DeleteAccountInfo()
    {
        if(deleteId && deletePw)
        {
            Debug.Log("Delete Success Account : id / " + inputAccount.id);
            accountList.id.RemoveAt(accountIdxInAccountList);
            accountList.pw.RemoveAt(accountIdxInAccountList);
            SaveAccountList(1);
        }
        else
        {
            Debug.Log("아이디나 비밀번호가 맞지 않습니다.");
        }
    }
    #endregion

    #endregion










}
