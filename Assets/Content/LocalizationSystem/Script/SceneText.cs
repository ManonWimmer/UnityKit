using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class SceneText : MonoBehaviour
{
    [SerializeField] public Text[] listTexts;

    public void GetAllText()
    {
        listTexts = FindObjectsOfType<Text>();
    }
}
