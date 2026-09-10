using UnityEngine;

public class PlaySFXAnimationEventRelay : MonoBehaviour
{
    [Tooltip("애니메이션 이벤트의 Int 값으로 재생할 효과음을 선택합니다. 번호는 0부터 시작합니다.")]
    [SerializeField] private SFXPlayer[] sfxPlayers;

    public void PlaySFX(int index)
    {
        if (!isActiveAndEnabled || sfxPlayers == null || index < 0 || index >= sfxPlayers.Length)
            return;

        SFXPlayer sfxPlayer = sfxPlayers[index];
        if (sfxPlayer != null)
            sfxPlayer.Play();
    }
}
