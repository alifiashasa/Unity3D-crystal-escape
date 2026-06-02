using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class LevelTimer : MonoBehaviour
{
    public float timeRemaining = 120f;

    public TextMeshProUGUI timerText;

    private bool timerRunning = true;

    void Update()
    {
        if(timerRunning)
        {
            if(timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;

                UpdateTimer();
            }
            else
            {
                timeRemaining = 0;
                timerRunning = false;

                GameOver();
            }
        }
    }

    void UpdateTimer()
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60);
        int seconds = Mathf.FloorToInt(timeRemaining % 60);

        timerText.text = 
            string.Format("Time : {0:00}:{1:00}",
            minutes,
            seconds);
    }

    void GameOver()
    {
        Debug.Log("GAME OVER");

        // nanti bisa pindah scene game over
    }
}