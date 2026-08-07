using UnityEngine;
using UnityEngine.EventSystems;

public class Joystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("Joystick Bileşenleri")]
    [SerializeField] private RectTransform background;
    [SerializeField] private RectTransform handle;

    [Header("Ayarlar")]
    [SerializeField] private float handleRange = 1f;

    private Vector2 input = Vector2.zero;

    public float Horizontal => input.x;
    public float Vertical => input.y;

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 position = RectTransformUtility.WorldToScreenPoint(null, background.position);
        Vector2 radius = background.sizeDelta / 2f;

        // Dokunulan noktanın joystick merkezine olan mesafesini -1 ile 1 arasında hesapla
        input = (eventData.position - position) / (radius * handleRange);

        if (input.magnitude > 1f)
        {
            input = input.normalized;
        }

        // Yuvarlağı (Handle) hareket ettir
        handle.anchoredPosition = input * radius * handleRange;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Dokunma bırakıldığında joystick'i merkeze sıfırla
        input = Vector2.zero;
        handle.anchoredPosition = Vector2.zero;
    }
}