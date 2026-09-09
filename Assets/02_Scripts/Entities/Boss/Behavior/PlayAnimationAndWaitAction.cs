using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.Scripting.APIUpdating;

[Serializable, GeneratePropertyBag]
[MovedFrom(true, sourceNamespace: "", sourceAssembly: "Assembly-CSharp", sourceClassName: "PlayBossAttackAction")]
[NodeDescription(name: "Play Animation And Wait", story: "[Agent] plays animation [StatePath] with [TransitionDuration] seconds of blending and waits for completion", category: "Action", id: "ac4a9db7d9f1d271537a563d71fd0b72")]
public partial class PlayAnimationAndWaitAction : Action
{
    [SerializeReference]
    public BlackboardVariable<GameObject> Agent;

    [SerializeReference]
    public BlackboardVariable<string> StatePath =
        new BlackboardVariable<string>();

    [SerializeReference]
    public BlackboardVariable<float> TransitionDuration =
        new BlackboardVariable<float>(0.05f);

    private Animator animator;

    private int stateHash;
    private bool hasEnteredState;

    protected override Status OnStart()
    {
        if (Agent == null || Agent.Value == null)
        {
            LogFailure("Play Animation And Wait: Agent가 연결되지 않았습니다.");

            return Status.Failure;
        }

        if (StatePath == null || string.IsNullOrWhiteSpace(StatePath.Value))
        {
            LogFailure("Play Animation And Wait: StatePath가 비어 있습니다.");

            return Status.Failure;
        }

        animator = Agent.Value.GetComponentInChildren<Animator>();

        if (animator == null)
        {
            LogFailure($"Play Animation And Wait: {Agent.Value.name}의 자식에서 Animator를 찾지 못했습니다.");

            return Status.Failure;
        }

        if (animator.runtimeAnimatorController == null)
        {
            LogFailure("Play Animation And Wait: Animator Controller가 연결되지 않았습니다.");

            return Status.Failure;
        }

        stateHash = Animator.StringToHash(StatePath.Value);

        if (!animator.HasState(0, stateHash))
        {
            LogFailure($"Play Animation And Wait: Animator에 '{StatePath.Value}' 상태가 없습니다.");

            return Status.Failure;
        }

        hasEnteredState = false;

        float transitionDuration = Mathf.Max(0f, TransitionDuration?.Value ?? 0.05f);

        /*
         * layerIndex를 0으로 지정한다.
         * fixedTimeOffset을 0으로 지정해 같은 상태가 연속으로
         * 선택되어도 애니메이션을 처음부터 다시 시작한다.
         */
        animator.CrossFadeInFixedTime(stateHash, transitionDuration, 0, 0f);

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (animator == null || !animator.isActiveAndEnabled)
        {
            return Status.Failure;
        }

        /*
         * 먼저 전환 목적지를 검사한다.
         *
         * 같은 상태가 연속 선택되면 현재 상태와 다음 상태가
         * 모두 같은 Hash를 가질 수 있다. 이때 현재 상태부터
         * 검사하면 이전 재생의 normalizedTime을 보고
         * 즉시 끝났다고 판단할 수 있다.
         */
        if (animator.IsInTransition(0))
        {
            AnimatorStateInfo nextState = animator.GetNextAnimatorStateInfo(0);

            if (IsRequestedState(nextState))
            {
                hasEnteredState = true;

                if (nextState.normalizedTime >= 1f)
                {
                    return Status.Success;
                }

                return Status.Running;
            }
        }

        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);

        if (IsRequestedState(currentState))
        {
            hasEnteredState = true;

            if (currentState.normalizedTime >= 1f)
            {
                return Status.Success;
            }

            return Status.Running;
        }

        /*
         * 요청한 상태에 한 번 진입한 뒤 다른 상태로 전환됐다면
         * 해당 재생은 종료된 것으로 처리한다.
         */
        if (hasEnteredState)
        {
            return Status.Success;
        }

        /*
         * CrossFade를 요청한 직후에는 Animator가 아직
         * 요청한 상태에 들어가지 않았을 수 있으므로 기다린다.
         */
        return Status.Running;
    }

    protected override void OnEnd()
    {
        animator = null;

        stateHash = 0;
        hasEnteredState = false;
    }

    private bool IsRequestedState(AnimatorStateInfo state)
    {
        // 기존 StatePath 값을 유지하면서 전체 경로와 짧은 상태 이름을 모두 지원한다.
        return state.fullPathHash == stateHash || state.shortNameHash == stateHash;
    }
}
