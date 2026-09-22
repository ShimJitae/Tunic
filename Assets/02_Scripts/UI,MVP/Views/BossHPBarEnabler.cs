using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class BossHPBarEnabler : MonoBehaviour
{
    [SerializeField, Min(0f)] private float slideDuration = 0.8f;
    [SerializeField, Min(0f)] private float offscreenPadding = 20f;
    [SerializeField] private Ease slideEase = Ease.OutCubic;

    [SerializeField] private RectTransform rectTransform;

    [SerializeField] private Health bossHealth;
    [SerializeField] private GameObject clearUI;
    private Vector2 visiblePosition;
    private Tween slideTween;

    private void Awake()
    {
        if (rectTransform == null)
            rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        slideTween?.Kill();

        Canvas.ForceUpdateCanvases();
        visiblePosition = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = GetOffscreenPosition();

        slideTween = rectTransform.DOAnchorPos(visiblePosition, slideDuration)
            .SetEase(slideEase)
            .SetUpdate(true)
            .OnKill(() => slideTween = null);

        bossHealth.OnDied += SetUpClearUI;
    }

    private void OnDisable()
    {
        slideTween?.Kill();

        if (rectTransform != null)
            rectTransform.anchoredPosition = visiblePosition;


        bossHealth.OnDied -= SetUpClearUI;
    }

    void Start()
    {
        clearUI.SetActive(false);
    }

    private void SetUpClearUI()
    {
        if (clearUI.activeSelf)
        {
            clearUI.SetActive(false);
        }
        else
        {
            clearUI.SetActive(true);
        }
    }

    private Vector2 GetOffscreenPosition()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        RectTransform boundary = canvas != null
            ? canvas.rootCanvas.transform as RectTransform
            : rectTransform.parent as RectTransform;

        if (boundary == null)
            return visiblePosition + Vector2.up * (rectTransform.rect.height + offscreenPadding);

        // Move the entire bar, including its children, above the canvas in canvas units.
        Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(boundary, rectTransform);
        float offset = Mathf.Max(0f, boundary.rect.yMax - bounds.min.y + offscreenPadding);
        Vector3 worldOffset = boundary.TransformVector(Vector3.up * offset);
        Vector3 localOffset = rectTransform.parent != null
            ? rectTransform.parent.InverseTransformVector(worldOffset)
            : worldOffset;

        return visiblePosition + (Vector2)localOffset;
    }
}
