using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider bar;
    void Start()
    {
        
    }

    public void setMaxHealth(int health)
    {
        bar.maxValue = health;
    }

    public void changeHealth(int health)
    {
        bar.value = health;
    }
}
