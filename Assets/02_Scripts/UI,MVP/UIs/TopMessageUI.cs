using DG.Tweening;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TopMessageUI : MonoBehaviour
{
    [SerializeField, Min(0f)] private float fadeInDuration = 0.5f;
    [SerializeField, Min(0f)] private float displayDuration = 2f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 0.5f;

    private TMP_Text messageText;
    private Sequence messageSequence;

    private void Awake()
    {
        messageText = GetComponent<TMP_Text>();
        messageText.alpha = 0f;
    }

    private void OnDisable()
    {
        messageSequence?.Kill();
        messageText.alpha = 0f;
    }

    public void Show(string message)
    {
        messageSequence?.Kill();
        messageText.text = message;
        messageText.alpha = 0f;

        messageSequence = DOTween.Sequence()
            .Append(DOVirtual.Float(0f, 1f, fadeInDuration,
                alpha => messageText.alpha = alpha).SetEase(Ease.Linear))
            .AppendInterval(displayDuration)
            .Append(DOVirtual.Float(1f, 0f, fadeOutDuration,
                alpha => messageText.alpha = alpha).SetEase(Ease.Linear))
            .SetUpdate(true)
            .OnKill(() => messageSequence = null);
    }
}
