using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Header("Engel Ayarları")]
    [SerializeField] private int damageAmount = 1; // Inspector'dan değiştirdiğiniz miktar

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (PlayerManager.Instance != null)
            {
                // Hasar miktarını (damageAmount) PlayerManager'a gönderiyoruz
                PlayerManager.Instance.RemovePlayer(other.gameObject, damageAmount);
            }
            else
            {
                Destroy(other.gameObject);
            }
        }

        if (other.CompareTag("Player"))
        {
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.RemovePlayer(other.gameObject, damageAmount);
            }

            // Ses ve Kamera Shake
            if (AudioManager.Instance != null) AudioManager.Instance.PlayCharacterDeath();
            if (CameraShake.Instance != null) CameraShake.Instance.Shake(0.15f, 0.25f);
        }
    }
}