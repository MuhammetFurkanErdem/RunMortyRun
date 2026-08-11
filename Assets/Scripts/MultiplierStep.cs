using UnityEngine;
using TMPro;

public class MultiplierStep : MonoBehaviour
{
    [Header("Basamak Ayarları")]
    public float multiplierValue = 1.5f; // Örn: 1.5, 2.0, 5.0, 10.0
    public int requiredClonesToPass = 3;  // Bu basamağı geçmek için harcanacak kopya sayısı

    [Header("Görsel Referanslar")]
    [SerializeField] private TextMeshPro multiplierText;

    private bool isPassed = false;

    private void Start()
    {
        UpdateText();
    }

    private void UpdateText()
    {
        if (multiplierText != null)
        {
            multiplierText.text = "x" + multiplierValue.ToString("F1") + "\n(" + requiredClonesToPass + ")";
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isPassed) return;

        if (other.CompareTag("Player"))
        {
            if (FinishLine.Instance != null && PlayerManager.Instance != null)
            {
                int currentCrowd = PlayerManager.Instance.GetCrowdCount();

                // Yeterli kopya var mı?
                if (currentCrowd >= requiredClonesToPass)
                {
                    isPassed = true;

                    // 1. Çarpanı güncelle
                    FinishLine.Instance.SetCurrentMultiplier(multiplierValue);

                    // 2. Basamak bedelini öde (kopyaları azalt)
                    PlayerManager.Instance.ApplyGateOperation(GateType.Subtract, requiredClonesToPass);
                }
                else
                {
                    // Yeterli kopya kalmadı! Tırmanış burada biter!
                    isPassed = true;
                    FinishLine.Instance.StopClimbingAndFinish();
                }
            }
        }
    }
}