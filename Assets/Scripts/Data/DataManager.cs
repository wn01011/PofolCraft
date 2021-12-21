using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;




public class DataManager
{
    private static DataManager DataMg = null;

    private DataManager() { }

    public static DataManager Single()
    {
        if(DataMg ==null)
        {
            DataMg = new DataManager();
            Debug.Log("NEW DATAMANAGER");
        }
        return DataMg;
    }



    private string toJsonData = "";
    private string fromJsonData = "";
    public int c = 0; 
    
    public void SaveFile<T>(string _path, T _data)
    {
        toJsonData = JsonUtility.ToJson(_data);
        File.WriteAllText(_path, toJsonData);
    }

    public void LoadFile<T>(string _path, ref T _data)
    {
        if(File.Exists(_path))
        {
            fromJsonData = File.ReadAllText(_path);
            _data = JsonUtility.FromJson<T>(fromJsonData);
            Debug.Log("File Load Sucess" + _data);
        }
        else
        {
            Debug.Log("File is Empty : " + _path);
        }
    }

}
