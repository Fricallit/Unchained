using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadScores : MonoBehaviour
{
    void Start()
    {
        GameManager.instance.LoadSavedScores();
    }
}
