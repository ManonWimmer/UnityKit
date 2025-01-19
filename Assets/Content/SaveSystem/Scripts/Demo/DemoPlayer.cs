using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoPlayer : MonoBehaviour
{
    public int PlayerHealth = 100;
    public int PlayerLevel = 1;

    [SerializeField] private float _minXPosition = 0f;
    [SerializeField] private float _maxXPosition = 0f;

    [SerializeField] private float _minYPosition = 0f;
    [SerializeField] private float _maxYPosition = 0f;

    public void Lose10Health()
    {
        PlayerHealth -= 10;
        if (PlayerHealth < 0 ) PlayerHealth = 0;
    }

    public void Gain10Health()
    {
        PlayerHealth += 10;
    }

    public void Lose1Level()
    {
        PlayerLevel -= 1;
        if (PlayerLevel < 0) PlayerLevel = 0;
    }

    public void Gain1Level()
    {
        PlayerLevel += 1;
    }

    public void RandomXPositon()
    {
        transform.position = new Vector3(Random.Range(_minXPosition, _maxXPosition), transform.position.y, transform.position.z);
    }

    public void RandomYPositon()
    {
        transform.position = new Vector3(transform.position.y, Random.Range(_minYPosition, _maxYPosition), transform.position.z);
    }
}
