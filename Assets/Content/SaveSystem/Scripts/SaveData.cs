using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SaveManager;

[System.Serializable]
public class SaveData
{
    public Dictionary<string, object> DictVariables = new Dictionary<string, object>(); // GUID - List variables
    public SaveInfos SaveInfos;
    public SaveData(Dictionary<string, object> data, SaveInfos saveInfos) { this.DictVariables = data; this.SaveInfos = saveInfos; } 
}

[System.Serializable]
public class SaveInfos
{
    public string GUID = "";
    public string Date = "";
    public string Time = "";

    public SaveInfos(string guid, string date, string time) { this.GUID = guid; this.Date = date; this.Time = time; }
}



