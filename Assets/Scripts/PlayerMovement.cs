using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float forwardSpeed = 7.5f;
    [SerializeField] private float sideSpeed = 10f;
    [SerializeField] private float limitX = 5.5f; // Başlangıç noktasından sağa/sola kaç birim gidebileceği

    private float startX; // Karakterin oyuna başladığı merkez X pozisyonu

    private void Start()
    {
        startX = transform.position.x;
    }

    private void Update()
    {
        MoveForward();
        HandleInput();
    }

    private void MoveForward()
    {
        transform.Translate(Vector3.forward * forwardSpeed * Time.deltaTime, Space.World);
    }

    private void HandleInput()
    {
        float horizontalInput = 0f;

        // Klavye Girdileri
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

        // X pozisyonunu merkez (startX) noktasına göre sınırla
        newPos.x = Mathf.Clamp(newPos.x, startX - limitX, startX + limitX);

        transform.position = newPos;
    }
}