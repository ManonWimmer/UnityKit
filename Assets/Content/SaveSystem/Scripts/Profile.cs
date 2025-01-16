using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Profile : MonoBehaviour 
{
    public string ProfileName = "";
    [SerializeField] private TMP_Text _profileNameTxt;
    [SerializeField] private TMP_Text _profileInfosTxt;
    private UIManager _uiManager;
    public SaveData SaveData;

    private void Start()
    {
        _uiManager = UIManager.Instance;
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

    public void SetData(SaveData saveData)
    {
        SaveData = saveData;
        _profileInfosTxt.text = $"Date : {SaveData.SaveInfos.Date} \n\n Time : {SaveData.SaveInfos.Time}";
    }
}
