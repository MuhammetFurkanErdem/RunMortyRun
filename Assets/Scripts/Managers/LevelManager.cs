using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [Header("Oyuncu Yapılandırması")]
    [SerializeField] private GameObject playerPrefab; // Morty Smith Prefabı

    [Header("Başlangıç ve Bitiş Parçaları")]
    [SerializeField] private GameObject startSegmentPrefab;  // Chunk_Start
    [SerializeField] private GameObject finishSegmentPrefab; // Chunk_Finish

    [Header("Rastgele Seçilecek Orta Parçalar")]
    [SerializeField] private List<GameObject> middleSegmentPrefabs = new List<GameObject>(); // Gate, Trap, Enemy parçaları

    [Header("Bölüm Ayarları")]
    [SerializeField] private int middleSegmentCount = 5; // Bölümde kaç tane rastgele engel parçası olacağı
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

        // 3. ORTA PARÇALARI RASTGELE DİZ (Chunk_Gates, Chunk_Traps, Chunk_Enemies)
        if (middleSegmentPrefabs.Count > 0)
        {
            for (int i = 0; i < middleSegmentCount; i++)
            {
                int randomIndex = Random.Range(0, middleSegmentPrefabs.Count);
                GameObject randomPrefab = middleSegmentPrefabs[randomIndex];

                if (randomPrefab != null)
                {
                    spawnPosition = SpawnSegment(randomPrefab, spawnPosition);
                }
            }
        }

        // 4. BİTİŞ PARÇASINI OLUŞTUR
        if (finishSegmentPrefab != null)
        {
            SpawnSegment(finishSegmentPrefab, spawnPosition);
        }
    }

    private void SpawnOrResetPlayer()
    {
        GameObject playerObj = null;

        // Sahnede zaten Morty varsa onu al
        if (PlayerManager.Instance != null)
        {
            playerObj = PlayerManager.Instance.gameObject;
        }
        // Sahnede Morty yoksa Prefab'dan sıfırdan oluştur
        else if (playerPrefab != null)
        {
            playerObj = Instantiate(playerPrefab);
        }

        // Morty'yi başlangıç çizgisine ($Z = 5$) yerleştir
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

        // Segment uzunluğunu al (varsayılan 50)
        LevelSegment segmentScript = spawned.GetComponent<LevelSegment>();
        float length = (segmentScript != null) ? segmentScript.segmentLength : 50f;

        // Bir sonraki parçanın Z pozisyonunu hesapla
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