using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowScreen : MonoBehaviour
{
    public GameObject DeathUI;
    public GameObject score;
    private bool shown = false;
    void Update()
    {
        if (GameManager.instance.gameOverState && !shown)
        {
            shown = true;
            DeathUI.SetActive(true); ;
            score.GetComponent<Text>().text = "" + GameManager.instance.GetScore();
        }
    }
}
