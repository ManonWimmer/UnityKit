using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class UIManager : MonoBehaviour
{
    // ----- FIELDS ----- //
    [SerializeField] private GameObject _profilesPanel;
    [SerializeField] private GameObject _saveAndLoadPanel;

    [SerializeField] TMP_Text _inputProfileName;
    [SerializeField] GameObject _contentProfiles;
    [SerializeField] GameObject _prefabProfile;

    [SerializeField] TMP_Text _profileNameInSavesTxt;

    [SerializeField] GameObject _contentSaves;
    [SerializeField] GameObject _prefabSave;

    [SerializeField] private SaveManager _saveManager;

    public static UIManager Instance;

    private List<SaveSlot> _saveSlots = new List<SaveSlot>();

    private void Awake()
    {
        if (Instance != null) { Destroy(this); }
        Instance = this;
    }
    // ----- FIELDS ----- //

    private void Start()
    {
        _saveManager.OnUpdateSave += ShowProfiles;
        _saveManager.OnUpdateSave += ShowSaves;
        _saveManager.OnAllProfilesData += SetProfiles;
        _saveManager.OnRemoveProfile += ShowProfiles;
        ShowProfiles();
        OpenProfilesPanel();
    }

    public void OpenProfilesPanel()
    {
        _profilesPanel.SetActive(true);
        _saveAndLoadPanel.SetActive(false);

        _saveManager.CurrentProfile = "";
    }

    public void OpenSaveAndLoadPanel(string profileName)
    {
        _profilesPanel.SetActive(false);
        _saveAndLoadPanel.SetActive(true);
        _saveManager.CurrentProfile = profileName;

        _profileNameInSavesTxt.text = profileName;

        ShowSaves();
    }

    public void CreateProfile()
    {
        if (!string.IsNullOrEmpty(_inputProfileName.text.Replace(" ", "")) && _inputProfileName.text.Replace(" ", "").Length > 1)
        {
            _saveManager.CreateProfile(_inputProfileName.text.Replace(" ", ""));
        }
    }


    public void NewSave()
    {
        _saveManager.NewSave(false, _saveManager.CurrentProfile);
    }

    public void ShowProfiles()
    {
        if (!_profilesPanel.activeSelf) return;
        //Debug.Log("show profiles");

        // Destroy all
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

        _saveManager.GetProfilesDirectories();
        _saveManager.GetAllProfilesSaves();
    }

    public void SetProfiles()
    {
        Dictionary<string, List<SaveData>> dict = new Dictionary<string, List<SaveData>>();
        dict = _saveManager.DictProfileSaveDatas;

        //Debug.Log($"dict count : {dict.Count}");
        if (dict.Count == 0) return;

        foreach (var entry in dict)
        {
            string key = entry.Key;
            List<SaveData> value = entry.Value;

            //Debug.Log($"Key: {key}");
            //Debug.Log("Value:");
            foreach (var data in value)
            {
                //Debug.Log($"{data.SaveInfos.GUID}");
            }
        }

        foreach (string profileName in _saveManager.ProfilesNames)
        {
            GameObject profile = Instantiate(_prefabProfile);
            profile.transform.SetParent(_contentProfiles.transform);
            Profile profileSlot = profile.GetComponent<Profile>();

            //Debug.Log("set profile name " + profileName);
            profileSlot.SetProfileName(profileName.ToUpper());

            //Debug.Log("set profile data");
            //Debug.Log($"dict save count : {dict[profileName].Count}");
            profileSlot.SetData(dict[profileName][0]);
        }
    }

    public void ShowSaves()
    {
        if (!_saveAndLoadPanel.activeSelf) return;
        //Debug.Log("show saves");

        // Destroy all
        GameObject[] allChildren = new GameObject[_contentSaves.transform.childCount];
        int tempIndex = 0;

        foreach (Transform child in _contentSaves.transform)
        {
            allChildren[tempIndex] = child.gameObject;
            ++tempIndex;
        }
        foreach (GameObject child in allChildren)
        {
            Destroy(child.gameObject);
        }

        _saveSlots.Clear();
        _saveManager.GetCurrentProfileSaves();
        int i = 0;
        foreach (SaveData saveData in _saveManager.CurrentProfileSaves)
        {
            //Debug.Log("instantiate");
            GameObject saveGameObject = Instantiate(_prefabSave);
            saveGameObject.transform.SetParent(_contentSaves.transform);
            SaveSlot saveSlot = saveGameObject.GetComponent<SaveSlot>();
            _saveSlots.Add(saveSlot);

            if (i == 0)
                saveSlot.InitSaveSlot(saveData, true);
            else
                saveSlot.InitSaveSlot(saveData, false);
            i++;

        }

        if (_saveManager.CurrentProfileSaves.Count <= 1)
            foreach(SaveSlot saveSlot in _saveSlots) { saveSlot.SetCanDelete(false);  }
        else
            foreach (SaveSlot saveSlot in _saveSlots) { saveSlot.SetCanDelete(true); }

        //Debug.Log("end instantiate");
    }

    public void DeselectAllSaveSlots()
    {
        foreach(SaveSlot saveSlot in _saveSlots)
        {
            saveSlot.SetIsLoaded(false);
        }
    }
}
