using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoPlayer : MonoBehaviour
{
    public int PlayerHealth = 100;
    public int PlayerLevel = 1;

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
}
