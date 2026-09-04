using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Tunic.BossCombat;
using Unity.Behavior;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

/// <summary>Play-mode integration checks using temporary prefab instances and cloned data only.</summary>
public static class BossCombatValidation
{
    [Serializable] private sealed class Report
    {
        public bool complete;
        public string failure;
        public List<string> passed = new List<string>();
    }
    private static Report report;
    private static readonly Stack<IEnumerator> routines = new Stack<IEnumerator>();
    private static readonly List<Object> temporary = new List<Object>();
    private static readonly List<GameObject> suspended = new List<GameObject>();
    private static BossController boss;
    private static Health target;
    private static BossCombatData data;
    private static BossAttackData attack;
    private static bool background;
    private static readonly Vector3 Origin = new Vector3(-4f, 0.5f, -3f);
    public const string ReportPath = "Library/BossCombatValidation.json";

    [MenuItem("Tools/Boss/Run Play Mode Validation")]
    public static void Run()
    {
        if (!EditorApplication.isPlaying) throw new InvalidOperationException("Enter Play mode in BossScene first.");
        if (routines.Count > 0) throw new InvalidOperationException("Validation is already running.");
        report = new Report();
        background = Application.runInBackground;
        Application.runInBackground = true;
        foreach (var item in Object.FindObjectsByType<BossController>(FindObjectsSortMode.None)) Suspend(item.gameObject);
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) Suspend(player);
        routines.Push(Scenarios());
        EditorApplication.update += Update;
        WriteReport();
    }

    private static void Suspend(GameObject go) { suspended.Add(go); go.SetActive(false); }
    private static void Update()
    {
        try
        {
            if (!EditorApplication.isPlaying) throw new InvalidOperationException("Play mode ended during validation.");
            EditorApplication.QueuePlayerLoopUpdate();
            while (routines.Count > 0)
            {
                var current = routines.Peek();
                if (!current.MoveNext()) { routines.Pop(); continue; }
                if (current.Current is IEnumerator child) { routines.Push(child); continue; }
                return;
            }
            Finish(null);
        }
        catch (Exception exception) { Finish(exception.ToString()); }
    }

    private static void Finish(string failure)
    {
        EditorApplication.update -= Update;
        routines.Clear();
        CleanFixture();
        foreach (var go in suspended) if (go != null) go.SetActive(true);
        suspended.Clear();
        Application.runInBackground = background;
        report.failure = failure;
        report.complete = true;
        WriteReport();
        if (failure == null) Debug.Log("Boss validation passed: " + report.passed.Count + " scenarios. " + ReportPath);
        else Debug.LogError("Boss validation failed: " + failure);
    }

    private static void WriteReport() => File.WriteAllText(ReportPath, JsonUtility.ToJson(report, true));
    private static void Passed(string name) { report.passed.Add(name); WriteReport(); }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static IEnumerator Wait(Func<bool> condition, float timeout, string reason)
    {
        float deadline = Time.realtimeSinceStartup + timeout;
        while (!condition())
        {
            if (Time.realtimeSinceStartup > deadline)
                throw new TimeoutException(reason + " task=" + boss?.CurrentTask + " motions=" + boss?.Attack.MotionStartCount);
            yield return null;
        }
    }
    private static IEnumerator Seconds(float duration)
    {
        float end = Time.time + duration;
        while (Time.time < end) yield return null;
    }

    private static void Spawn(int attackIndex, float distance = 3.2f, float interval = 0.15f)
    {
        CleanFixture();
        var source = AssetDatabase.LoadAssetAtPath<BossCombatData>(BossCombatSetup.DataFolder + "/BossCombat.asset");
        data = Object.Instantiate(source);
        temporary.Add(data);
        data.attackInterval = interval;
        if (attackIndex >= 0)
        {
            attack = Object.Instantiate(source.attacks[attackIndex]);
            temporary.Add(attack);
            data.attacks = new[] { attack };
        }
        else { attack = null; data.attacks = new BossAttackData[0]; }
        var dummy = new GameObject("Boss validation target");
        temporary.Add(dummy);
        dummy.layer = LayerMask.NameToLayer("Player");
        dummy.transform.position = Origin + Vector3.forward * distance;
        var capsule = dummy.AddComponent<CapsuleCollider>();
        capsule.center = Vector3.up; capsule.height = 2f; capsule.radius = 0.5f;
        target = dummy.AddComponent<Health>();
        target.SetUpData(data);
        var sourcePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BossCombatSetup.PrefabPath);
        var go = Object.Instantiate(sourcePrefab, Origin, Quaternion.identity);
        go.name = "Boss validation instance";
        temporary.Add(go);
        boss = go.GetComponent<BossController>();
        var serialized = new SerializedObject(boss);
        serialized.FindProperty("data").objectReferenceValue = data;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        boss.SetTarget(dummy.transform);
        Physics.SyncTransforms();
    }

    private static BossValidationSpecial Special(float duration = 0.6f)
    {
        var special = boss.gameObject.AddComponent<BossValidationSpecial>();
        special.duration = duration;
        var serialized = new SerializedObject(boss);
        serialized.FindProperty("specialAttack").objectReferenceValue = special;
        serialized.ApplyModifiedPropertiesWithoutUndo();
        return special;
    }

    private static void MoveTarget(Vector3 direction, float distance)
    {
        target.transform.position = boss.transform.position + direction * distance;
        Physics.SyncTransforms();
    }

    private static void CleanFixture()
    {
        for (int i = temporary.Count - 1; i >= 0; i--) if (temporary[i] != null) Object.DestroyImmediate(temporary[i]);
        temporary.Clear();
        boss = null; target = null; data = null; attack = null;
    }

    private static IEnumerator Scenarios()
    {
        Spawn(0, 3.2f, 2.5f);
        var duplicate = target.gameObject.AddComponent<BoxCollider>();
        duplicate.center = Vector3.up; duplicate.size = new Vector3(0.6f, 1.5f, 0.6f);
        yield return Seconds(0.5f);
        Require(boss.Attack.MotionStartCount == 0, "First attack ignored its initial cooldown.");
        yield return Wait(() => boss.Attack.IsMotionPlaying, 4f, "First melee motion did not start.");
        boss.Health.TakeDamage(1f);
        Require(boss.Attack.IsMotionPlaying, "Normal damage interrupted the boss.");
        yield return Wait(() => boss.CurrentTask == BossTask.Recovery, 12f, "Three-hit combo did not finish.");
        Require(boss.Attack.MotionStartCount == 3, "Melee combo did not play exactly three motions.");
        Require(Mathf.Approximately(target.CurrHP, target.MaxHP - 30f), "A motion missed or dealt duplicate damage: " + target.CurrHP);
        Vector3 recoveryPosition = boss.transform.position;
        MoveTarget(Vector3.forward, 1.8f);
        yield return Seconds(0.35f);
        Require(Vector3.Distance(recoveryPosition, boss.transform.position) < 0.05f, "Boss moved during recovery.");
        yield return Wait(() => boss.SelectedAttack == null, 2f, "Cooldown was not scheduled.");
        float scheduled = boss.NextAttackTime;
        Require(scheduled > Time.time + 2.2f, "Cooldown did not begin after recovery.");
        yield return Seconds(0.5f);
        Require(boss.Attack.MotionStartCount == 3, "Boss attacked before post-recovery cooldown.");
        Passed("3-hit combo, duplicate colliders, damage armor, stationary recovery, 2.5s cooldown");

        Spawn(0);
        yield return Wait(() => boss.Attack.IsMotionPlaying, 3f, "Cancel fixture did not attack.");
        MoveTarget(Vector3.forward, 6f);
        yield return Wait(() => boss.ComboCancelled, 1f, "Range exit was not latched.");
        MoveTarget(Vector3.forward, 3.2f);
        Require(boss.Attack.IsMotionPlaying, "Distance exit cut the current motion short.");
        yield return Wait(() => boss.CurrentTask == BossTask.Recovery, 4f, "Cancelled combo did not recover.");
        Require(boss.Attack.MotionStartCount == 1, "Returning target resumed a cancelled combo.");
        Passed("Range exit stays latched after return; current motion finishes before cancellation");

        Spawn(0);
        var repeated = attack.motions[0];
        attack.motions = new[] { repeated, repeated, repeated };
        yield return Wait(() => boss.Attack.MotionStartCount == 1 && !boss.Attack.IsMotionPlaying, 5f, "Repeated first clip did not finish.");
        MoveTarget(Vector3.back, 3.2f);
        yield return Wait(() => boss.Attack.MotionStartCount == 2, 2f, "Repeated second clip did not start.");
        Require(Vector3.Dot(boss.transform.forward, Vector3.back) > 0.99f, "Second combo motion did not snap to target.");
        yield return Wait(() => boss.Attack.MotionStartCount == 2 && !boss.Attack.IsMotionPlaying, 3f, "Second clip did not finish.");
        MoveTarget(Vector3.right, 3.2f);
        yield return Wait(() => boss.Attack.MotionStartCount == 3, 2f, "Third repeated clip did not start.");
        Require(Vector3.Dot(boss.transform.forward, Vector3.right) > 0.99f, "Third combo motion did not reacquire target.");
        yield return Wait(() => boss.CurrentTask == BossTask.Recovery, 3f, "Repeated combo did not complete.");
        Require(Mathf.Approximately(target.CurrHP, target.MaxHP - 30f), "Repeated clips did not create three independent hit windows.");
        Passed("Same clip repeated three times with independent damage and snap facing between motions");

        Spawn(0);
        var special = Special();
        yield return Wait(() => boss.Attack.IsMotionPlaying, 3f, "Special fixture did not attack.");
        boss.Health.TakeDamage(boss.Health.MaxHP * 0.5f);
        Require(!boss.SpecialPending, "Special triggered at exactly 50 percent.");
        boss.Health.TakeDamage(1f);
        Require(boss.SpecialPending && !boss.Health.IsInvincible && boss.Attack.IsMotionPlaying, "Special interrupted an active motion or gave early invulnerability.");
        yield return Wait(() => special.begins == 1, 4f, "Special did not start at a motion boundary.");
        Require(boss.Attack.MotionStartCount == 1 && boss.Health.IsInvincible, "Special did not replace remaining combo with an invulnerable cast.");
        float hp = boss.Health.CurrHP;
        boss.Health.TakeDamage(999f);
        Require(boss.Health.CurrHP == hp, "Special cast took damage.");
        yield return Wait(() => special.ends == 1, 2f, "Special cast did not end.");
        Require(!boss.Health.IsInvincible && boss.SpecialUsed, "Special left invulnerability enabled.");
        boss.Health.RestoreHp(100f); boss.Health.TakeDamage(100f);
        Require(!boss.SpecialPending, "Used special was reserved again after healing.");
        Passed("Strict 50 percent threshold, motion boundary, once-only special and cast-only invulnerability");

        Spawn(0);
        special = Special(10f);
        yield return Wait(() => boss.Ready, 1f, "Boss did not initialize.");
        boss.Health.TakeDamage(151f);
        yield return Wait(() => boss.IsCastingSpecial, 2f, "Disable fixture did not cast.");
        boss.gameObject.SetActive(false);
        Require(!boss.Health.IsInvincible && special.interrupted && special.ends == 1, "Disable failed to clean up special invulnerability.");
        Passed("Disabling the boss interrupts the special and always clears invulnerability");

        Spawn(0);
        yield return Wait(() => boss.Attack.IsMotionPlaying, 3f, "Unassigned special fixture did not attack.");
        boss.Health.TakeDamage(151f);
        yield return Wait(() => boss.CurrentTask == BossTask.Recovery, 12f, "Unassigned special blocked combat.");
        Require(boss.Attack.MotionStartCount == 3 && boss.SpecialPending && !boss.Health.IsInvincible, "Unassigned special changed normal combat.");
        boss.Health.TakeDamage(1000f);
        yield return null;
        Require(boss.IsDead && !boss.Attack.HitWindowOpen && !boss.Health.IsInvincible, "Lethal damage did not clean up combat.");
        Passed("Unassigned special does not block combos; lethal damage takes priority");

        Spawn(2, 8f);
        yield return Wait(() => boss.Attack.IsMotionPlaying, 3f, "Charge did not start.");
        Vector3 chargeStart = boss.transform.position;
        Vector3 heading = boss.Movement.ChargeDirection;
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        temporary.Add(wall);
        wall.transform.position = chargeStart + heading * 4.5f + Vector3.up * 1.5f;
        wall.transform.localScale = new Vector3(6f, 3f, 0.4f);
        Physics.SyncTransforms();
        MoveTarget(Vector3.right, 7f);
        yield return Wait(() => boss.CurrentTask == BossTask.Recovery, 6f, "Charge did not finish.");
        float traveled = Vector3.Dot(boss.transform.position - chargeStart, heading);
        Require(traveled > 0.1f && traveled < 3.6f, "Charge failed to stop before the wall: " + traveled);
        Require(Vector3.Dot(boss.transform.forward, heading) > 0.999f, "Charge tracked the target after commitment.");
        Passed("Charge direction remains fixed and swept collision stops at a wall");

        Spawn(3, 8f);
        yield return Wait(() => boss.Attack.ProjectilesFired == 1, 4f, "Ranged attack did not fire.");
        var projectile = Object.FindFirstObjectByType<BossProjectile>();
        Require(projectile != null, "Projectile vanished at the muzzle.");
        Vector3 projectileHeading = projectile.transform.forward;
        MoveTarget(Vector3.right, 6f);
        yield return Seconds(0.15f);
        Require(projectile != null && Vector3.Dot(projectile.transform.forward, projectileHeading) > 0.999f, "Projectile homed after release.");
        target.TakeDamage(1000f);
        yield return Seconds(0.1f);
        Require(Object.FindObjectsByType<BossProjectile>(FindObjectsSortMode.None).Length == 0 && !boss.Attack.HitWindowOpen, "Target death left active projectiles or hit windows.");
        Passed("One non-homing projectile; target death cleans up projectiles and hit windows");

        Spawn(-1, 2f);
        yield return Wait(() => boss.Ready, 1f, "Movement fixture did not initialize.");
        float initial = boss.Distance;
        yield return Seconds(0.5f);
        Require(boss.Distance > initial + 0.2f && boss.Distance < 4.1f, "Close target did not cause gradual retreat.");
        MoveTarget(Vector3.forward, 8f);
        yield return Seconds(0.5f);
        Require(boss.Distance < 7.5f, "Far target did not cause approach.");
        yield return Wait(() => boss.Distance < 4f, 4f, "Approach never reached the desired band.");
        yield return Seconds(0.7f);
        Vector3 stopped = boss.transform.position;
        yield return Seconds(0.3f);
        Require(Vector3.Distance(stopped, boss.transform.position) < 0.15f && boss.GetComponent<NavMeshAgent>().isOnNavMesh, "Distance band did not settle.");
        Passed("Gradual retreat, approach, stable distance band and valid NavMesh placement");

        Spawn(0);
        attack.motions[0].animatorState = "Base Layer.MissingValidationState";
        yield return Seconds(0.7f);
        Require(boss.Attack.MotionStartCount == 0 && !boss.Attack.HitWindowOpen && boss.GetComponent<BehaviorGraphAgent>().Graph.IsRunning,
            "Missing Animator state stopped the BT or opened an invalid hit window.");
        Passed("Invalid motion is excluded without blocking the Behavior graph");
    }
}
