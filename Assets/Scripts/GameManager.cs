using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int crystalCount = 0;

    public TextMeshProUGUI crystalText;

    [SerializeField] private AudioClip collectCrystalSound;

    private void Awake()
    {
        instance = this;
    }

    public void AddCrystal()
    {
        crystalCount++;

        crystalText.text = "Crystal : " + crystalCount + "/3";

        if (collectCrystalSound != null)
        {
            AudioSource cameraAudio = Camera.main.GetComponent<AudioSource>();
            if (cameraAudio == null)
            {
                cameraAudio = Camera.main.gameObject.AddComponent<AudioSource>();
            }
            cameraAudio.PlayOneShot(collectCrystalSound);
        }
    }
}