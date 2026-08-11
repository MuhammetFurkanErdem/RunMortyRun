using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [System.Serializable]
    public class SegmentConfig
    {
        public string segmentName; // Inspector'da kolay tanım için (Örn: Kapılar, Düşmanlar)
        public GameObject prefab;

        [Range(0f, 1f)]
        [Tooltip("Bu parçanın bölümde bulunma minimum oranı (Örn: 0.1 = %10)")]
        public float minRatio = 0.1f;

        [Range(0f, 1f)]
        [Tooltip("Bu parçanın bölümde bulunma maksimum oranı (Örn: 0.3 = %30)")]
        public float maxRatio = 0.3f;
    }

    [Header("Oyuncu Yapılandırması")]
    [SerializeField] private GameObject playerPrefab; // Morty Smith Prefabı

    [Header("Başlangıç ve Bitiş Parçaları")]
    [SerializeField] private GameObject startSegmentPrefab;  // Chunk_Start
    [SerializeField] private GameObject finishSegmentPrefab; // Chunk_Finish

    [Header("Orta Parçalar ve Oransal Sınırları")]
    [SerializeField] private List<SegmentConfig> middleSegmentConfigs = new List<SegmentConfig>();

    [Header("Bölüm Ayarları")]
    [SerializeField] private int middleSegmentCount = 10; // Bölümde kaç tane rastgele engel parçası olacağı
    [SerializeField] private Transform levelHolder;

    private List<GameObject> spawnedSegments = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        GenerateRandomLevel();
    }

    public void GenerateRandomLevel()
    {
        ClearCurrentLevel();

        Vector3 spawnPosition = Vector3.zero;

        // 1. BAŞLANGIÇ PARÇASINI OLUŞTUR (Chunk_Start)
        if (startSegmentPrefab != null)
        {
            spawnPosition = SpawnSegment(startSegmentPrefab, spawnPosition);
        }

        // 2. PLAYER (MORTY) OLUŞTUR/IŞINLA
        SpawnOrResetPlayer();

        // 3. ORTA PARÇALARI LİMİTLERE VE ORANLARA GÖRE DİZ
        List<GameObject> selectedSegments = GenerateBalancedSegmentList();
        foreach (GameObject segmentPrefab in selectedSegments)
        {
            if (segmentPrefab != null)
            {
                spawnPosition = SpawnSegment(segmentPrefab, spawnPosition);
            }
        }

        // 4. BİTİŞ PARÇASINI OLUŞTUR (Chunk_Finish)
        if (finishSegmentPrefab != null)
        {
            SpawnSegment(finishSegmentPrefab, spawnPosition);
        }
    }

    private List<GameObject> GenerateBalancedSegmentList()
    {
        List<GameObject> resultList = new List<GameObject>();

        if (middleSegmentConfigs == null || middleSegmentConfigs.Count == 0)
            return resultList;

        Dictionary<SegmentConfig, int> currentCounts = new Dictionary<SegmentConfig, int>();
        Dictionary<SegmentConfig, int> minCounts = new Dictionary<SegmentConfig, int>();
        Dictionary<SegmentConfig, int> maxCounts = new Dictionary<SegmentConfig, int>();

        // Her parça için Min ve Max sayı limitlerini hesapla
        foreach (var config in middleSegmentConfigs)
        {
            currentCounts[config] = 0;
            minCounts[config] = Mathf.FloorToInt(middleSegmentCount * config.minRatio);
            maxCounts[config] = Mathf.CeilToInt(middleSegmentCount * config.maxRatio);
        }

        // A ADIMI: Önce Her Parçanın Garanti Edilen Minimum Sayısını Ekle
        foreach (var config in middleSegmentConfigs)
        {
            int minNeeded = minCounts[config];
            for (int i = 0; i < minNeeded && resultList.Count < middleSegmentCount; i++)
            {
                if (config.prefab != null)
                {
                    resultList.Add(config.prefab);
                    currentCounts[config]++;
                }
            }
        }

        // B ADIMI: Kalan Boşlukları Maksimum Sınırını Aşmayan Parçalar Arasından Doldur
        while (resultList.Count < middleSegmentCount)
        {
            List<SegmentConfig> availableConfigs = new List<SegmentConfig>();

            foreach (var config in middleSegmentConfigs)
            {
                if (config.prefab != null && currentCounts[config] < maxCounts[config])
                {
                    availableConfigs.Add(config);
                }
            }

            if (availableConfigs.Count == 0)
            {
                foreach (var config in middleSegmentConfigs)
                {
                    if (config.prefab != null) availableConfigs.Add(config);
                }
            }

            SegmentConfig chosenConfig = availableConfigs[Random.Range(0, availableConfigs.Count)];
            resultList.Add(chosenConfig.prefab);
            currentCounts[chosenConfig]++;
        }

        // C ADIMI: AKILLI KARIŞTIRMA (Aynı kategorideki parçaların art arda gelmesini engelle)
        SmartShuffleList(resultList);

        return resultList;
    }

    // --- AKILLI KARIŞTIRMA VE ARDIŞIK TEKRAR ENGELLEME ---
    private void SmartShuffleList(List<GameObject> list)
    {
        // 1. Önce Klasik Fisher-Yates Karıştırma
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            GameObject temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }

        // 2. Ardışık Aynı Kategorileri Ayrıştır (Takas Et)
        for (int i = 1; i < list.Count; i++)
        {
            if (IsSameCategory(list[i], list[i - 1]))
            {
                // İleride farklı bir kategoriye ait parça bul ve takas et
                bool swapped = false;
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (!IsSameCategory(list[j], list[i - 1]))
                    {
                        GameObject temp = list[i];
                        list[i] = list[j];
                        list[j] = temp;
                        swapped = true;
                        break;
                    }
                }

                // İleride takas edilecek eleman bulunamadıysa geriye doğru dene
                if (!swapped)
                {
                    for (int j = 0; j < i - 1; j++)
                    {
                        if (!IsSameCategory(list[i], list[j]) && (j == 0 || !IsSameCategory(list[i], list[j - 1])))
                        {
                            GameObject temp = list[i];
                            list[i] = list[j];
                            list[j] = temp;
                            break;
                        }
                    }
                }
            }
        }
    }

    private bool IsSameCategory(GameObject a, GameObject b)
    {
        if (a == null || b == null) return false;

        string catA = GetCategoryName(a.name);
        string catB = GetCategoryName(b.name);

        return catA == catB;
    }

    private string GetCategoryName(string objName)
    {
        string lower = objName.ToLower();

        if (lower.Contains("gate")) return "Gate";
        if (lower.Contains("trap") || lower.Contains("obstacle")) return "Trap";
        if (lower.Contains("enemy") || lower.Contains("enemies")) return "Enemy";

        return objName;
    }

    private void SpawnOrResetPlayer()
    {
        GameObject playerObj = null;

        if (PlayerManager.Instance != null)
        {
            playerObj = PlayerManager.Instance.gameObject;
        }
        else if (playerPrefab != null)
        {
            playerObj = Instantiate(playerPrefab);
        }

        if (playerObj != null)
        {
            PlayerMovement playerMovement = playerObj.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                Vector3 playerStartPos = new Vector3(0f, 0.1f, 5f);
                playerMovement.ResetPosition(playerStartPos);
            }
        }
    }

    private Vector3 SpawnSegment(GameObject prefab, Vector3 position)
    {
        Transform parentTransform = levelHolder != null ? levelHolder : transform;
        GameObject spawned = Instantiate(prefab, position, Quaternion.identity, parentTransform);
        spawnedSegments.Add(spawned);

        LevelSegment segmentScript = spawned.GetComponent<LevelSegment>();
        float length = (segmentScript != null) ? segmentScript.segmentLength : 50f;

        position.z += length;
        return position;
    }

    public void RestartLevel()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    private void ClearCurrentLevel()
    {
        foreach (GameObject segment in spawnedSegments)
        {
            Destroy(segment);
        }
        spawnedSegments.Clear();
    }
}