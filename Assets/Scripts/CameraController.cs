using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Hedef ve Takip Ayarları")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 7.5f, -7f);
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private bool followX = true;

    private void LateUpdate()
    {
        // Hedef yoksa (veya Morty yeni oluştysa) PlayerManager'dan otomatik bul
        if (target == null)
        {
            if (PlayerManager.Instance != null)
            {
                target = PlayerManager.Instance.transform;
            }
            else
            {
                return; // Karakter henüz doğmadıysa bekle
            }
        }

        Vector3 targetPosition;

        if (followX)
        {
            targetPosition = target.position + offset;
        }
        else
        {
            targetPosition = new Vector3(transform.position.x, target.position.y + offset.y, target.position.z + offset.z);
        }

        // Kamerayı yumuşakça hedefe taşı
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);
    }
}