using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CREMOT.DialogSystem
{
    [DisallowMultipleComponent]
    public class PersistentGUID : MonoBehaviour
    {
        [SerializeField, ReadOnly] private string _gUID;

        public string GUID { get => _gUID; set => _gUID = value; }


        private void Reset()
        {
            GenerateGUID();
        }

        public void GenerateGUID()
        {
            if (string.IsNullOrEmpty(_gUID))
            {
                _gUID = Guid.NewGuid().ToString();
            }
        }
    }
}
