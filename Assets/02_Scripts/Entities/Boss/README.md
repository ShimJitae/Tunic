# 보스 전투 설정

`Assets/06_Scenes/BossScene.unity`를 열고 Play로 실행합니다.

## 그래프와 데이터

- `Assets/03_Data/Boss/BossBehavior.asset`: 편집 가능한 Unity Behavior 그래프입니다. 근접 가지의 거리 검사 → 바라보기 → 한 타 재생 → 다음 타 → 연결 간격을 직접 확인할 수 있습니다.
- `Assets/03_Data/Boss/BossCombat.asset`: 체력, 유지 거리, 후퇴/접근 속도, 공격 간격, 공격 목록, 대상/장애물 레이어를 설정합니다. 모든 거리는 XZ 평면의 캐릭터 중심 간 거리입니다.
- 같은 폴더의 `MeleeCombo1`, `MeleeCombo2`, `Charge`, `Ranged`: 공격별 사거리, 콤보 중단 거리, 후딜, 모션과 판정 구간을 설정합니다. `Attack_1`과 `Attack_2`는 각각 3타 근접 콤보, `Attack_3`은 돌진, `Attack_4`는 한 모션에서 투사체 1발을 발사하는 원거리 공격입니다.
- 보스 프리팹: `Assets/Imports/Prefabs/Monster/Boss.prefab`. 기존 일반 몬스터의 HFSM을 사용하지 않습니다.

Blackboard에는 `Target`, `SelectedAttack`, `MotionIndex`(0부터 시작), `ComboCancelled`, `NextAttackTime`, `SpecialPending`, `SpecialUsed`가 표시됩니다. `BossController.CurrentTask`와 공격 모듈의 누적 모션/발사 횟수도 런타임 진단에 사용할 수 있습니다.

## 콤보 모션 추가

1. Boss Animator의 Base Layer에 새 모션 상태를 추가합니다. 자동 전환은 필요하지 않습니다.
2. 해당 공격 데이터의 `Motions` 목록에 항목을 추가하고, `Animator State`에 `Base Layer.상태이름`, `Clip`에 같은 클립을 지정합니다.
3. `Speed`, `Hit Start/End`(0~1 정규화 시간), `Damage`와 `Hit Center/Half Extents`를 조절합니다. 판정 박스는 보스 루트 기준이며 선택 상태에서 빨간 Gizmo로 확인합니다.

같은 상태/클립을 여러 항목에 넣어도 각 타는 0부터 다시 재생됩니다. 콤보 도중 거리를 한 번이라도 벗어나면 현재 모션 종료 후 취소하며, 돌아와도 예약을 해제하지 않습니다. 모션을 늘려도 그래프 수정은 필요하지 않습니다.

현재 판정 시간과 박스는 첫 플레이테스트용 기본값입니다. 최종 모션의 실제 접촉 시점에 맞춰 공격 데이터에서 조절합니다. 애니메이션 이벤트를 추가하지 않아도 동작합니다.

## 원거리 공격

`Ranged` 데이터의 한 모션과 Animator의 `Boss_Ranged` 상태는 모두 `Attack_4/Attack_4.anim`을 사용합니다. 준비 → Attack_4 재생 → 투사체 1발 발사 → 모션 완료 → 후딜 순서로 진행합니다. `Fire Time`의 기본값은 `0.5`이며, 모션 진행률 50%에서 발사합니다. 발사 시점은 Inspector에서 조절할 수 있습니다.

## 특수 공격 연결

`BossSpecialAttack`을 상속한 컴포넌트를 작성해 프리팹의 `Special Attack`에 연결합니다.

- `Begin(BossController)`: 시전 시작. 실행별 상태를 초기화합니다.
- `Tick(float)`: 진행 중에는 false, 시전 완료 시 true를 반환합니다.
- `End(bool interrupted)`: 정상 완료 또는 중단 시 연출·이동·생성 오브젝트를 정리합니다.

무적은 컨트롤러가 관리합니다. 체력 50% 미만에서 최초 1회 예약하고, 재생 중인 모션이 끝난 후 시전합니다. 사망이나 대상 소실·비활성화는 즉시 정리합니다. 특수 컴포넌트가 지정되지 않으면 예약만 남기고 일반 전투를 계속합니다. 실제 특수 공격은 아직 구현/지정하지 않았습니다.

## NavMesh와 검증

보스 전용 NavMesh Agent Type `Boss`는 반경 1m, 높이 3m입니다. 기존 `Enemy` 타입은 유지합니다. BossScene 바닥의 NavMeshSurface에서 지형 변경 후 다시 Bake합니다. 후퇴 애니메이션은 현재 Move를 재사용합니다.

`Tools > Boss > Run Play Mode Validation`은 **BossScene의 Play 모드에서** 실행합니다. 임시 보스/대상과 복제 데이터를 사용해 콤보·재조준·후딜·무적·돌진·투사체·거리 유지·잘못된 모션을 검증합니다. 결과는 `Library/BossCombatValidation.json`에 저장합니다. 테스트용 특수 컴포넌트는 `UNITY_EDITOR`에서만 컴파일하며 실제 프리팹에 연결하지 않습니다.

`Tools > Boss > Set Up Combat Assets and Current BossScene`은 초기 구성용입니다. 기존 공격 수치와 그래프 편집은 보존하고 누락 에셋·컴포넌트를 연결합니다. 기존 원거리 임시 모션도 Attack_4로 교체합니다. 현재 BossScene을 저장하고 NavMesh를 다시 Bake하므로, 단순 수치 조정에는 실행할 필요가 없습니다.
