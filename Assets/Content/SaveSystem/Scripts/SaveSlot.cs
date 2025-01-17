using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SaveSlot : MonoBehaviour
{
    // ----- FIELDS ----- //
    public SaveData SaveData;
    [SerializeField] private TMP_Text _saveNameTxt;
    [SerializeField] private TMP_Text _saveInfosTxt;

    [SerializeField] private GameObject _isLoadedContent;
    [SerializeField] private GameObject _isNotLoadedContent;

    private UIManager _uiManager;
    private SaveManager _saveManager;

    private bool _isLoaded = false;
    // ----- FIELDS ----- //

    private void Start()
    {
        _uiManager = UIManager.Instance;
        _saveManager = SaveManager.Instance;
    }

    public void InitSaveSlot(SaveData saveData, bool autoLoad = false)
    {
        Debug.Log($"init save {SaveData.SaveInfos.Time}");
        SaveData = saveData;

        _saveNameTxt.text = SaveData.SaveInfos.GUID;
        _saveInfosTxt.text = $"Date : {saveData.SaveInfos.Date} \n\n Time : {saveData.SaveInfos.Time}";
        Debug.Log($"Date : {saveData.SaveInfos.Date} \n\n Time : {saveData.SaveInfos.Time}");

        if (autoLoad) LoadSave();
    }

    public void LoadSave()
    {
        if (_saveManager == null) _saveManager = SaveManager.Instance;
        _saveManager.LoadSave(SaveData.SaveInfos);

        SetIsLoaded(true);
    }

    public void DeleteSave()
    {
        _saveManager.DeleteSave(SaveData.SaveInfos);
    }

    public void SetIsLoaded(bool isLoaded)
    {
        _isLoaded = isLoaded;

        if (isLoaded)
        {
            _isLoadedContent.SetActive(true);
            _isNotLoadedContent.SetActive(false);
        }
        else
        {
            _isLoadedContent.SetActive(false);
            _isNotLoadedContent.SetActive(true);
        }
    }
}
