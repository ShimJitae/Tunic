#if UNITY_EDITOR
using Tunic.BossCombat;

// Test-only cast: no production special attack is assigned by setup.
public sealed class BossValidationSpecial : BossSpecialAttack
{
    public float duration = 0.6f;
    public int begins;
    public int ends;
    public bool interrupted;
    private float elapsed;
    public override void Begin(BossController boss) { begins++; elapsed = 0f; }
    public override bool Tick(float deltaTime) { elapsed += deltaTime; return elapsed >= duration; }
    public override void End(bool wasInterrupted) { ends++; interrupted = wasInterrupted; }
}
#endif
