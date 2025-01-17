using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Profile : MonoBehaviour 
{
    // ----- FIELDS ----- //
    public string ProfileName = "";

    [SerializeField] private TMP_Text _profileNameTxt;
    [SerializeField] private TMP_Text _profileInfosTxt;

    private UIManager _uiManager;
    private SaveManager _saveManager;
    public SaveData SaveData;
    // ----- FIELDS ----- //

    private void Start()
    {
        _uiManager = UIManager.Instance;
        _saveManager = SaveManager.Instance;
    }

    public void SetProfileName(string profileName)
    {
        ProfileName = profileName;
        _profileNameTxt.text = profileName;
    }

    public void SelectProfile()
    {
        if (_uiManager != null)
        {
            _uiManager.OpenSaveAndLoadPanel(ProfileName);
        }
    }

    public void DeleteProfile()
    {
        _saveManager.DeleteProfile(ProfileName);
    }

    public void SetData(SaveData saveData)
    {
        SaveData = saveData;
        _profileInfosTxt.text = $"Date : {SaveData.SaveInfos.Date} \n\n Time : {SaveData.SaveInfos.Time}";
    }
}
