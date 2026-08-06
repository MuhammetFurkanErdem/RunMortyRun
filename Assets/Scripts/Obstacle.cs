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
    }
}