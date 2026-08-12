using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [Header("Ölüm Efekti")]
    [SerializeField] private GameObject deathParticlePrefab;

    [Header("Oyuncu Sayısı")]
    [SerializeField] private int currentCount = 1;
    [SerializeField] private TextMeshPro countText;

    [Header("Harita Sınır Ayarları")]
    [SerializeField] private float maxTrackX = 3.8f; // Yolun genişlik sınırı (Sol: -3.8, Sağ: +3.8)

    [Header("Kalabalık (Mob) Ayarları")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform mobHolder;
    [SerializeField] private float distanceFactor = 0.45f; // Daha sıkı dizilim
    [SerializeField] private float minRadius = 0.5f;       // Daha toplu merkez

    private List<GameObject> subPlayers = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        UpdateCountUI();
        UpdateMobVisuals();
    }

    private void Update()
    {
        UpdateSubPlayerHeights();
    }

    public int GetCrowdCount()
    {
        return Mathf.Max(1, currentCount);
    }

    public void MakeClonesKinematic()
    {
        CleanupSubPlayers();
        foreach (var clone in subPlayers)
        {
            if (clone == null) continue;
            Rigidbody rb = clone.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;

            Collider col = clone.GetComponent<Collider>();
            if (col != null) col.isTrigger = true;
        }
    }

    public void StopAllAnimations()
    {
        CleanupSubPlayers();

        Animator mainAnim = GetComponent<Animator>();
        if (mainAnim != null)
        {
            mainAnim.SetBool("isRunning", false);
        }

        foreach (var clone in subPlayers)
        {
            if (clone == null) continue;
            Animator subAnim = clone.GetComponent<Animator>();
            if (subAnim != null)
            {
                subAnim.SetBool("isRunning", false);
            }
        }
    }

    private void UpdateSubPlayerHeights()
    {
        CleanupSubPlayers();

        Transform parentTransform = mobHolder != null ? mobHolder : transform;

        for (int i = 0; i < subPlayers.Count; i++)
        {
            if (subPlayers[i] == null) continue;

            // 1. Standart Spiral Konum Hesabı
            float phi = (i + 1) * 137.5f * Mathf.Deg2Rad;
            float r = minRadius + (distanceFactor * Mathf.Sqrt(i));

            float localX = r * Mathf.Cos(phi);
            float localZ = r * Mathf.Sin(phi);

            // 2. Dünya X Pozisyonunu Ve Sınır Taşmasını Hesapla
            float worldX = parentTransform.position.x + localX;

            // --- HARİTADAN TAŞMAYI ENGELLEME VE ARKAYA YÖNLENDİRME ---
            if (worldX > maxTrackX)
            {
                float overflow = worldX - maxTrackX;
                localX -= overflow;           // Sağ duvara sabitle
                localZ -= overflow * 1.2f;    // Dışarı taşan kısmı arkaya doğru uzat!
            }
            else if (worldX < -maxTrackX)
            {
                float overflow = -maxTrackX - worldX;
                localX += overflow;           // Sol duvara sabitle
                localZ -= overflow * 1.2f;    // Dışarı taşan kısmı arkaya doğru uzat!
            }

            // 3. Bastığı Zemini Raycast İle Bul
            Vector3 cloneWorldPos = parentTransform.TransformPoint(new Vector3(localX, 0f, localZ));
            Vector3 rayOrigin = cloneWorldPos + Vector3.forward * 0.8f + Vector3.up * 5f;
            RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, 15f, ~0, QueryTriggerInteraction.Collide);

            float groundY = transform.position.y;
            float maxHitY = -999f;
            bool foundGround = false;

            foreach (var hit in hits)
            {
                // Kopyalar için de engelleri ve düşmanları yok say
                if (hit.collider.CompareTag("FinishLine") ||
                    hit.collider.CompareTag("Player") ||
                    hit.collider.CompareTag("Obstacle") ||
                    hit.collider.CompareTag("Enemy") ||
                    hit.collider.CompareTag("Gate") ||
                    hit.collider.GetComponent<FinishLine>() != null)
                    continue;

                if (hit.point.y > maxHitY)
                {
                    maxHitY = hit.point.y;
                    foundGround = true;
                }
            }

            if (foundGround)
            {
                groundY = maxHitY;
            }

            float localY = groundY - transform.position.y;

            Vector3 targetLocalPos = new Vector3(localX, localY, localZ);
            float climbSpeed = (localY > subPlayers[i].transform.localPosition.y) ? 40f : 25f;
            subPlayers[i].transform.localPosition = Vector3.Lerp(subPlayers[i].transform.localPosition, targetLocalPos, Time.deltaTime * climbSpeed);
        }
    }

    public void ApplyGateOperation(GateType type, int value)
    {
        int newCount = currentCount;

        switch (type)
        {
            case GateType.Add:
                newCount += value;
                break;
            case GateType.Subtract:
                newCount -= value;
                break;
            case GateType.Multiply:
                newCount *= value;
                break;
            case GateType.Divide:
                if (value != 0) newCount /= value;
                break;
        }

        currentCount = Mathf.Max(0, newCount);

        UpdateCountUI();
        UpdateMobVisuals();

        if (currentCount <= 0)
        {
            GameOver();
        }
    }

    public void RemovePlayer(GameObject hitObject, int amount = 1)
    {
        CleanupSubPlayers();

        for (int i = 0; i < amount; i++)
        {
            GameObject victim = null;

            if (subPlayers.Count > 0)
            {
                if (i == 0 && hitObject != null && subPlayers.Contains(hitObject))
                {
                    victim = hitObject;
                    subPlayers.Remove(hitObject);
                }
                else
                {
                    victim = subPlayers[subPlayers.Count - 1];
                    subPlayers.RemoveAt(subPlayers.Count - 1);
                }
            }
            else
            {
                if (hitObject != null && hitObject != gameObject)
                {
                    victim = hitObject;
                }
            }

            // Ölüm Noktasında VFX Patlat ve Objeyi SİL
            if (victim != null)
            {
                SpawnDeathVFX(victim.transform.position);
                Destroy(victim);
            }
        }

        currentCount -= amount;
        currentCount = Mathf.Max(0, currentCount);

        CleanupSubPlayers();
        UpdateCountUI();

        if (currentCount <= 0)
        {
            GameOver();
        }
    }

    private void SpawnDeathVFX(Vector3 position)
    {
        if (deathParticlePrefab != null)
        {
            Vector3 spawnPos = position + Vector3.up * 0.8f;
            GameObject fx = Instantiate(deathParticlePrefab, spawnPos, Quaternion.identity);

            fx.transform.localScale = Vector3.one;

            ParticleSystem[] particles = fx.GetComponentsInChildren<ParticleSystem>();
            foreach (ParticleSystem ps in particles)
            {
                ps.Clear();
                ps.Play();
            }

            Destroy(fx, 1.5f);
        }
    }

    private void UpdateMobVisuals()
    {
        CleanupSubPlayers();

        int targetSubPlayerCount = currentCount - 1;
        if (targetSubPlayerCount < 0) targetSubPlayerCount = 0;

        while (subPlayers.Count < targetSubPlayerCount)
        {
            if (playerPrefab == null) break;

            Transform parentTransform = mobHolder != null ? mobHolder : transform;
            GameObject newSubPlayer = Instantiate(playerPrefab, parentTransform);

            newSubPlayer.tag = "Player";

            Animator subAnim = newSubPlayer.GetComponent<Animator>();
            if (subAnim != null)
            {
                bool isPlaying = GameManager.Instance == null || GameManager.Instance.CurrentState == GameManager.GameState.Playing;
                subAnim.SetBool("isRunning", isPlaying);
            }

            subPlayers.Add(newSubPlayer);
        }

        while (subPlayers.Count > targetSubPlayerCount && subPlayers.Count > 0)
        {
            GameObject lastPlayer = subPlayers[subPlayers.Count - 1];

            if (lastPlayer != null)
            {
                SpawnDeathVFX(lastPlayer.transform.position);
                Destroy(lastPlayer);
            }

            subPlayers.RemoveAt(subPlayers.Count - 1);
        }
    }

    private void CleanupSubPlayers()
    {
        subPlayers.RemoveAll(player => player == null);
    }

    private void UpdateCountUI()
    {
        if (countText != null)
        {
            countText.text = currentCount.ToString();
        }
    }

    private void GameOver()
    {
        if (FinishLine.Instance != null && FinishLine.Instance.IsTriggered) return;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}