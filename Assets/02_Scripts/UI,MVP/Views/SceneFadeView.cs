using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class SceneFadeView : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeDuration = 0.5f;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    void Start()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;
    }

    public async UniTask FadeInAsync()
    {
        canvasGroup.DOKill(); // 기존에 실행되고 있는 tween이 있었다면 Kill 

        canvasGroup.blocksRaycasts = true;

        await canvasGroup
        .DOFade(1f, fadeDuration)
        .SetUpdate(true) // Time.timeScale에 영향을 받지 않고 DOTween 애니메이션 실행
        .AsyncWaitForCompletion(); // UniTask 메서드에서 DOTween이 끝날때까지 기다림
    }

    public async UniTask FadeOutAsync()
    {
        canvasGroup.DOKill();

        await canvasGroup
        .DOFade(0f, fadeDuration)
        .SetUpdate(true)
        .AsyncWaitForCompletion();

        canvasGroup.blocksRaycasts = false;
    }

    public void Onestroy()
    {
        canvasGroup.DOKill();
    }
}
