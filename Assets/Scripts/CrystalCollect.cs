using UnityEngine;

public class CrystalCollect : MonoBehaviour
{
    private bool playerNear = false;

    void Update()
    {
        if(playerNear && Input.GetKeyDown(KeyCode.E))
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
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            playerNear = false;
        }
    }
}