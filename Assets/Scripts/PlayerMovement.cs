using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Koşu Hızları")]
    [SerializeField] private float forwardSpeed = 7.5f;
    [SerializeField] private float sideSpeed = 10f;
    [SerializeField] private float limitX = 4.5f;

    [Header("Yumuşatma ve Dönüş Ayarları")]
    [SerializeField] private float positionLerpSpeed = 15f; // Sağa-sola kayma yumuşatması
    [SerializeField] private float rotationSpeed = 10f;    // Çapraza dönme yumuşatması
    [SerializeField] private float maxTiltAngle = 20f;      // Sağa/sola kayarken kaç derece yatacağı

    private Animator animator;
    private float targetXPosition;
    private float currentHorizontalInput;

    private void Start()
    {
        animator = GetComponent<Animator>();
        targetXPosition = transform.position.x;
    }

    private void Update()
    {
        // Game Over durumunda hareketi kes
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.GameOver)
        {
            if (animator != null) animator.speed = 0f;
            return;
        }

        // Oyun başlamadıysa tuş girdisi bekle
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.NotStarted)
        {
            CheckForStartInput();
            return;
        }

        // Oyun başladıysa hareket et
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            HandleInput();
            MovePlayer();
            ApplySmoothRotation();
        }
    }

    private void CheckForStartInput()
    {
        if (Input.anyKeyDown || Input.GetMouseButtonDown(0))
        {
            GameManager.Instance.StartGame();
            if (animator != null) animator.speed = 1f;
        }
    }

    private void HandleInput()
    {
        // Klavyeden veya Mobil/Fare girdisinden yön al
        currentHorizontalInput = Input.GetAxis("Horizontal");

        // Hedef X pozisyonunu hesapla ve sınırlar içinde tut
        targetXPosition += currentHorizontalInput * sideSpeed * Time.deltaTime;
        targetXPosition = Mathf.Clamp(targetXPosition, -limitX, limitX);
    }

    private void MovePlayer()
    {
        // 1. İleriye sabit hareket
        Vector3 newPosition = transform.position + Vector3.forward * forwardSpeed * Time.deltaTime;

        // 2. X pozisyonunu Lerp ile yumuşatarak hedefe çek (Çekiştirilme hissini siler)
        newPosition.x = Mathf.Lerp(transform.position.x, targetXPosition, Time.deltaTime * positionLerpSpeed);

        transform.position = newPosition;
    }

    private void ApplySmoothRotation()
    {
        // Karakterin anlık horizontal hareketine göre açı hesapla
        // Sağa gidiyorsa pozitif açı, sola gidiyorsa negatif açı
        float targetYRotation = currentHorizontalInput * maxTiltAngle;

        // Anlık rotasyonu hedef açıya doğru yumuşakça döndür (Slerp)
        Quaternion targetRotation = Quaternion.Euler(0f, targetYRotation, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }
}