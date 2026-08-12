using UnityEngine;

public class CloneDeathEffect : MonoBehaviour
{
    public static void PlayDeath(GameObject victim, GameObject particlePrefab)
    {
        if (victim == null) return;

        // 1. Toz / Poof Efektini Karakterin Göğüs Hizasında Patlat
        if (particlePrefab != null)
        {
            Vector3 spawnPos = victim.transform.position + Vector3.up * 0.8f;
            GameObject fx = Instantiate(particlePrefab, spawnPos, Quaternion.identity);

            // Efekti 1.5 saniye sonra hafızadan temizle
            Destroy(fx, 1.5f);
        }

        // 2. Kopyayı Anında Ortadan Kaldır
        Destroy(victim);
    }
}