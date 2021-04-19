using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayNew()
    {
        GameManager.instance.SetGameState(GameManager.GameState.newGame);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ContinuePlaying()
    {
        GameManager.instance.SetGameState(GameManager.GameState.loadGame);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void ExitGame()
    {
        Debug.Log("Quitted");
        Application.Quit();
    }

    public void DeleteScores()
    {
        PlayerPrefs.DeleteAll();
        GameManager.instance.ResetGameManagerScores();
        GameManager.instance.GoToMainMenu();
    }
}
