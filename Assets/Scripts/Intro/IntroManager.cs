using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class IntroManager : MonoBehaviour
{
    private UIManager uiMg = null;
    private AccountDataManager acDataMg = null;

    private void Awake()
    {
        Time.timeScale = 1;
        InitMg();
        SetCallBack();
        DeleteTempScreenShots();
    }


    private void InitMg()
    {
        acDataMg = new AccountDataManager();
        uiMg = GetComponentInChildren<UIManager>();
    }

    private void SetCallBack()
    {
        uiMg.AddFuncUIManager(JoinIdDupliCheck,JoinConfirm, Login,
                              DeleteCheck, DeleteCheckConfirm);
    }


    private void JoinIdDupliCheck(string _id, string _pw)
    {
        Debug.Log(DataManager.Single().c++);
        acDataMg.Initbools();
        Debug.Log(_id.ToString() +"/"+ _pw.ToString());
        acDataMg.SetInputAccount(_id, _pw);
        acDataMg.LoadAccountList_Join();
        ChangeColor();


    }
    private void JoinConfirm()
    {
        Debug.Log(DataManager.Single().c++);
        acDataMg.AddInputAccountInAccountList();
        acDataMg.SaveAccountList(0);
    }

    private void Login(string _id, string _pw)
    {
        acDataMg.Initbools();
        acDataMg.SetInputAccount(_id, _pw);
        acDataMg.LoadAccountList_Login();
        if(acDataMg.GetPassLogin())
        {
            uiMg.ActiveLoginSeal();
            SceneController.Instance().StartLoadScene(1);
        }
        else
        {
            Debug.Log("Can't Login, ID or PW is Wrong");
        }
    }

    private void DeleteCheck(string _id, string _pw)
    {
        acDataMg.Initbools();
        acDataMg.SetInputAccount(_id, _pw);
        acDataMg.LoadAccountList_Delete();
        acDataMg.StartDeletedAccountCheck();
    }
    private void DeleteCheckConfirm()
    {
        if(uiMg.GetDeleteConfirm())
        {
            acDataMg.DeleteAccountInfo();
        }
        else
        {
            Debug.Log("체크를 확인하세요");
        }
    }

    private void ChangeColor()
    {
        Debug.Log(DataManager.Single().c++);
        uiMg.Getidchktext().gameObject.SetActive(true);
        if (!acDataMg.GetDuplichkId())
        {
            Debug.Log(DataManager.Single().c++);
            uiMg.Getidchktext().text = "<color=red>중복되는 아이디가 있습니다.</color>";
        }
        else
        {
            Debug.Log(DataManager.Single().c++);
            uiMg.Getidchktext().text = "<color=blue>가입가능한 아이디입니다.</color>";
        }
    }

    private void DeleteTempScreenShots()
    {
        List<System.IO.FileInfo> infoDelete = new List<System.IO.FileInfo>();
        string filePath = null;

        if (Application.isEditor)
            filePath = Application.dataPath + "/Resources/screenshots";
        else
            filePath = Application.persistentDataPath + "/screenshots";

        if(!System.IO.File.Exists(filePath))
        {
            System.IO.Directory.CreateDirectory(filePath);
        }

        System.IO.DirectoryInfo directoryInfo = new System.IO.DirectoryInfo(filePath);
        directoryInfo.Refresh();

        int count = directoryInfo.GetFiles().Length;
        
        int addNum = 0;

        if (Application.isEditor) addNum = 2;
        else                      addNum = 1;

        for(int i = 0; i < count; i += addNum)
        {
            string curPath = directoryInfo.GetFiles()[i].Name;
            curPath = curPath.Split('.')[0];
            infoDelete.Add(directoryInfo.GetFiles()[i]);
        }

        for(int i =0; i< infoDelete.Count; ++i)
        {
            infoDelete[i].Delete();
        }
    }
}
