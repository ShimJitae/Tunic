using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SFXPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip clip;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    public void Play()
    {
        if (!isActiveAndEnabled || audioSource == null || clip == null)
            return;

        audioSource.PlayOneShot(clip, volume);
    }

    // Health.OnDamaged처럼 float를 전달하는 이벤트에 연결합니다.
    public void Play(float _)
    {
        Play();
    }
}
