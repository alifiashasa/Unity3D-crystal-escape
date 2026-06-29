using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int crystalCount = 0;

    public TextMeshProUGUI crystalText;

    public GameObject winPanel;
    public GameObject gameOverPanel;
    public GameObject pausePanel;

    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;
    private AudioSource audioSource;

    private bool levelComplete = false;
    private bool isGameOver = false;
    private bool isPaused = false;

    public bool IsPaused => isPaused;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        // Lock cursor at start
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;

        if (gameOverPanel != null)
        {
            Transform restartBtnTransform = gameOverPanel.transform.Find("RestartButton");
            if (restartBtnTransform == null)
            {
                restartBtnTransform = gameOverPanel.transform.Find("NextLevel");
            }
            if (restartBtnTransform != null)
            {
                Button restartBtn = restartBtnTransform.GetComponent<Button>();
                if (restartBtn != null)
                {
                    restartBtn.onClick.RemoveAllListeners();
                    restartBtn.onClick.AddListener(RestartGame);
                }
            }

            Transform mainMenuBtnTransform = gameOverPanel.transform.Find("MainMenu");
            if (mainMenuBtnTransform != null)
            {
                Button mainMenuBtn = mainMenuBtnTransform.GetComponent<Button>();
                if (mainMenuBtn != null)
                {
                    mainMenuBtn.onClick.RemoveAllListeners();
                    mainMenuBtn.onClick.AddListener(MainMenu);
                }
            }
        }

        if (pausePanel != null)
        {
            Transform resumeBtnTransform = pausePanel.transform.Find("ResumeButton");
            if (resumeBtnTransform != null)
            {
                Button resumeBtn = resumeBtnTransform.GetComponent<Button>();
                if (resumeBtn != null)
                {
                    resumeBtn.onClick.RemoveAllListeners();
                    resumeBtn.onClick.AddListener(ResumeGame);
                }
            }

            Transform restartBtnTransform = pausePanel.transform.Find("RestartButton");
            if (restartBtnTransform != null)
            {
                Button restartBtn = restartBtnTransform.GetComponent<Button>();
                if (restartBtn != null)
                {
                    restartBtn.onClick.RemoveAllListeners();
                    restartBtn.onClick.AddListener(RestartGame);
                }
            }

            Transform mainMenuBtnTransform = pausePanel.transform.Find("MainMenu");
            if (mainMenuBtnTransform != null)
            {
                Button mainMenuBtn = mainMenuBtnTransform.GetComponent<Button>();
                if (mainMenuBtn != null)
                {
                    mainMenuBtn.onClick.RemoveAllListeners();
                    mainMenuBtn.onClick.AddListener(MainMenu);
                }
            }
        }

        // Find Pause Button in gameplay UI and bind it
        GameObject timerCanvas = GameObject.Find("TimerImage");
        if (timerCanvas != null)
        {
            Transform pauseBtnTransform = timerCanvas.transform.Find("PauseButton");
            if (pauseBtnTransform != null)
            {
                Button pauseBtn = pauseBtnTransform.GetComponent<Button>();
                if (pauseBtn != null)
                {
                    pauseBtn.onClick.RemoveAllListeners();
                    pauseBtn.onClick.AddListener(TogglePause);
                }
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!levelComplete && !isGameOver)
            {
                TogglePause();
            }
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }

        if (MobileControls.instance != null)
        {
            MobileControls.instance.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }

        if (MobileControls.instance != null)
        {
            MobileControls.instance.SetActive(true);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
    }

    public void AddCrystal()
    {
        if(levelComplete || isGameOver) return;

        if (collectSound != null)
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null)
                {
                    audioSource = gameObject.AddComponent<AudioSource>();
                }
            }
            audioSource.PlayOneShot(collectSound);
        }

        crystalCount++;

        crystalText.text =
            "Crystal : " + crystalCount + "/3";

        if(crystalCount >= 3)
        {
            LevelComplete();
        }
    }

    void LevelComplete()
    {
        levelComplete = true;

        winPanel.SetActive(true);

        if (MobileControls.instance != null)
        {
            MobileControls.instance.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    public void GameOver()
    {
        if (levelComplete || isGameOver) return;

        isGameOver = true;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (MobileControls.instance != null)
        {
            MobileControls.instance.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void NextLevel()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex + 1
        );
    }

    public void MainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("Main Menu");
    }
}