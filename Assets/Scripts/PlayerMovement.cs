using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance { get; private set; }

    [Header("Koşu Hızları")]
    [SerializeField] private float forwardSpeed = 7.5f;
    [SerializeField] private float sideSpeed = 10f;
    [SerializeField] private float limitX = 4.5f;

    [Header("Kontrolcü Referansı")]
    [SerializeField] private Joystick joystick;

    [Header("Yumuşatma ve Dönüş Ayarları")]
    [SerializeField] private float positionLerpSpeed = 15f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float maxTiltAngle = 20f;

    [Header("Zemin ve Merdiven Tırmanma (Raycast)")]
    [SerializeField] private float raycastOffsetY = 5f;
    [SerializeField] private float raycastDistance = 15f;
    [SerializeField] private float yLerpSpeed = 35f;

    private Animator animator;
    private float targetXPosition;
    private float currentHorizontalInput;
    private bool canMoveHorizontally = true;
    private bool isBonusRunning = false; // Merdiven tırmanışı aktif mi?

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        animator = GetComponent<Animator>();
        targetXPosition = transform.position.x;

        if (animator != null)
        {
            animator.SetBool("isRunning", false);
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;

        // 1. Game Over Durumu
        if (GameManager.Instance.CurrentState == GameManager.GameState.GameOver)
        {
            if (animator != null) animator.speed = 0f;
            return;
        }

        // 2. Level Complete Durumu (Sadece merdiven tırmanışı BİTTİĞİNDE buraya girecek)
        if (GameManager.Instance.CurrentState == GameManager.GameState.LevelComplete)
        {
            if (animator != null)
            {
                animator.SetBool("isRunning", false);
            }
            return; // Hareketi durdur
        }

        // 3. Oyun Başlamadıysa
        if (GameManager.Instance.CurrentState == GameManager.GameState.Start)
        {
            CheckForStartInput();
            return;
        }

        // 4. Oyun Oynanıyorsa VEYA Merdiven Tırmanılıyorsa İLERİ GİTMEYE DEVAM ET
        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing || isBonusRunning)
        {
            if (animator != null && !animator.GetBool("isRunning"))
            {
                animator.speed = 1f;
                animator.SetBool("isRunning", true);
            }

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
        }
    }

    private void HandleInput()
    {
        if (!canMoveHorizontally)
        {
            currentHorizontalInput = 0f;
            return;
        }

        currentHorizontalInput = 0f;

        if (joystick != null && Mathf.Abs(joystick.Horizontal) > 0.05f)
        {
            currentHorizontalInput = joystick.Horizontal;
        }
        else
        {
            currentHorizontalInput = Input.GetAxis("Horizontal");
        }

        targetXPosition += currentHorizontalInput * sideSpeed * Time.deltaTime;
        targetXPosition = Mathf.Clamp(targetXPosition, -limitX, limitX);
    }

    private void MovePlayer()
    {
        Vector3 newPosition = transform.position + Vector3.forward * forwardSpeed * Time.deltaTime;
        newPosition.x = Mathf.Lerp(transform.position.x, targetXPosition, Time.deltaTime * positionLerpSpeed);

        // --- MERDİVEN YÜKSEKLİK HESABI ---
        Vector3 rayOrigin = transform.position + Vector3.forward * 0.8f + Vector3.up * raycastOffsetY;
        RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, raycastDistance, ~0, QueryTriggerInteraction.Collide);

        float maxHitY = -999f;
        bool foundGround = false;

        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("FinishLine") || hit.collider.CompareTag("Player") || hit.collider.GetComponent<FinishLine>() != null)
                continue;

            if (hit.point.y > maxHitY)
            {
                maxHitY = hit.point.y;
                foundGround = true;
            }
        }

        if (foundGround)
        {
            float climbSpeed = (maxHitY > transform.position.y) ? 40f : yLerpSpeed;
            newPosition.y = Mathf.Lerp(transform.position.y, maxHitY, Time.deltaTime * climbSpeed);
        }

        transform.position = newPosition;
    }

    private void ApplySmoothRotation()
    {
        float targetYRotation = currentHorizontalInput * maxTiltAngle;
        Quaternion targetRotation = Quaternion.Euler(0f, targetYRotation, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    // Bitiş çizgisinde merdiven tırmanışını başlatır
    public void StartBonusRun()
    {
        canMoveHorizontally = false;
        currentHorizontalInput = 0f;
        isBonusRunning = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;

        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
    }

    // Merdiven tırmanışı bittiğinde hareketi keser
    public void StopBonusRun()
    {
        isBonusRunning = false;
    }

    public void DisableSideMovement()
    {
        StartBonusRun();
    }

    public void ResetPosition(Vector3 startPosition)
    {
        transform.position = startPosition;
        targetXPosition = startPosition.x;
        transform.rotation = Quaternion.identity;
        canMoveHorizontally = true;
        isBonusRunning = false;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;

        Collider col = GetComponent<Collider>();
        if (col != null) col.isTrigger = false;

        if (animator != null)
        {
            animator.SetBool("isRunning", false);
        }
    }
}