using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    [Header("Oyuncu Takip Ayarları")]
    [SerializeField] private Transform target;
    [SerializeField] private Vector3 offset = new Vector3(0f, 15f, -16f);
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private bool followX = true;

    [Header("Bitiş Çizgisi Sinematik Kamera Ayarları")]
    [SerializeField] private Vector3 finishOffset = new Vector3(-8f, 10f, -12f); // Yan ve yüksek açı
    [SerializeField] private Vector3 finishRotation = new Vector3(25f, 35f, 0f);  // Karakterlere yan çaprazdan bakan açı
    [SerializeField] private float finishTransitionSpeed = 2.5f;                 // Yumuşak geçiş hızı

    private bool isFinishView = false;
    private Quaternion targetFinishRotation;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void LateUpdate()
    {
        // Hedef yoksa sahnedeki Morty'yi bulmayı dene
        if (target == null)
        {
            if (PlayerMovement.Instance != null)
                target = PlayerMovement.Instance.transform;
            else
                return;
        }

        if (isFinishView)
        {
            // 1. Bitiş Çizgisi Sonrası: Yan/Çapraz Sinematik Açıya Yumuşakça Geç
            Vector3 desiredPosition = target.position + finishOffset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * finishTransitionSpeed);

            // Kameranın rotasyonunu yavaşça yan açıya döndür
            transform.rotation = Quaternion.Slerp(transform.rotation, targetFinishRotation, Time.deltaTime * finishTransitionSpeed);
        }
        else
        {
            // 2. Normal Oyun İçi Takip
            Vector3 targetPos = target.position;
            float targetX = followX ? targetPos.x : 0f;
            Vector3 desiredPosition = new Vector3(targetX, 0f, targetPos.z) + offset;

            transform.position = Vector3.Lerp(transform.position, desiredPosition, Time.deltaTime * smoothSpeed);
        }
    }

    // FinishLine.cs tarafından bitiş çizgisinde çağrılır
    public void SwitchToFinishView()
    {
        isFinishView = true;
        targetFinishRotation = Quaternion.Euler(finishRotation);
    }

    // Yeni seviye başladığında kamerayı eski haline getirir
    public void ResetCamera()
    {
        isFinishView = false;
        transform.rotation = Quaternion.Euler(25f, 0f, 0f); // Varsayılan kamera eğimi
    }
}