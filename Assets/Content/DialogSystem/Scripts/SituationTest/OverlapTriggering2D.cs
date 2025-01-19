using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OverlapTriggering2D : MonoBehaviour
{
    [SerializeField] private string _tagsToCheck;


    public UnityEvent OnTriggerEnterUnity;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null) return;

        if (collision.gameObject.CompareTag(_tagsToCheck))
        {
            OnTriggerEnterUnity?.Invoke();
            Debug.Log("UNity event");
        }
    }
}
