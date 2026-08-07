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

    private List<GameObject> subPlayers = new List<GameObject>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        UpdateCountUI();
        UpdateMobVisuals();
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
                if (value != 0)
                    newCount /= value;
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
                // İlk döngüde eğer temasa geçen obje bir klon ise öncelikle onu sil
                if (i == 0 && hitObject != null && subPlayers.Contains(hitObject))
                {
                    subPlayers.Remove(hitObject);
                    Destroy(hitObject);
                }
                else
                {
                    // Diğer durumlarda en arkadaki klondan başlayarak sil
                    GameObject lastClone = subPlayers[subPlayers.Count - 1];
                    subPlayers.RemoveAt(subPlayers.Count - 1);
                    if (lastClone != null) Destroy(lastClone);
                }
            }
            else
            {
                // Arkada hiç klon kalmadıysa Ana Karakteri yok et
                if (hitObject != null && hitObject != gameObject)
                {
                    Destroy(hitObject);
                }
                break;
            }
        }

        // Toplam sayıdan hasar miktarını düş
        currentCount -= amount;
        currentCount = Mathf.Max(0, currentCount);

        CleanupSubPlayers();
        UpdateCountUI();
        FormatSubPlayers();

        // Karakterler bittiyse Game Over çağır
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

        // Klon Ekleme
        while (subPlayers.Count < targetSubPlayerCount)
        {
            if (playerPrefab == null)
            {
                Debug.LogError("PlayerManager: playerPrefab Inspector üzerinde atanmamış!");
                break;
            }

            Transform parentTransform = mobHolder != null ? mobHolder : transform;
            GameObject newSubPlayer = Instantiate(playerPrefab, parentTransform);

            // Klonun da Player tag'ine sahip olduğundan emin olalım
            newSubPlayer.tag = "Player";

            Animator subAnim = newSubPlayer.GetComponent<Animator>();
            if (subAnim != null)
            {
                subAnim.SetBool("isRunning", true);
            }

            subPlayers.Add(newSubPlayer);   
        }

        // Klon Eksiltme
        while (subPlayers.Count > targetSubPlayerCount && subPlayers.Count > 0)
        {
            GameObject lastPlayer = subPlayers[subPlayers.Count - 1];
            subPlayers.RemoveAt(subPlayers.Count - 1);
            if (lastPlayer != null) Destroy(lastPlayer);
        }

        FormatSubPlayers();
    }

    private void FormatSubPlayers()
    {
        CleanupSubPlayers();

        for (int i = 0; i < subPlayers.Count; i++)
        {
            if (subPlayers[i] == null) continue;

            float phi = (i + 1) * 137.5f * Mathf.Deg2Rad;
            float r = distanceFactor * Mathf.Sqrt(i + 1);

            float x = r * Mathf.Cos(phi);
            float z = r * Mathf.Sin(phi);

            Vector3 newLocalPos = new Vector3(x, 0f, z);
            subPlayers[i].transform.localPosition = newLocalPos;
        }
    }

    private void CleanupSubPlayers()
    {
        // Yok edilmiş (null) klonları listeden tamamen siler
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
        Debug.Log("GAME OVER: Tüm karakterler yok oldu!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }
    }
}