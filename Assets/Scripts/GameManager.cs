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

    [Header("Audio")]
    [SerializeField] private AudioClip collectSound;
    private AudioSource audioSource;

    private bool levelComplete = false;
    private bool isGameOver = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
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