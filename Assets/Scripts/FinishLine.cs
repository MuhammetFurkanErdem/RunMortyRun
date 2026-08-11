using System.Collections;
using UnityEngine;

public class FinishLine : MonoBehaviour
{
    public static FinishLine Instance { get; private set; }

    [Header("Çarpan Ayarları")]
    private float currentMultiplier = 1.0f;
    private bool isTriggered = false;
    private int initialCrowdCount = 1;

    [Header("Güvenlik Süresi (Merdivene ulaşamazsa)")]
    [SerializeField] private float maxBonusRunDuration = 10f;

    private Coroutine safetyRoutine;
    private bool levelFinished = false;

    public bool IsTriggered => isTriggered;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isTriggered) return;

        if (other.CompareTag("Player"))
        {
            isTriggered = true;

            // 1. Sinematik Kamera Açısına Geç
            if (CameraController.Instance != null)
            {
                CameraController.Instance.SwitchToFinishView();
            }

            // 2. Merdiven tırmanışını başlat
            if (PlayerManager.Instance != null)
            {
                initialCrowdCount = PlayerManager.Instance.GetCrowdCount();
                PlayerManager.Instance.MakeClonesKinematic();
            }

            if (PlayerMovement.Instance != null)
            {
                PlayerMovement.Instance.StartBonusRun();
            }

            safetyRoutine = StartCoroutine(SafetyTimeoutRoutine());
        }
    }

    public void SetCurrentMultiplier(float multiplier)
    {
        if (multiplier > currentMultiplier)
        {
            currentMultiplier = multiplier;
            Debug.Log("Yeni Çarpan Ulaşıldı: x" + currentMultiplier);
        }
    }

    public void StopClimbingAndFinish()
    {
        if (levelFinished) return;

        if (safetyRoutine != null)
        {
            StopCoroutine(safetyRoutine);
            safetyRoutine = null;
        }

        FinishLevel();
    }

    public void CompleteRunAfterStairs()
    {
        if (levelFinished) return;
        if (!isTriggered) return;

        if (safetyRoutine != null)
        {
            StopCoroutine(safetyRoutine);
            safetyRoutine = null;
        }

        FinishLevel();
    }

    private IEnumerator SafetyTimeoutRoutine()
    {
        yield return new WaitForSeconds(maxBonusRunDuration);
        FinishLevel();
    }

    private void FinishLevel()
    {
        if (levelFinished) return;
        levelFinished = true;

        // 1. İleri hareketi kes
        if (PlayerMovement.Instance != null)
        {
            PlayerMovement.Instance.StopBonusRun();
        }

        // 2. Animasyonları durdur
        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.StopAllAnimations();
        }

        // 3. Yıldız Sayısını Ulaşılan Çarpana Göre Dinamik Hesapla
        int starCount = 1;
        if (currentMultiplier >= 5.0f)      // x5 veya x10 çarpanına ulaşıldıysa -> 3 Yıldız
        {
            starCount = 3;
        }
        else if (currentMultiplier >= 2.0f) // x2 veya x3 çarpanına ulaşıldıysa -> 2 Yıldız
        {
            starCount = 2;
        }
        else                                // x1.5 veya altı -> 1 Yıldız
        {
            starCount = 1;
        }

        // 4. Altın Hesabı
        int earnedCoins = (50 + (initialCrowdCount * 10)) * Mathf.RoundToInt(currentMultiplier);

        // 5. Oyunu Bitir ve Dinamik UI Ekranını Aç
        if (GameManager.Instance != null)
        {
            GameManager.Instance.LevelCompleted();
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowLevelCompleteUI(earnedCoins, currentMultiplier, initialCrowdCount, starCount);
        }
    }
}