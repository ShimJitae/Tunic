using System;
using Unity.Behavior;

[BlackboardEnum]
public enum BossState
{
	Idle,
	Intro,
	Chase,
	Combat,
	//SpecialAttack,
	Die
}
