using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private GameObject JoystickBackground;

    [Header("Paneller")]
    [SerializeField] private GameObject startPanel;        // Tap to Play ekranı
    [SerializeField] private GameObject gameOverPanel;     // Try Again ekranı
    [SerializeField] private GameObject levelCompletePanel; // Next Level / Kazandınız ekranı

    [Header("Level Complete Dinamik UI Elemanları")]
    [SerializeField] private TextMeshProUGUI coinsText;
    [SerializeField] private TextMeshProUGUI multiplierText;
    [SerializeField] private TextMeshProUGUI crowdText;
    [SerializeField] private GameObject[] starObjects; // 1, 2 ve 3 yıldız objeleri

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ShowStartUI();
    }

    public void ShowStartUI()
    {
        if (startPanel != null) startPanel.SetActive(true);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
    }

    public void ShowInGameUI()
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
    }

    public void ShowGameOverUI()
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(true);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
        if (JoystickBackground != null) JoystickBackground.SetActive(false);
    
    }

    // Dinamik Level Complete Arayüzü Güncellemesi
    public void ShowLevelCompleteUI(int earnedCoins, float multiplier, int crowdSize, int starCount)
    {
        if (startPanel != null) startPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (levelCompletePanel != null) levelCompletePanel.SetActive(true);

        // Küçük mor kutulara sığacak sade format:
        if (coinsText != null) coinsText.text = "+" + earnedCoins.ToString();
        if (multiplierText != null) multiplierText.text = "x" + multiplier.ToString("F1");
        if (crowdText != null) crowdText.text = crowdSize.ToString();

        // Yıldızları aktifleşme durumu
        if (starObjects != null)
        {
            for (int i = 0; i < starObjects.Length; i++)
            {
                if (starObjects[i] != null)
                {
                    starObjects[i].SetActive(i < starCount);
                }
            }
        }
    }

    public void ShowLevelCompleteUI()
    {
        ShowLevelCompleteUI(100, 1.0f, 1, 1);
    }

    public void OnRestartButtonClicked()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
    }
}