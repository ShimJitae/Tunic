using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    private const string BossSceneName = "BossScene";
    private const string StartPointName = "StartPoint";

    private static SceneLoadManager instance;
    public static SceneLoadManager Instance => instance;

    [SerializeField] private PlayerController player;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);

        if (player == null)
            player = FindFirstObjectByType<PlayerController>();

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance != this)
            return;

        SceneManager.sceneLoaded -= HandleSceneLoaded;
        instance = null;
    }

    public void LoadBossScene()
    {
        if (!TryCachePlayer())
            return;

        SceneManager.LoadScene(BossSceneName);
    }

    private void HandleSceneLoaded(
        Scene scene,
        LoadSceneMode loadSceneMode)
    {
        if (scene.name != BossSceneName)
            return;

        MovePlayerToStartPoint();
        SetCinemachineTarget();
    }

    private bool TryCachePlayer()
    {
        if (player != null)
            return true;

        player = FindFirstObjectByType<PlayerController>();

        if (player != null)
            return true;

        Debug.LogError(
            $"{nameof(SceneLoadManager)}: PlayerController를 찾지 못했습니다.",
            this);

        return false;
    }

    private void MovePlayerToStartPoint()
    {
        GameObject startPoint = GameObject.Find(StartPointName);

        if (startPoint == null)
        {
            Debug.LogError(
                $"{nameof(SceneLoadManager)}: " +
                $"{BossSceneName}에서 {StartPointName}를 찾지 못했습니다.",
                this);

            return;
        }

        PlayerMoveModule moveModule =
            player.GetComponent<PlayerMoveModule>();

        if (moveModule != null)
        {
            moveModule.Stop();
            moveModule.CancelDodge();
        }

        CharacterController characterController =
            player.GetComponent<CharacterController>();

        bool wasControllerEnabled =
            characterController != null && characterController.enabled;

        if (wasControllerEnabled)
            characterController.enabled = false;

        player.transform.SetPositionAndRotation(
            startPoint.transform.position,
            startPoint.transform.rotation);

        if (wasControllerEnabled)
            characterController.enabled = true;
    }

    private void SetCinemachineTarget()
    {
        CinemachineCamera cinemachineCamera =
            FindFirstObjectByType<CinemachineCamera>();

        if (cinemachineCamera == null)
        {
            Debug.LogError(
                $"{nameof(SceneLoadManager)}: " +
                "CinemachineCamera를 찾지 못했습니다.",
                this);

            return;
        }

        cinemachineCamera.Target.TrackingTarget = player.transform;
    }
}