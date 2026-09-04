using System;
using System.Collections;
using System.Linq;
using System.Reflection;
using Tunic.BossCombat;
using Unity.AI.Navigation;
using Unity.Behavior;
using Unity.Behavior.GraphFramework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using Object = UnityEngine.Object;

/// <summary>Non-destructive first-time setup. Existing attack data and graph edits are retained on rerun.</summary>
public static class BossCombatSetup
{
    public const string DataFolder = "Assets/03_Data/Boss";
    public const string GraphPath = DataFolder + "/BossBehavior.asset";
    public const string PrefabPath = "Assets/Imports/Prefabs/Monster/Boss.prefab";
    private const string Clips = "Assets/Imports/Assets/Animation/Monster/Boss/";
    private const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;

    [MenuItem("Tools/Boss/Set Up Combat Assets and Current BossScene")]
    public static void SetUp()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Run setup in Edit mode.");
        if (EditorSceneManager.GetActiveScene().path != "Assets/06_Scenes/BossScene.unity")
            throw new InvalidOperationException("Open BossScene before setup; the current scene will not be replaced.");
        EnsureFolder(DataFolder);
        EnsureFolder("Assets/04_Prefabs/Boss");
        EnsureFolder("Assets/06_Scenes/BossScene");
        int agentType = EnsureBossAgent();
        var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>("Assets/05_Animation/Monster/Boss.controller");
        if (controller == null) throw new InvalidOperationException("Boss Animator controller not found.");
        var firstCombo = ClipsInFolder("Attack_1");
        var secondCombo = ClipsInFolder("Attack_2");
        var jab = firstCombo[0];
        var power = secondCombo[0];
        var move = Clip("Move.anim");
        var chargeClip = AssetDatabase.LoadAssetAtPath<AnimationClip>(Clips + "Attack_3/Attack_3.anim") ?? move;
        var rangedClip = Clip("Attack_4/Attack_4.anim");
        for (int i = 0; i < firstCombo.Length; i++) EnsureState(controller, "Boss_Melee1_" + (i + 1), firstCombo[i]);
        for (int i = 0; i < secondCombo.Length; i++) EnsureState(controller, "Boss_Melee2_" + (i + 1), secondCombo[i]);
        EnsureState(controller, "Boss_Charge", chargeClip);
        EnsureState(controller, "Boss_Ranged", rangedClip, replaceMotion: true);
        AssetDatabase.SaveAssetIfDirty(controller);
        var projectile = EnsureProjectile();
        var melee1 = EnsureAttack("MeleeCombo1", BossAttackKind.Melee, jab, "Boss_Melee1", 0f, 3.5f, projectile, firstCombo);
        var melee2 = EnsureAttack("MeleeCombo2", BossAttackKind.Melee, power, "Boss_Melee2", 0f, 4.5f, projectile, secondCombo);
        var charge = EnsureAttack("Charge", BossAttackKind.Charge, chargeClip, "Boss_Charge", 4f, 10f, projectile);
        var ranged = EnsureAttack("Ranged", BossAttackKind.Ranged, rangedClip, "Boss_Ranged", 4f, 12f, projectile);
        var data = AssetDatabase.LoadAssetAtPath<BossCombatData>(DataFolder + "/BossCombat.asset");
        if (data == null)
        {
            data = ScriptableObject.CreateInstance<BossCombatData>();
            data.attacks = new[] { melee1, melee2, charge, ranged };
            data.targetLayers = LayerMask.GetMask("Player");
            data.obstacleLayers = LayerMask.GetMask("Default");
            var serialized = new SerializedObject(data);
            serialized.FindProperty("maxHP").floatValue = 300f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(data, DataFolder + "/BossCombat.asset");
        }
        var graph = EnsureGraph();
        var prefab = PrefabUtility.LoadPrefabContents(PrefabPath);
        try
        {
            foreach (var child in prefab.GetComponentsInChildren<Transform>(true)) child.gameObject.layer = LayerMask.NameToLayer("Enemy");
            prefab.tag = "Enemy";
            var agent = GetOrAdd<NavMeshAgent>(prefab);
            agent.agentTypeID = agentType;
            agent.radius = 1f; agent.height = 3f; agent.baseOffset = 0f;
            agent.speed = 4f; agent.acceleration = 12f; agent.stoppingDistance = 0.1f;
            agent.updateRotation = false;
            var boss = GetOrAdd<BossController>(prefab);
            var serialized = new SerializedObject(boss);
            serialized.FindProperty("data").objectReferenceValue = data;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            var animation = new SerializedObject(prefab.GetComponent<BossAnimationModule>());
            animation.FindProperty("animator").objectReferenceValue = prefab.GetComponentInChildren<Animator>();
            animation.ApplyModifiedPropertiesWithoutUndo();
            prefab.GetComponent<BehaviorGraphAgent>().Graph = graph;
            PrefabUtility.SaveAsPrefabAsset(prefab, PrefabPath);
        }
        finally { PrefabUtility.UnloadPrefabContents(prefab); }
        BuildNavigation(agentType);
        EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
        Debug.Log("Boss combat configured: four attacks, editable Behavior graph, prefab, and Boss arena NavMesh.");
    }

    private static AnimationClip Clip(string path)
    {
        var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(Clips + path);
        if (clip == null) throw new InvalidOperationException("Missing clip: " + path);
        return clip;
    }

    private static AnimationClip[] ClipsInFolder(string folder)
    {
        var clips = AssetDatabase.FindAssets("t:AnimationClip", new[] { Clips + folder })
            .Select(AssetDatabase.GUIDToAssetPath).OrderBy(path => path, StringComparer.Ordinal)
            .Select(AssetDatabase.LoadAssetAtPath<AnimationClip>).ToArray();
        if (clips.Length == 0) throw new InvalidOperationException("No combo clips in " + folder);
        return clips;
    }

    private static void EnsureState(AnimatorController controller, string name, AnimationClip clip, bool replaceMotion = false)
    {
        var machine = controller.layers[0].stateMachine;
        var existing = machine.states.FirstOrDefault(child => child.state.name == name).state;
        if (existing != null)
        {
            if (replaceMotion && existing.motion != clip)
            {
                Undo.RecordObject(existing, "Connect Boss Attack_4");
                existing.motion = clip;
                EditorUtility.SetDirty(existing);
                EditorUtility.SetDirty(controller);
            }
            return;
        }
        var state = machine.AddState(name, new Vector3(620f, 80f * machine.states.Length));
        state.motion = clip;
        EditorUtility.SetDirty(controller);
    }

    private static BossAttackData EnsureAttack(string name, BossAttackKind kind, AnimationClip clip, string state, float min, float max, BossProjectile projectile, AnimationClip[] combo = null)
    {
        string path = DataFolder + "/" + name + ".asset";
        var data = AssetDatabase.LoadAssetAtPath<BossAttackData>(path);
        if (data != null)
        {
            // Upgrade the original ranged placeholder while preserving its firing time and tuning.
            if (kind == BossAttackKind.Ranged)
            {
                if (data.motions == null || data.motions.Length != 1 || data.motions[0] == null)
                    throw new InvalidOperationException("Ranged attack must contain exactly one motion.");
                var existingMotion = data.motions[0];
                if (existingMotion.clip != clip || existingMotion.animatorState != "Base Layer." + state)
                {
                    Undo.RecordObject(data, "Connect Boss Attack_4");
                    existingMotion.clip = clip;
                    existingMotion.animatorState = "Base Layer." + state;
                    EditorUtility.SetDirty(data);
                    AssetDatabase.SaveAssetIfDirty(data);
                }
            }
            return data;
        }
        data = ScriptableObject.CreateInstance<BossAttackData>();
        data.kind = kind; data.minimumRange = min; data.maximumRange = max; data.comboBreakRange = max;
        var motion = new BossMotion { animatorState = "Base Layer." + state, clip = clip };
        if (name == "MeleeCombo2") { motion.hitCenter.z = 2.3f; motion.hitHalfExtents.z = 1.7f; }
        if (kind == BossAttackKind.Charge) { motion.hitStart = 0f; motion.hitEnd = 1f; }
        data.motions = new[] { motion };
        if (combo != null)
        {
            data.motions = combo.Select((step, index) => new BossMotion
            {
                animatorState = "Base Layer." + state + "_" + (index + 1), clip = step,
                hitCenter = motion.hitCenter, hitHalfExtents = motion.hitHalfExtents
            }).ToArray();
        }
        if (kind == BossAttackKind.Ranged) data.projectilePrefab = projectile;
        AssetDatabase.CreateAsset(data, path);
        return data;
    }

    private static BossProjectile EnsureProjectile()
    {
        const string path = "Assets/04_Prefabs/Boss/BossProjectile.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<BossProjectile>(path);
        if (existing != null) return existing;
        const string materialPath = "Assets/04_Prefabs/Boss/BossProjectile.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.SetColor("_BaseColor", new Color(0.7f, 0.2f, 1f));
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(0.7f, 0.05f, 1f) * 2f);
            AssetDatabase.CreateAsset(material, materialPath);
        }
        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.name = "BossProjectile";
        go.layer = LayerMask.NameToLayer("Enemy");
        go.transform.localScale = Vector3.one * 0.36f;
        Object.DestroyImmediate(go.GetComponent<Collider>()); // Swept queries own collision and hit deduplication.
        go.GetComponent<Renderer>().sharedMaterial = material;
        go.AddComponent<BossProjectile>();
        var result = PrefabUtility.SaveAsPrefabAsset(go, path).GetComponent<BossProjectile>();
        Object.DestroyImmediate(go);
        return result;
    }

    public static BehaviorGraph EnsureGraph()
    {
        var graphType = TypeNamed("Unity.Behavior.BehaviorAuthoringGraph");
        var graph = AssetDatabase.LoadMainAssetAtPath(GraphPath) as ScriptableObject;
        if (graph != null) return (BehaviorGraph)Call(graph, "BuildRuntimeGraph", true);
        graph = ScriptableObject.CreateInstance(graphType);
        AssetDatabase.CreateAsset(graph, GraphPath);
        Call(graph, "ValidateAsset");
        var nodes = (IEnumerable)Get(graph, "Nodes");
        var root = nodes.Cast<object>().First(node => Get(node, "IsRoot") is bool isRoot && isRoot);
        Set(root, "Position", new Vector2(0, 0));
        var priority = Node(graph, typeof(BossPriority), root, null, 0, 150);
        Action(graph, BossTask.Die, priority, "Death", -800, 450);
        Action(graph, BossTask.WaitForTarget, priority, "NoTarget", -440, 450);
        var special = Action(graph, BossTask.CastSpecial, priority, "Special", 0, 450);
        var specialRecovery = Action(graph, BossTask.Recovery, special, null, 0, 620);
        Action(graph, BossTask.ScheduleNextAttack, specialRecovery, null, 0, 790);
        var normal = Node(graph, TypeNamed("Unity.Behavior.SequenceComposite"), priority, "Combat", 900, 450);
        var wait = Action(graph, BossTask.WaitForOpportunity, normal, null, 500, 650);
        Action(graph, BossTask.SelectAttack, wait, null, 500, 820);
        var branch = Node(graph, typeof(BossAttackBranch), normal, null, 900, 650);
        var recovery = Action(graph, BossTask.Recovery, normal, null, 1400, 650);
        Action(graph, BossTask.ScheduleNextAttack, recovery, null, 1400, 820);
        var combo = Node(graph, typeof(BossComboLoop), branch, "Melee", 600, 1000);
        var check = Action(graph, BossTask.CheckComboRange, combo, null, 600, 1160);
        var face = Action(graph, BossTask.FaceTarget, check, null, 600, 1320);
        var play = Action(graph, BossTask.PlayOneMotion, face, null, 600, 1480);
        var advance = Action(graph, BossTask.AdvanceMotion, play, null, 600, 1640);
        Action(graph, BossTask.ComboGap, advance, null, 600, 1800);
        foreach (string port in new[] { "Charge", "Ranged" })
        {
            float x = port == "Charge" ? 960 : 1320;
            var prepare = Action(graph, BossTask.PrepareAttack, branch, port, x, 1000);
            var aim = Action(graph, BossTask.FaceTarget, prepare, null, x, 1160);
            Action(graph, BossTask.PlayOneMotion, aim, null, x, 1320);
        }
        var blackboard = Get(graph, "Blackboard");
        var variables = (IList)Get(blackboard, "Variables");
        variables.Add(new TypedVariableModel<GameObject> { Name = "Target" });
        variables.Add(new TypedVariableModel<BossAttackData> { Name = "SelectedAttack" });
        variables.Add(new TypedVariableModel<int> { Name = "MotionIndex" });
        variables.Add(new TypedVariableModel<bool> { Name = "ComboCancelled" });
        variables.Add(new TypedVariableModel<float> { Name = "NextAttackTime" });
        variables.Add(new TypedVariableModel<bool> { Name = "SpecialPending" });
        variables.Add(new TypedVariableModel<bool> { Name = "SpecialUsed" });
        Call(graph, "ValidateAsset");
        var runtime = (BehaviorGraph)Call(graph, "BuildRuntimeGraph", true);
        EditorUtility.SetDirty(graph);
        AssetDatabase.SaveAssetIfDirty(graph);
        return runtime;
    }

    private static object Action(object graph, BossTask task, object parent, string port, float x, float y)
    {
        var node = Node(graph, typeof(BossAction), parent, port, x, y);
        var field = Call(node, "GetOrCreateField", "Task", typeof(BossTask));
        ((BlackboardVariable)Get(field, "LocalValue")).ObjectValue = task;
        return node;
    }

    private static object Node(object graph, Type runtimeType, object parent, string portName, float x, float y)
    {
        var registry = TypeNamed("Unity.Behavior.NodeRegistry");
        var info = registry.GetMethod("GetInfo", All, null, new[] { typeof(Type) }, null).Invoke(null, new object[] { runtimeType });
        if (info == null) throw new InvalidOperationException("Node not registered: " + runtimeType);
        Type modelType = (Type)Get(Get(info, "ModelType"), "Type");
        var ports = ((IEnumerable)Get(parent, "OutputPortModels")).Cast<object>();
        var port = portName == null ? ports.First() : ports.First(p => (string)Get(p, "Name") == portName);
        if ((bool)Get(port, "IsFloating"))
        {
            var connection = ((IEnumerable)Get(port, "Connections")).Cast<object>().First();
            var floating = Get(connection, "NodeModel");
            port = ((IEnumerable)Get(floating, "OutputPortModels")).Cast<object>().First();
        }
        return Call(graph, "CreateNode", modelType, new Vector2(x, y), port, new[] { info });
    }

    private static int EnsureBossAgent()
    {
        for (int i = 0; i < NavMesh.GetSettingsCount(); i++)
        {
            var settings = NavMesh.GetSettingsByIndex(i);
            if (NavMesh.GetSettingsNameFromID(settings.agentTypeID) == "Boss") return settings.agentTypeID;
        }
        var created = NavMesh.CreateSettings();
        var obj = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/NavMeshAreas.asset")[0]);
        var settingsList = obj.FindProperty("m_Settings");
        var names = obj.FindProperty("m_SettingNames");
        for (int i = 0; i < settingsList.arraySize; i++)
        {
            var settings = settingsList.GetArrayElementAtIndex(i);
            if (settings.FindPropertyRelative("agentTypeID").intValue != created.agentTypeID) continue;
            settings.FindPropertyRelative("agentRadius").floatValue = 1f;
            settings.FindPropertyRelative("agentHeight").floatValue = 3f;
            settings.FindPropertyRelative("agentClimb").floatValue = 0.4f;
            names.GetArrayElementAtIndex(i).stringValue = "Boss";
        }
        obj.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.SaveAssetIfDirty(obj.targetObject);
        return created.agentTypeID;
    }

    private static void BuildNavigation(int agentType)
    {
        var floor = GameObject.Find("GrassGround_20_20");
        if (floor == null) throw new InvalidOperationException("Boss arena ground not found.");
        var surface = GetOrAdd<NavMeshSurface>(floor);
        Undo.RecordObject(surface, "Configure Boss NavMesh");
        surface.agentTypeID = agentType;
        surface.collectObjects = CollectObjects.All;
        surface.layerMask = LayerMask.GetMask("Default");
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.BuildNavMesh();
        const string path = "Assets/06_Scenes/BossScene/NavMesh-BossArena.asset";
        var previous = AssetDatabase.LoadAssetAtPath<NavMeshData>(path);
        if (previous == null) AssetDatabase.CreateAsset(surface.navMeshData, path);
        else
        {
            var built = surface.navMeshData;
            surface.RemoveData();
            EditorUtility.CopySerialized(built, previous);
            surface.navMeshData = previous;
            surface.AddData();
            Object.DestroyImmediate(built);
            EditorUtility.SetDirty(previous);
        }
        AssetDatabase.SaveAssetIfDirty(surface.navMeshData);
        PrefabUtility.RecordPrefabInstancePropertyModifications(surface);
        EditorUtility.SetDirty(surface);
    }

    private static T GetOrAdd<T>(GameObject go) where T : Component
    {
        var component = go.GetComponent<T>();
        return component != null ? component : go.AddComponent<T>();
    }
    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int separator = path.LastIndexOf('/');
        EnsureFolder(path.Substring(0, separator));
        AssetDatabase.CreateFolder(path.Substring(0, separator), path.Substring(separator + 1));
    }
    private static Type TypeNamed(string name) => AppDomain.CurrentDomain.GetAssemblies().Select(a => a.GetType(name)).First(t => t != null);
    private static object Get(object obj, string name)
    {
        var type = obj.GetType();
        return type.GetProperty(name, All)?.GetValue(obj) ?? type.GetField(name, All)?.GetValue(obj);
    }
    private static void Set(object obj, string name, object value)
    {
        var type = obj.GetType();
        var property = type.GetProperty(name, All);
        if (property != null) property.SetValue(obj, value); else type.GetField(name, All).SetValue(obj, value);
    }
    private static object Call(object obj, string name, params object[] args)
    {
        var method = obj.GetType().GetMethods(All).First(m => m.Name == name && !m.IsGenericMethod && m.GetParameters().Length == args.Length);
        return method.Invoke(obj, args);
    }
}
