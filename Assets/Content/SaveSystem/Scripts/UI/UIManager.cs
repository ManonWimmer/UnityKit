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

    [SerializeField] TMP_Text _profileNameInSavesTxt;

    [SerializeField] GameObject _contentSaves;
    [SerializeField] GameObject _prefabSave;

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
        _saveManager.OnAddSave += ShowSaves;
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
        _saveManager.NewSave();
    }

    public void ShowProfiles()
    {
        Debug.Log("show profiles");

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

        foreach (string profileName in _saveManager.ProfilesNames)
        {
            GameObject profile = Instantiate(_prefabProfile);
            profile.transform.SetParent(_contentProfiles.transform);
            Profile profileSlot = profile.GetComponent<Profile>();
            profileSlot.SetProfileName(profileName.ToUpper());
            profileSlot.SetData(_saveManager.DictProfileSaveDatas[profileName][0]);
        }
    }

    public void ShowSaves()
    {
        Debug.Log("show saves");

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

        _saveManager.GetCurrentProfileSaves();
        foreach (SaveData saveData in _saveManager.CurrentProfileSaves)
        {
            Debug.Log("instantiate");
            GameObject saveGameObject = Instantiate(_prefabSave);
            saveGameObject.transform.SetParent(_contentSaves.transform);
            SaveSlot saveSlot = saveGameObject.GetComponent<SaveSlot>();
            saveSlot.InitSaveSlot(saveData);
        }
        Debug.Log("end instantiate");
    }
}
