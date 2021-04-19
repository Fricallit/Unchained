using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenuUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!GameManager.instance.gameOverState)
            {
                if (GameManager.instance.isGamePaused())
                    Resume();
                else
                    Pause();
            }
        }
    }
    public void Resume()
    {
        GameManager.instance.SetGameCursor();
        pauseMenuUI.SetActive(false);
        GameManager.instance.ResumeGame();
    }

    public  void Pause()
    {
        GameManager.instance.SetMenuCursor();
        pauseMenuUI.SetActive(true);
        GameManager.instance.PauseGame();
    }

    public void exitToMenu()
    {
        GameManager.instance.SaveScore();
        GameManager.instance.SaveGame();
        GameManager.instance.ResumeGame();
        GameManager.instance.GoToMainMenu();
    }
}
