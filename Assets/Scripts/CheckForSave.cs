using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CheckForSave : MonoBehaviour
{
    void Start()
    {
        if (!GameManager.instance.saveSystem.SaveExists())
        {
            Image continueButton = GameObject.Find("ContinueButton").GetComponent<Image>();
            continueButton.color = new Color(255, 255, 255, 80);
            Image continueText = GameObject.Find("ContinueText").GetComponent<Image>();
            continueText.color = new Color(255, 255, 255, 80);
        }
    }
}
