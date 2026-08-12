using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    [Header("Sarsıntı Ayarları")]
    [SerializeField] private float cooldownTime = 0.15f; // İki sarsıntı arasındaki minimum süre
    private float lastShakeTime;

    private Coroutine shakeCoroutine;
    private Vector3 lastShakeOffset = Vector3.zero;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void Shake(float duration = 0.1f, float magnitude = 0.12f)
    {
        // Çok sık aralıklarla sallantı gelirse es geç (Sürünün her üyesi için tekrar tekrar sallama)
        if (Time.time < lastShakeTime + cooldownTime) return;
        lastShakeTime = Time.time;

        if (shakeCoroutine != null)
        {
            StopCoroutine(shakeCoroutine);
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

        transform.localPosition -= lastShakeOffset;
        lastShakeOffset = Vector3.zero;
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            transform.localPosition -= lastShakeOffset;

            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            lastShakeOffset = new Vector3(x, y, 0f);

            transform.localPosition += lastShakeOffset;

            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        transform.localPosition -= lastShakeOffset;
        lastShakeOffset = Vector3.zero;
    }
}