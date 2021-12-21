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

    //�÷��̾ �Է��� Account
    private InputAccount inputAccount = new InputAccount();

    //���� ������ �����̸� 
    private readonly string accountDataFileName = "AccountData.json";
    private string filePath = null;

    //Json���� �Ѱ��� Account
    private Account accountList = new Account();

    //Id �� Pw ������ ���� ���� / ��밡�ɿ��� 
    private bool useableId = false;
    private bool useableIdLength = false;
    private bool passableLoginId = false;
    private bool passableLoginPw = false;
    private bool deleteId = false;
    private bool deletePw = false;
    private int accountIdxInAccountList = 0;

    //�����ڷ� Start ����� ��ü
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


    //���� ���
    private void SetfilePath()
    {
        filePath = Application.persistentDataPath;
        filePath += accountDataFileName;
    }


    //�Է� ������ �޴� �Լ� 
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


   
       
    //���� �����ϱ�
    public void AddInputAccountInAccountList()
    {
        //���̵� ���̰� �����ϰ�, ���̵� �ߺ����� ���� ��
        if (useableIdLength == true && useableId == true)
        {
            accountList.id.Add(inputAccount.id);
            accountList.pw.Add(inputAccount.pw);
        }
        else
        {
            //���� ������
            Debug.Log("Fail SaveAccount(idlength/useableid) : " + useableIdLength + " / " + useableId);
        }
    }


    #region IntroScene

    #region Join
    //id ���� ���� : ���� �� �ߺ�����
    public void StartJoinCheck()
    {
        IdLengthCheck();
        IdDupliCheck();
    }
    //ID ���� üũ 
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

    //id �ߺ� ����
    //id ���� ������ true �� �� accountList�� ������ �ε���
    //�ε�� accountList�� ����ִ��� �˻��Ͽ� Id �ߺ� üũ �ǽ� 
    //����ִٸ� �ٷ� true �ݿ�, �ߺ��̶�� false, ���� id�� ������ true
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
    //�α���

    //�ߺ� �˻� �� id ��ȯ
    public bool GetDuplichkId()
    {
        return useableId;
    }
    //���̵� �α��� ���� ���� ��ȯ 
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
            Debug.Log("��������� �����ϴ�.");
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
                    
                    Debug.Log("�α��� ���� " + passableLoginId.ToString() + " " + passableLoginPw.ToString());
                    CreateUID(ref _inputInfo.id);
                    break;
                }
                else
                {
                    passableLoginId = false;
                    passableLoginPw = false;
                    Debug.Log("�α��� ���� " + passableLoginId.ToString() + " " + passableLoginPw.ToString());
                }
            }
        }
        else
        {
            Debug.Log("�α��� ���� �Է� â�� ����ֽ��ϴ�.");
        }
    }
    #endregion


    #region DeleteAccount
    //ȸ��Ż��

    //ȸ��Ż�� ���� ���� ��ȯ 
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
            Debug.Log("���� ����Ʈ�� �ֽ��ϴ�.");
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
                    Debug.Log("������ �����Ͻðڽ��ϱ�? " + deleteId.ToString() + " " + deletePw.ToString());
                    break;
                }
                else
                {
                    deleteId = false;
                    deletePw = false;
                    Debug.Log("��ġ�ϴ� ������ �����ϴ�. " + deleteId.ToString() + " " + deletePw.ToString());
                }
            }
        }
        else
        {
            Debug.Log("���� ���� �Է� â�� ����ֽ��ϴ�.");
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
            Debug.Log("���̵� ��й�ȣ�� ���� �ʽ��ϴ�.");
        }
    }
    #endregion

    #endregion










}
