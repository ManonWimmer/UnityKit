using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] GameObject _profilesPanel;
    [SerializeField] GameObject _saveAndLoadPanel;

    private void Start()
    {
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
}
