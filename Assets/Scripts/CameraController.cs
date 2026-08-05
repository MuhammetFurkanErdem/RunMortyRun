using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Hedef ve Takip Ayarları")]
    [SerializeField] private Transform target;           // Takip edilecek Oyuncu (Morty Smith)
    [SerializeField] private Vector3 offset = new Vector3(0f, 7.5f, -7f); // Kameranın duracağı açı/mesafe
    [SerializeField] private float smoothSpeed = 10f;    // Takip yumuşaklığı

    [Header("Kamera X Eksen Ayarı")]
    [SerializeField] private bool followX = true;        // Kamera sağa-sola oyuncuyla gitsin mi?

    private void LateUpdate()
    {
        if (target == null) return;

        // Kameranın gitmek istediği ideal pozisyon
        Vector3 targetPosition = target.position + offset;

        // Eğer kameranın sağa-sola oyuncuyla kaymasını istemiyorsan X'i sabitleyebilirsin
        if (!followX)
        {
            targetPosition.x = offset.x;
        }

        // Yumuşak geçiş (Lerp) ile kamerayı taşı
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}