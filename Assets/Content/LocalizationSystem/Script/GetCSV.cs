using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class GetCSV : MonoBehaviour
{
    [SerializeField] LocalizationSystem localizationSystem;
    private string CSVText;
    private int indexOffset = 1;
    private 
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
                    break;
            }
        }
    }

    string[][] CSVtoList()
    {
        string[][] listValues;
        CSVText = CSVText.Replace("\"", "");
        //listValues = CSVText.Split("");
        return null;
    }

    private Dictionary<string,string> GetDictionaryValues(string language)
    {
        string[][] listData = CSVtoList();
        Dictionary<string, string> DictionaryValues = new Dictionary<string, string>();
        switch (language) 
        {
            case "FR":
                foreach(string[] line in listData)
                {
                    DictionaryValues.Add(line[indexOffset + 0], line[indexOffset + 1]);
                }
                break;
            case "EN":
                foreach (string[] line in listData)
                {
                    DictionaryValues.Add(line[indexOffset + 0], line[indexOffset + 2]);
                }
                break;
            case "ES":
                foreach (string[] line in listData)
                {
                    DictionaryValues.Add(line[indexOffset + 0], line[indexOffset + 3]);
                }
                break;
        }
        return DictionaryValues;

    }
    //EditorUtility.SaveFilePanelInProject("asset");

}
