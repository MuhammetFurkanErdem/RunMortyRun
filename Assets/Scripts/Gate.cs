using UnityEngine;
using TMPro;

public enum GateType { Add, Subtract, Multiply, Divide }

public class Gate : MonoBehaviour
{
    [Header("Kapı Ayarları")]
    public GateType gateType = GateType.Add;
    public int gateValue = 10;

    [Header("UI Referansı")]
    [SerializeField] private TextMeshPro valueText;

    private bool isUsed = false; // Kapının birden fazla kez tetiklenmesini önler

    private void Start()
    {
        UpdateGateText();
    }

    private void UpdateGateText()
    {
        if (valueText == null) return;

        string symbol = gateType switch
        {
            GateType.Add => "+",
            GateType.Subtract => "-",
            GateType.Multiply => "x",
            GateType.Divide => "÷",
            _ => ""
        };

        valueText.text = $"{symbol}{gateValue}";
    }

    private void OnTriggerEnter(Collider other)
    {
        // Kapı zaten kullanıldıysa işlem yapma
        if (isUsed) return;

        // Çarpan obje Oyuncu ise (veya Oyuncu grubuna aitse)
        if (other.CompareTag("Player"))
        {
            isUsed = true;

            // PlayerManager script'ini bul
            PlayerManager playerManager = other.GetComponentInParent<PlayerManager>();
            if (playerManager == null)
            {
                playerManager = other.GetComponent<PlayerManager>();
            }

            if (playerManager != null)
            {
                // İşlemi oyuncuya uygula
                playerManager.ApplyGateOperation(gateType, gateValue);
            }

            // Yan yana duran diğer kapının da aynı anda tetiklenmesini engellemek için 
            // kapıyı veya parent'ını pasife çekebilirsiniz
            gameObject.SetActive(false);
        }
    }
}