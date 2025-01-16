using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Profile : MonoBehaviour 
{
    public string ProfileName = "";
    [SerializeField] private TMP_Text _profileNameTxt;
    private UIManager _uiManager;

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
            _uiManager.OpenSaveAndLoadPanel();
        }
    }
}
