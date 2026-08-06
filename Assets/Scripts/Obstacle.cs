using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [Header("Engel Ayarları")]
    [SerializeField] private int damageAmount = 1; // Değecek her karakter için kaç kişi eksilteceği

    private void OnTriggerEnter(Collider other)
    {
        // Temas eden obje "Player" veya "SubPlayer" etiketi taşıyorsa
        if (other.CompareTag("Player"))
        {
            // Eğer temasa geçen obje bir klon ise veya ana oyuncu ise
            // Karakter yönetim script'i üzerinden eksiltme çağrısı yapalım
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.RemovePlayer(other.gameObject);
            }
            else
            {
                // Alternatif fallback: PlayerManager yoksa doğrudan objeyi yok et
                Destroy(other.gameObject);
            }
        }
    }
}