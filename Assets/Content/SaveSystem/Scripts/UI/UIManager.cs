using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    // ----- FIELDS ----- //
    [SerializeField] private GameObject _profilesPanel;
    [SerializeField] private GameObject _saveAndLoadPanel;

    [SerializeField] TMP_Text _inputProfileName;
    [SerializeField] GameObject _contentProfiles;
    [SerializeField] GameObject _prefabProfile;

    [SerializeField] private SaveManager _saveManager;

    public static UIManager Instance;

    private void Awake()
    {
        if (Instance != null) { Destroy(this); }
        Instance = this;
    }
    // ----- FIELDS ----- //

    private void Start()
    {
        _saveManager.OnAddProfile += ShowProfiles;
        ShowProfiles();
        OpenProfilesPanel();
    }

    public void OpenProfilesPanel()
    {
        _profilesPanel.SetActive(true);
        _saveAndLoadPanel.SetActive(false);
    }

    public void OpenSaveAndLoadPanel()
    {
        _profilesPanel.SetActive(false);
        _saveAndLoadPanel.SetActive(true);
    }

    public void CreateProfile()
    {
        if (!string.IsNullOrEmpty(_inputProfileName.text.Replace(" ", "")) && _inputProfileName.text.Replace(" ", "").Length > 1)
        {
            _saveManager.CreateProfile(_inputProfileName.text.Replace(" ", ""));
        }
    }

    public void ShowProfiles()
    {
        GameObject[] allChildren = new GameObject[_contentProfiles.transform.childCount];
        int tempIndex = 0;

        foreach (Transform child in _contentProfiles.transform)
        {
            allChildren[tempIndex] = child.gameObject;
            ++tempIndex;
        }
        foreach (GameObject child in allChildren)
        {
            Destroy(child.gameObject);
        }

        foreach (string profileName in _saveManager.ProfilesNames)
        {
            GameObject profile = Instantiate(_prefabProfile);
            profile.transform.SetParent(_contentProfiles.transform);
            profile.GetComponent<Profile>().SetProfileName(profileName.ToUpper());
        }
    }
}
