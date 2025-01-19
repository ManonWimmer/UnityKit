using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using static Unity.VisualScripting.StickyNote;

public class CSVLoader : MonoBehaviour
{
    private static string CSVText;
    private static List<List<string>> sortCSV;
    private static int indexOffset = 1;
    LocalizationSystem localizationSystem;

    private bool isLoad = false;
    public delegate void CSVisLoad();
    public event CSVisLoad csvIsLoad;

    void Start()
    {
        StartCoroutine(GetRequest("https://docs.google.com/spreadsheets/d/e/2PACX-1vSXqS5FKCsIi3my0kWlN2RHHga0aSIUu7gs3SEM3jj1TtUS2Pxm7NRfWXWxVk0jgVlo16IPRBYhcuYK/pub?output=csv"));
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
                case UnityWebRequest.Result.DataProcessingError:
                    Debug.LogError("Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.ProtocolError:
                    Debug.LogError("HTTP Error: " + webRequest.error);
                    break;
                case UnityWebRequest.Result.Success:
                    //Debug.Log("Received: " + webRequest.downloadHandler.text);
                    CSVText = webRequest.downloadHandler.text;
                    if (!isLoad)
                    {
                        CSVtoList();
                        csvIsLoad();
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

    public static Dictionary<string,string> GetDictionaryValues(LocalizationSystem.Language language)
    {
        Dictionary<string, string> DictionaryValues = new Dictionary<string, string>();
        foreach (List<string> line in sortCSV)
        {
            if(indexOffset + ((int)language) >= line.Count) 
            {
                DictionaryValues.Add(line[indexOffset], "Text not defined in the CSV");
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
