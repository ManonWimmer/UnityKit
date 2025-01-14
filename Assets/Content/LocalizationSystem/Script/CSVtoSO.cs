using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;
using UnityEngine.Rendering;
using static Unity.VisualScripting.Icons;


public enum Language   //The language has to be in order to correspond to his column in the CSV. Don't use 0 it correspond to the column of language
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

    #region Menu
    [MenuItem("Utilities/Localization/Francais")]
    private static void FR()
    {
        CSVtoSO instance = new CSVtoSO();
        instance.GenerateSO(Language.FR);
    }

    [MenuItem("Utilities/Localization/English")]
    private static void EN()
    {
        CSVtoSO instance = new CSVtoSO();
        instance.GenerateSO(Language.EN);
    }

    [MenuItem("Utilities/Localization/Español")]
    private static void ES()
    {
        CSVtoSO instance = new CSVtoSO();
        instance.GenerateSO(Language.ES);
    }
    #endregion

    private void GenerateSO(Language language)
    {
        string[] SplitData = AllLines[((int)language)].Split(',');
        foreach (string text in SplitData) 
        {
            SODialogue Dialogue = ScriptableObject.CreateInstance<SODialogue>();
            Dialogue.text = text;
            Dialogue.id = id;
            if (id != 0)
            {
                AssetDatabase.CreateAsset(Dialogue, $"Assets/Content/LocalizationSystem/SO/{Dialogue.id}.asset");
            }
            id++;
            
        }
    }
}
