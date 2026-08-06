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
    [SerializeField] private float distanceFactor = 0.25f;

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

    // amount parametresi eklendi (varsayılan 1)
    public void RemovePlayer(GameObject hitObject, int amount = 1)
    {
        for (int i = 0; i < amount; i++)
        {
            if (subPlayers.Count > 0)
            {
                // İlk döngüde eğer temasa geçen obje bir klon ise öncelikle onu sil
                if (i == 0 && subPlayers.Contains(hitObject))
                {
                    subPlayers.Remove(hitObject);
                    Destroy(hitObject);
                }
                else
                {
                    // Diğer durumlarda en arkadaki klondan başlayarak sil
                    GameObject lastClone = subPlayers[subPlayers.Count - 1];
                    subPlayers.RemoveAt(subPlayers.Count - 1);
                    Destroy(lastClone);
                }
            }
            else
            {
                // Arkada hiç klon kalmadıysa Ana Karakteri yok et
                if (hitObject != null)
                {
                    Destroy(hitObject);
                }
                break;
            }
        }

        // Toplam sayıdan hasar miktarını düş
        currentCount -= amount;
        currentCount = Mathf.Max(0, currentCount);
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
        int targetSubPlayerCount = currentCount - 1;

        while (subPlayers.Count < targetSubPlayerCount)
        {
            Transform parentTransform = mobHolder != null ? mobHolder : transform;
            GameObject newSubPlayer = Instantiate(playerPrefab, parentTransform);
            subPlayers.Add(newSubPlayer);
        }

        while (subPlayers.Count > targetSubPlayerCount && subPlayers.Count > 0)
        {
            GameObject lastPlayer = subPlayers[subPlayers.Count - 1];
            subPlayers.RemoveAt(subPlayers.Count - 1);
            Destroy(lastPlayer);
        }

        FormatSubPlayers();
    }

    private void FormatSubPlayers()
    {
        for (int i = 0; i < subPlayers.Count; i++)
        {
            float phi = (i + 1) * 137.5f * Mathf.Deg2Rad;
            float r = distanceFactor * Mathf.Sqrt(i + 1);

            float x = r * Mathf.Cos(phi);
            float z = r * Mathf.Sin(phi);

            Vector3 newLocalPos = new Vector3(x, 0f, z);
            subPlayers[i].transform.localPosition = newLocalPos;
        }
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
            GameManager.Instance.SetGameOver();
        }
    }
}