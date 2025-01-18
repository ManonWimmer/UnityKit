using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    [SerializeField] private int _privateValue = 10;
    [SerializeField] private string _privateString = "Apagan";

    public string PublicString = "Apagan";

    public string PrivateString { get => _privateString; set => _privateString = value; }
}
