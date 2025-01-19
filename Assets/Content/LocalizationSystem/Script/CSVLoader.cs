using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using static LocalizationSystem;
using static System.Net.WebRequestMethods;
using static Unity.VisualScripting.StickyNote;

public class CSVLoader : MonoBehaviour
{
    private static string CSVText;
    private static List<List<string>> sortCSV;
    private static int indexOffset = 1;

    private bool isLoad = false;
    public delegate void CSVisLoad();
    public static event CSVisLoad csvIsLoad;
    [SerializeField] private string webLinkCSV = "https://docs.google.com/spreadsheets/d/e/2PACX-1vSXqS5FKCsIi3my0kWlN2RHHga0aSIUu7gs3SEM3jj1TtUS2Pxm7NRfWXWxVk0jgVlo16IPRBYhcuYK/pub?output=csv";

    void Start()
    {
        StartCoroutine(GetRequest(webLinkCSV));
    }

    IEnumerator GetRequest(string uri)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(uri))
        {
            // Request and wait for the desired page.
            yield return webRequest.SendWebRequest();
            switch (webRequest.result)
            {
                case UnityWebRequest.Result.ConnectionError:
                    Debug.LogError("No connection");
                    break;
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    //Debug.Log("Received: " + webRequest.downloadHandler.text);
                    if (!isLoad)
                    {
                        CSVText = webRequest.downloadHandler.text;
                        CSVtoList();
                        csvIsLoad?.Invoke();
                        isLoad = true;
                    }
                    break;
            }
        }
    }


    static void CSVtoList()
    {
        string[] listContainList;
        string csvText = CSVText;
        csvText = csvText.Replace("\"", "");
        listContainList = csvText.Split("\n");
        List<List<string>> result = new List<List<string>>();
        foreach (string line in listContainList)
        {
            string[] words = line.Split(',');
            result.Add(new List<string>(words));
        }
        sortCSV = result;
    }

    Dictionary<string, Dictionary<Enum, string>> GetDialogueDictionary()
    {
        Dictionary<Enum, string> dicoLngTxt = new Dictionary<Enum, string>();
        Dictionary<string, Dictionary<Enum, string>> resultDico = new Dictionary<string, Dictionary<Enum, string>>();
        if (!isLoad)
        {
            return resultDico;
        }
        for (int i = 0; i < sortCSV.Count; i++)
        {
            for(int y = 0; y < 3; y++)
            {
                dicoLngTxt.Add((Language)y, sortCSV[i][y+2]);
            }
            resultDico.Add(sortCSV[i][1],dicoLngTxt);
            dicoLngTxt.Clear();
        }
        return resultDico;
    }

    public static Dictionary<string,string> GetDictionaryValues(LocalizationSystem.Language language)
    {
        Dictionary<string, string> DictionaryValues = new Dictionary<string, string>();
        foreach (List<string> line in sortCSV)
        {
            if(indexOffset + ((int)language) >= line.Count) 
            {
                DictionaryValues.Add(line[indexOffset], string.Format("The keyword {0} isn't defined in {1}.", line[indexOffset], language));
            }
            else
            {
                DictionaryValues.Add(line[indexOffset], line[indexOffset + ((int)language)]);
            }
        }
        return DictionaryValues;
    }
    //EditorUtility.SaveFilePanelInProject("asset");
}
