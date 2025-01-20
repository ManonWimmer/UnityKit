using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class OverlapTriggering2D : MonoBehaviour
{
    [SerializeField] private List<string> _tagsToCheck;


    public UnityEvent OnTriggerEnterUnity;

    public UnityEvent OnTriggerExitUnity;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision == null) return;

        if (_tagsToCheck.Contains(collision.gameObject.tag))
        {
            OnTriggerEnterUnity?.Invoke();
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision == null) return;

        if (_tagsToCheck.Contains(collision.gameObject.tag))
        {
            OnTriggerExitUnity?.Invoke();
        }
    }
}
