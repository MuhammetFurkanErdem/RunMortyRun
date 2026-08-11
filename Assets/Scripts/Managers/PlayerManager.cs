using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; }

    [Header("Oyuncu Sayısı")]
    [SerializeField] private int currentCount = 1;
    [SerializeField] private TextMeshPro countText;

    [Header("Kalabalık (Mob) Ayarları")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform mobHolder;
    [SerializeField] private float distanceFactor = 0.65f;
    [SerializeField] private float minRadius = 0.85f;

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

    // Seviye bittiğinde tüm kopyaların koşu animasyonunu durdurur
    public void StopAllAnimations()
    {
        CleanupSubPlayers();

        // Ana Karakter
        Animator mainAnim = GetComponent<Animator>();
        if (mainAnim != null)
        {
            mainAnim.SetBool("isRunning", false);
        }

        // Tüm Klonlar
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

            float phi = (i + 1) * 137.5f * Mathf.Deg2Rad;
            float r = minRadius + (distanceFactor * Mathf.Sqrt(i));

            float localX = r * Mathf.Cos(phi);
            float localZ = r * Mathf.Sin(phi);

            Vector3 cloneWorldPos = parentTransform.TransformPoint(new Vector3(localX, 0f, localZ));

            Vector3 rayOrigin = cloneWorldPos + Vector3.forward * 0.8f + Vector3.up * 5f;
            RaycastHit[] hits = Physics.RaycastAll(rayOrigin, Vector3.down, 15f, ~0, QueryTriggerInteraction.Collide);

            float groundY = transform.position.y;
            float maxHitY = -999f;
            bool foundGround = false;

            foreach (var hit in hits)
            {
                if (hit.collider.CompareTag("FinishLine") || hit.collider.CompareTag("Player") || hit.collider.GetComponent<FinishLine>() != null)
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
            if (subPlayers.Count > 0)
            {
                if (i == 0 && hitObject != null && subPlayers.Contains(hitObject))
                {
                    subPlayers.Remove(hitObject);
                    Destroy(hitObject);
                }
                else
                {
                    GameObject lastClone = subPlayers[subPlayers.Count - 1];
                    subPlayers.RemoveAt(subPlayers.Count - 1);
                    if (lastClone != null) Destroy(lastClone);
                }
            }
            else
            {
                if (hitObject != null && hitObject != gameObject)
                {
                    Destroy(hitObject);
                }
                break;
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
                // Eğer oyun devam ediyorsa koşma animasyonu açılır
                bool isPlaying = GameManager.Instance == null || GameManager.Instance.CurrentState == GameManager.GameState.Playing;
                subAnim.SetBool("isRunning", isPlaying);
            }

            subPlayers.Add(newSubPlayer);
        }

        while (subPlayers.Count > targetSubPlayerCount && subPlayers.Count > 0)
        {
            GameObject lastPlayer = subPlayers[subPlayers.Count - 1];
            subPlayers.RemoveAt(subPlayers.Count - 1);
            if (lastPlayer != null) Destroy(lastPlayer);
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