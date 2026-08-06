using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance { get; private set; } // Tip 'PlayerManager' olarak düzeltildi

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
        // Singleton bağlantısı kuruldu
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

    public void RemovePlayer(GameObject hitObject)
    {
        // 1. Çarpan obje bir klon ise listeden çıkar ve yok et
        if (subPlayers.Contains(hitObject))
        {
            subPlayers.Remove(hitObject);
            Destroy(hitObject);
        }
        // 2. Çarpan obje ANA Karakter ise ve arkasında klon varsa en arkadaki klonu sil
        else if (subPlayers.Count > 0)
        {
            GameObject lastClone = subPlayers[subPlayers.Count - 1];
            subPlayers.RemoveAt(subPlayers.Count - 1);
            Destroy(lastClone);
        }

        // Oyuncu sayısını eksilt
        currentCount--;
        currentCount = Mathf.Max(0, currentCount);
        UpdateCountUI();
        FormatSubPlayers();

        // 3. Hiç karakter kalmadıysa Game Over tetikle
        if (currentCount <= 0)
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SetGameOver();
            }
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
    }
}