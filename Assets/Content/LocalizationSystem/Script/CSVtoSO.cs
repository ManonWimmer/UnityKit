using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.Rendering;
using static Unity.VisualScripting.Icons;


public enum Language   //The language has to be in order to correspond to his column in the CSV
{
    FR = 1,
    EN = 2,
    ES = 3
}


public class CSVtoSO : MonoBehaviour
{
    private static string CSVLocalisationPath = "Assets/Content/LocalizationSystem/Localisation.csv";
    private static string[] AllLines = File.ReadAllLines(CSVLocalisationPath);
    private int id = 0;


    [MenuItem("Utilities/Localisation/Francais")]
    private static void FR()
    {
        CSVtoSO instance = new CSVtoSO();
        instance.GenerateSO(Language.FR);
    }

    [MenuItem("Utilities/Localisation/English")]
    private static void EN()
    {
        CSVtoSO instance = new CSVtoSO();
        instance.GenerateSO(Language.EN);
    }

    [MenuItem("Utilities/Localisation/Español")]
    private static void ES()
    {
        CSVtoSO instance = new CSVtoSO();
        instance.GenerateSO(Language.ES);
    }

    private void GenerateSO(Language language)
    {
        string[] SplitData = AllLines[((int)language)].Split(',');
        foreach (string text in SplitData) 
        {
            SODialogue Dialogue = ScriptableObject.CreateInstance<SODialogue>();
            Dialogue.text = text;
            Dialogue.id = id;
            id++;
            AssetDatabase.CreateAsset(Dialogue, $"Assets/Content/LocalizationSystem/SO/{Dialogue.id}.asset" );
        }
    }
}
