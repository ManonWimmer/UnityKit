using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GetSceneText : MonoBehaviour
{
    [SerializeField] private static List<Text> listText = new List<Text>();
    void GetAllTextinScene()
    {
        listText.Clear();
        //listText = FindAnyObjectByType<Text>;
    }
    
}
