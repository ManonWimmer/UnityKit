using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SaveManager;

[System.Serializable]
public class SaveData
{
    public List<(string, object)> ListTupleSelectionGUIDListVariables = new List<(string, object)>(); // GUID - List variables
    public Dictionary<string, TransformSelection> DictSelectionGUIDTransformSelection = new Dictionary<string, TransformSelection>(); // GUID - Transform Selection
    public SaveInfos SaveInfos;
    public SaveData(List<(string, object)> data, Dictionary<string, TransformSelection> dictTransforms, SaveInfos saveInfos) { this.ListTupleSelectionGUIDListVariables = data; this.DictSelectionGUIDTransformSelection = dictTransforms; this.SaveInfos = saveInfos; } 
}

[System.Serializable]
public class SaveInfos
{
    public string SaveGUID = "";
    public string Date = "";
    public string Time = "";
    public int SaveNbr = 0;

    public SaveInfos(string guid, string date, string time, int saveNbr) { this.SaveGUID = guid; this.Date = date; this.Time = time; this.SaveNbr = saveNbr; }
}



