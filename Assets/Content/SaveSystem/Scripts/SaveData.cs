using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SaveManager;

[System.Serializable]
public class SaveData
{
    //public List<ScriptSelection> scriptSelections = new List<ScriptSelection>();
    //public SaveData (List<ScriptSelection> scriptSelections) {  this.scriptSelections = scriptSelections; }

    public Dictionary<string, object> data;
    public SaveData(Dictionary<string, object> data) { this.data = data; }
}



