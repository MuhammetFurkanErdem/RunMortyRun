using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PlayerManager : MonoBehaviour
{
    [Header("Oyuncu Sayısı")]
    [SerializeField] private int currentCount = 1;
    [SerializeField] private TextMeshPro countText;

    [Header("Kalabalık (Mob) Ayarları")]
    [SerializeField] private GameObject playerPrefab; // Project'ten atayacağınız Karakter Prefab'ı
    [SerializeField] private Transform mobHolder;     // Klonsal karakterlerin toplanacağı Parent obje
    [SerializeField] private float distanceFactor = 0.25f; // Karakterler arası mesafe çarpanı

    // Sahnede aktif duran klon karakterlerin listesi
    private List<GameObject> subPlayers = new List<GameObject>();

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

    private void UpdateMobVisuals()
    {
        // Ana karakter zaten sahnede olduğu için kopyalanacak sayı: (currentCount - 1)
        int targetSubPlayerCount = currentCount - 1;

        // 1. EĞER EKSİK KARAKTER VARSA: Yeni Karakterler Üret
        while (subPlayers.Count < targetSubPlayerCount)
        {
            Transform parentTransform = mobHolder != null ? mobHolder : transform;
            GameObject newSubPlayer = Instantiate(playerPrefab, parentTransform);
            subPlayers.Add(newSubPlayer);
        }

        // 2. EĞER FAZLA KARAKTER VARSA: Karakterleri Sil
        while (subPlayers.Count > targetSubPlayerCount && subPlayers.Count > 0)
        {
            GameObject lastPlayer = subPlayers[subPlayers.Count - 1];
            subPlayers.RemoveAt(subPlayers.Count - 1);
            Destroy(lastPlayer);
        }

        // 3. KALABALIK FORMASYONUNU YENİDEN HİZALA
        FormatSubPlayers();
    }

    private void FormatSubPlayers()
    {
        for (int i = 0; i < subPlayers.Count; i++)
        {
            // Altın Oran / Fermat Spiral Algoritması ile halka şeklinde düzenleme
            float phi = (i + 1) * 137.5f * Mathf.Deg2Rad;
            float r = distanceFactor * Mathf.Sqrt(i + 1);

            float x = r * Mathf.Cos(phi);
            float z = r * Mathf.Sin(phi);

            // Yerel pozisyonu ayarla (Ana karakter merkezde kalır)
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