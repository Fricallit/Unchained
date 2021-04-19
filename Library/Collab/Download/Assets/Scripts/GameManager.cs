using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class GameManager : MonoBehaviour
{
    public int playerHitPoints = 150;
    public static GameManager instance = null;
    public DungeonManager dungeonScript;
    public GameObject statusItem;
    public Texture2D menuCursor;
    public Texture2D gameCursor;
    public float transitionTime = 1f;
    public enum GameState
    {
        inMenu, newGame, loadGame
    }

    private GameState state;
    private int difficultyLevel = 0;
    private GameObject statusHolder;
    private GameObject statusBox;
    private GameObject scoreItem;
    private GameObject hud;
    private Camera cam;
    private float deathScreenShowTime = 2f;
    private bool gamePaused = true;
    public bool gameOverState = false;
    private int score = 0;
    private int highScore = 0;
    private int highestLevel = 0;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);

        Cursor.SetCursor(menuCursor, Vector2.zero, CursorMode.ForceSoftware);

        DontDestroyOnLoad(gameObject);
        instance.state = GameState.inMenu;
        dungeonScript = GetComponent<DungeonManager>();
        Debug.Log(highestLevel);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static public void CallbackInitialization()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    static private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        instance.gameOverState = false;
        if (!(instance.state == GameState.inMenu))
        {
            if (instance.state == GameState.loadGame)
            {
                Debug.Log("In loading game");
                instance.ContinueGame();
            }
            else
            {
                Debug.Log("In new game");
            }
            instance.InitGame();
            instance.difficultyLevel++;
            instance.ResumeGame();
        }
        else
        {
            Debug.Log("In menu");
            instance.difficultyLevel = 1;
            instance.playerHitPoints = 150;
        }
    }

    public void LoadSavedScores()
    {
        if (PlayerPrefs.HasKey("hiscore") && PlayerPrefs.HasKey("hilevel"))
        {
            highScore = PlayerPrefs.GetInt("hiscore");
            highestLevel = PlayerPrefs.GetInt("hilevel");
        }
    }

    public void ResetGameManagerScores()
    {
        highestLevel = 0;
        highScore = 0;
    }

    public void SaveGame()
    {

    }

    public int GetHiScore()
    {
        return highScore;
    }

    public int GetHighestLevel()
    {
        return highestLevel;
    }

    public void SetGameState(GameState st)
    {
        instance.state = st;
    }

    public bool isGamePaused()
    {
        return gamePaused;
    }

    public void ResumeGame()
    {
        gamePaused = false;
        Time.timeScale = 1;
    }

    public void PauseGame()
    {
        gamePaused = true;
        Time.timeScale = 0;
    }

    public void SetGameCursor()
    {
        Cursor.SetCursor(gameCursor, Vector2.zero, CursorMode.ForceSoftware);
    }

    public void SetMenuCursor()
    {
        Cursor.SetCursor(menuCursor, Vector2.zero, CursorMode.ForceSoftware);
    }

    void InitGame()
    {
        Cursor.SetCursor(gameCursor, Vector2.zero, CursorMode.ForceSoftware);
        deathScreenShowTime = 2f;
        score = 0;
        statusHolder = GameObject.Find("StatusHolder");
        hud = GameObject.Find("HUD");
        statusBox = GameObject.Find("StatusBox");
        scoreItem = GameObject.Find("Score");
        cam = GameObject.Find("MainCamera").GetComponent<Camera>();
        scoreItem.GetComponent<Text>().text = "" + score;
        UpdateStatus("Entered level " + difficultyLevel);
        dungeonScript.SetupScene(difficultyLevel);
    }

    public void SaveScore()
    {
        if (difficultyLevel > highestLevel)
        {
            highestLevel = difficultyLevel;
            PlayerPrefs.SetInt("hilevel", highestLevel);
        }
        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("hiscore", highScore);
        }
    }
    public Camera getCamera()
    {
        return cam;
    }

    public void AddScore(int sc)
    {
        score += sc;
        scoreItem.GetComponent<Text>().text = "" + score;
    }

    public int GetScore()
    {
        return score;
    }

    public void NextLevel()
    {
        Debug.Log("Initiated scene reloading");
        GameObject.Find("PauseMenuCanvas").SetActive(false);
        instance.state = GameState.newGame;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    public void GoToMainMenu()
    {
        instance.state = GameState.inMenu;
        SceneManager.LoadScene(0, LoadSceneMode.Single);
    }

    public void GameOver()
    {
        UpdateStatus("Game Over");
        hud.SetActive(false);

        Cursor.SetCursor(menuCursor, Vector2.zero, CursorMode.ForceSoftware);

        SaveScore();

        gameOverState = true;
    }

    public void UpdateStatus(string message)
    {
        GameObject toInstantiate = statusItem;
        GameObject itemInstance = Instantiate(toInstantiate);
        itemInstance.GetComponentInChildren<Text>().text = message;
        itemInstance.transform.SetParent(statusHolder.transform);
        statusBox.GetComponent<ScrollRect>().velocity = new Vector2(0, 100);
    }

    public void ContinueGame()
    {
        Debug.Log("Continue the game!");
    }

    private void Update()
    {
        if (gameOverState)
        {
            deathScreenShowTime -= Time.unscaledDeltaTime;
            if (deathScreenShowTime < 0)
            {
                if (Input.anyKey)
                {
                    GoToMainMenu();
                }
            }
        }
    }
}
