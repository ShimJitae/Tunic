using UnityEngine;

namespace Tunic.BossCombat
{
    /// <summary>Attach a concrete implementation when the special attack design is ready.</summary>
    public abstract class BossSpecialAttack : MonoBehaviour
    {
        public virtual bool IsAvailable => isActiveAndEnabled;
        public abstract void Begin(BossController boss);
        /// <returns>True when the cast has finished.</returns>
        public abstract bool Tick(float deltaTime);
        public abstract void End(bool interrupted);
    }
}
