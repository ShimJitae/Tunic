using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip clip;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (audioSource == null)
        {
            if (!audioSource.TryGetComponent(out audioSource))
                Debug.LogError($"SFXPlayer : {gameObject.name}에 AudioSource가 없습니다.");
        }
        else
        {
            audioSource.playOnAwake = false;
        }
    }

    public void Play()
    {
        if (!isActiveAndEnabled || audioSource == null || clip == null)
        {
            Debug.LogError("SFXPlayer : " + $"{gameObject.name}에서 SFX를 재생할 수 없습니다. " +
                $"isActiveAndEnabled: {isActiveAndEnabled}, audioSource: {audioSource}, clip: {clip}");
            return;
        }

        audioSource.PlayOneShot(clip, volume);
    }

    // Health.OnDamaged처럼 float를 전달하는 이벤트에 연결합니다.
    public void Play(float _)
    {
        Play();
    }
}
