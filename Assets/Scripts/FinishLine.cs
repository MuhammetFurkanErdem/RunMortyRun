using UnityEngine;

public class FinishLine : MonoBehaviour
{
    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if (other.CompareTag("Player"))
        {
            isTriggered = true;

            // GameManager'a Seviye Tamamlandı haberini gönder
            if (GameManager.Instance != null)
            {
                GameManager.Instance.LevelCompleted();
            }
        }
    }
}