using UnityEngine;
using static GameManager;

public class Enemy : MonoBehaviour
{
    [Header("Düşman Ayarları")]
    [SerializeField] private float detectionDistance = 18f; // Kovalamayı başlatma mesafesi
    [SerializeField] private float loseDistance = 22f;      // Oyuncu bu mesafeyi aşarsa kovalamayı bırakır
    [SerializeField] private float moveSpeed = 6f;          // Düşmanın koşma hızı

    private Animator animator;
    private bool isRunning = false;
    private Transform playerTarget;

    private void Start()
    {
        animator = GetComponent<Animator>();

        // Sahnedeki ana oyuncuyu hedef al
        if (PlayerManager.Instance != null)
        {
            playerTarget = PlayerManager.Instance.transform;
        }
    }

    private void Update()
    {
        // Oyun başlamadıysa veya bittiyse hareket etme
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameState.Playing)
            return;

        if (playerTarget == null)
        {
            if (PlayerManager.Instance != null)
                playerTarget = PlayerManager.Instance.transform;
            return;
        }

        // Oyuncu ile arasındaki mesafeyi kontrol et
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        // Algılama mesafesine girdiyse koşmayı başlat
        if (!isRunning && distanceToPlayer <= detectionDistance)
        {
            StartRunning();
        }
        // 2. Kovalamayı Bırakma Kontrolü (Oyuncu atlatıp arayı açtıysa)
        else if (isRunning && distanceToPlayer > loseDistance)
        {
            StopRunning();
        }

        // Koşma durumundaysa oyuncunun olduğu yöne doğru ilerle
        if (isRunning)
        {
            MoveTowardsPlayer();
        }
    }

    private void StartRunning()
    {
        isRunning = true;

        if (animator != null)
        {
            animator.SetBool("isRunning", true);
        }
    }

    private void StopRunning()
    {
        isRunning = false;

        if (animator != null)
        {
            animator.SetBool("isRunning", false);
        }
    }

    private void MoveTowardsPlayer()
    {
        // Oyuncunun Z ve X pozisyonuna doğru hareket et
        Vector3 targetPosition = new Vector3(playerTarget.position.x, transform.position.y, playerTarget.position.z);
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);

        // Yüzünü oyuncunun olduğu yöne döndür
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Temas eden obje oyuncu sürüsündense
        if (other.CompareTag("Player"))
        {
            if (PlayerManager.Instance != null)
            {
                // Sürüden 1 kişi eksilt
                PlayerManager.Instance.RemovePlayer(other.gameObject, 1);
            }

            // Düşman kendini yok eder (1-e-1 çarpışma mantığı)
            Destroy(gameObject);
        }
    }
}