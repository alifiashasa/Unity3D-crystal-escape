using UnityEngine;

public class CrystalCollect : MonoBehaviour
{
    private bool playerNear = false;

    void Update()
    {
        if(playerNear && (Input.GetKeyDown(KeyCode.E) || (MobileControls.instance != null && MobileControls.instance.IsInteractPressedThisFrame)))
        {
            GameManager.instance.AddCrystal();

            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerNear = true;
            if (MobileControls.instance != null)
            {
                MobileControls.instance.SetInteractActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerNear = false;
            if (MobileControls.instance != null)
            {
                MobileControls.instance.SetInteractActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        if (playerNear && MobileControls.instance != null)
        {
            MobileControls.instance.SetInteractActive(false);
        }
    }
}