using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SaveSlot : MonoBehaviour
{
    public SaveData SaveData;
    [SerializeField] private TMP_Text _saveNameTxt;
    [SerializeField] private TMP_Text _saveInfosTxt;
    private UIManager _uiManager;
    private SaveManager _saveManager;

    private void Start()
    {
        _uiManager = UIManager.Instance;
        _saveManager = SaveManager.Instance;
    }

    public void InitSaveSlot(SaveData saveData)
    {
        SaveData = saveData;
        Debug.Log($"init save {SaveData.SaveInfos.Time}");

        _saveNameTxt.text = SaveData.SaveInfos.GUID;
        _saveInfosTxt.text = $"Date : {saveData.SaveInfos.Date} \n\n Time : {saveData.SaveInfos.Time}";
    }

    public void LoadSave()
    {
        _saveManager.LoadSave(SaveData.SaveInfos);
    }
}
