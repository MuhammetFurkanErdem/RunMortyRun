using UnityEngine;

public class RewardManager : MonoBehaviour
{
    public static RewardManager Instance { get; private set; }

    [Header("Altın Ödül Ayarları")]
    [SerializeField] private int baseReward = 50;       // Tamamlanan her seviye için taban altın
    [SerializeField] private int coinPerClone = 10;     // Yaşayan her Morty kopyası başına ekstra altın

    [Header("Yıldız Eşikleri (Kopya Sayısı)")]
    [SerializeField] private int twoStarThreshold = 6;  // 2 Yıldız için min kopya
    [SerializeField] private int threeStarThreshold = 16; // 3 Yıldız için min kopya

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public struct RewardResult
    {
        public int earnedCoins;
        public int starCount;
        public int finalCrowdSize;
    }

    public RewardResult CalculateReward(int finalCrowdSize, float multiplier = 1f)
    {
        RewardResult result = new RewardResult();
        result.finalCrowdSize = finalCrowdSize;

        // 1. Altın Hesaplama
        int rawCoins = baseReward + (finalCrowdSize * coinPerClone);
        result.earnedCoins = Mathf.RoundToInt(rawCoins * multiplier);

        // 2. Yıldız Hesaplama
        if (finalCrowdSize >= threeStarThreshold)
        {
            result.starCount = 3;
        }
        else if (finalCrowdSize >= twoStarThreshold)
        {
            result.starCount = 2;
        }
        else
        {
            result.starCount = 1;
        }

        // 3. Toplam Altını Kaydetme (PlayerPrefs)
        int currentTotalCoins = PlayerPrefs.GetInt("TotalCoins", 0);
        PlayerPrefs.SetInt("TotalCoins", currentTotalCoins + result.earnedCoins);
        PlayerPrefs.Save();

        return result;
    }
}