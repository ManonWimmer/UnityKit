using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DemoUIManager : MonoBehaviour
{
    // ----- FIELDS ----- //
    [SerializeField] private DemoPlayer _demoPlayer;

    [SerializeField] private TMP_Text _playerHealthTxt;
    [SerializeField] private TMP_Text _playerLevelTxt;
    [SerializeField] private TMP_Text _playerPositionTxt;

    [SerializeField] private RectTransform _playerRectTransform;
    // ----- FIELDS ----- //

    private void Update()
    {
        _playerHealthTxt.text = $"PLAYER HEALTH : {_demoPlayer.PlayerHealth}";
        _playerLevelTxt.text = $"PLAYER LEVEL : {_demoPlayer.PlayerLevel}";
        _playerPositionTxt.text = $"PLAYER POSITION : {_demoPlayer.transform.position}";
    }
}
