using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    [SerializeField] private int _privateValue = 10;
    [SerializeField] private string _privateString = "Private String";

    public string PublicString = "Public String";
}
