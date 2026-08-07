using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("Koşu Hızları")]
    [SerializeField] private float forwardSpeed = 7.5f;
    [SerializeField] private float sideSpeed = 10f;
    [SerializeField] private float limitX = 4.5f;

    [Header("Kontrolcü Referansı")]
    [SerializeField] private Joystick joystick; // Canvas üzerindeki Joystick

    [Header("Yumuşatma ve Dönüş Ayarları")]
    [SerializeField] private float positionLerpSpeed = 15f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float maxTiltAngle = 20f;

    private Animator animator;
    private float targetXPosition;
    private float currentHorizontalInput;

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

        if (GameManager.Instance.CurrentState == GameManager.GameState.GameOver)
        {
            if (animator != null) animator.speed = 0f;
            return;
        }

        if (GameManager.Instance.CurrentState == GameManager.GameState.Start)
        {
            CheckForStartInput();
            return;
        }

        if (GameManager.Instance.CurrentState == GameManager.GameState.Playing)
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
        currentHorizontalInput = 0f;

        // 1. Önce Joystick Kontrolü (Mobil ve Fare)
        if (joystick != null && Mathf.Abs(joystick.Horizontal) > 0.05f)
        {
            currentHorizontalInput = joystick.Horizontal;
        }
        // 2. Eğer Joystick kullanılmıyorsa Klavyeyi Oku (A/D - Yön Tuşları)
        else
        {
            currentHorizontalInput = Input.GetAxis("Horizontal");
        }

        // Hedef X pozisyonunu güncelle ve limitX sınırına tam oturt
        targetXPosition += currentHorizontalInput * sideSpeed * Time.deltaTime;
        targetXPosition = Mathf.Clamp(targetXPosition, -limitX, limitX);
    }

    private void MovePlayer()
    {
        Vector3 newPosition = transform.position + Vector3.forward * forwardSpeed * Time.deltaTime;
        newPosition.x = Mathf.Lerp(transform.position.x, targetXPosition, Time.deltaTime * positionLerpSpeed);
        transform.position = newPosition;
    }

    private void ApplySmoothRotation()
    {
        float targetYRotation = currentHorizontalInput * maxTiltAngle;
        Quaternion targetRotation = Quaternion.Euler(0f, targetYRotation, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    public void ResetPosition(Vector3 startPosition)
    {
        transform.position = startPosition;
        targetXPosition = startPosition.x;
        transform.rotation = Quaternion.identity;

        if (animator != null)
        {
            animator.SetBool("isRunning", false);
        }
    }
}