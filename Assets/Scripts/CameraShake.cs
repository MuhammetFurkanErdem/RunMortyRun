using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    private Coroutine shakeCoroutine;
    private Vector3 lastShakeOffset = Vector3.zero;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Shake(float duration = 0.15f, float magnitude = 0.2f)
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            // Önceki sarsıntıdan kalan sapmayı temizle
            transform.localPosition -= lastShakeOffset;
            lastShakeOffset = Vector3.zero;
        }

        shakeCoroutine = StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    public void StopShake()
    {
        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
            shakeCoroutine = null;
        }

        // Sarsıntıyı anında kes ve eklenen ekstra sapmayı geri al
        transform.localPosition -= lastShakeOffset;
        lastShakeOffset = Vector3.zero;
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Bir önceki karede eklediğimiz sarsıntı farkını çıkar
            transform.localPosition -= lastShakeOffset;

            // Yeni küçük sarsıntı offseti hesapla
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            lastShakeOffset = new Vector3(x, y, 0f);

            // Kameranın anlık pozisyonuna uygula
            transform.localPosition += lastShakeOffset;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Sarsıntı bitince kamerayı tam olması gereken yere sıfırla
        transform.localPosition -= lastShakeOffset;
        lastShakeOffset = Vector3.zero;
    }
}