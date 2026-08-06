using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float forwardSpeed = 7.5f;
    [SerializeField] private float sideSpeed = 10f;
    [SerializeField] private float limitX = 5.5f;

    private float startX;
    private Animator animator;

    private void Start()
    {
        startX = transform.position.x;

        // Karakterin Animator bileşenini al ve başlangıçta animasyonu dondur
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.speed = 0f;
        }
    }

    private void Update()
    {
        // 1. Oyun henüz başlamadıysa tuş girdisi bekle
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.NotStarted)
        {
            CheckForStartInput();
            return;
        }

        // 2. Oyun başladıysa ileri git ve yönlendir
        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameState.Playing)
        {
            MoveForward();
            HandleInput();
        }
    }

    private void CheckForStartInput()
    {
        bool hasInput = false;

        // Klavye Kontrolleri
        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.wasPressedThisFrame ||
                Keyboard.current.dKey.wasPressedThisFrame ||
                Keyboard.current.leftArrowKey.wasPressedThisFrame ||
                Keyboard.current.rightArrowKey.wasPressedThisFrame ||
                Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                hasInput = true;
            }
        }

        // Fare / Dokunmatik Kontrol
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            hasInput = true;
        }

        // Girdi alındıysa oyunu başlat ve animasyonu çalıştır
        if (hasInput)
        {
            GameManager.Instance.StartGame();

            if (animator != null)
            {
                animator.speed = 1f; // Animasyonu normale döndür
            }
        }
    }

    private void MoveForward()
    {
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.World);
    }

    private void HandleInput()
    {
        float horizontalInput = 0f;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            {
                horizontalInput = -1f;
            }
            else if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            {
                horizontalInput = 1f;
            }
        }

        Vector3 newPos = transform.position;
        newPos.x += horizontalInput * sideSpeed * Time.deltaTime;
        newPos.x = Mathf.Clamp(newPos.x, startX - limitX, startX + limitX);

        transform.position = newPos;
    }
}