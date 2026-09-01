using UnityEngine;

public class PlayerAnimClipRelay : EntityAnimClipRelay
{
    public void DodgeFinished()
    {
        (entityController as PlayerController).NotifyDodgeFinished();
    }
}
