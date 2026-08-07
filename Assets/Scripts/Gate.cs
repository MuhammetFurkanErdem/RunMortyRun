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

            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;

            // Singleton üzerinden doğrudan PlayerManager'a ulaş
            if (PlayerManager.Instance != null)
            {
                PlayerManager.Instance.ApplyGateOperation(gateType, gateValue);
            }

            // Kapıyı tamamen gizle
            gameObject.SetActive(false);
        }

        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.ApplyGateOperation(gateType, gateValue);

            // Ses Efekti Tetikleme
            if (AudioManager.Instance != null)
            {
                if (gateType == GateType.Add || gateType == GateType.Multiply)
                    AudioManager.Instance.PlayPositiveGate();
                else
                    AudioManager.Instance.PlayNegativeGate();
            }
        }
    }
}