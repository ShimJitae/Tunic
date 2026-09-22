using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
    private const string BossSceneName = "BossScene";
    private const string StartPointName = "StartPoint";
    private const string TitleSceneName = "TitleScene";

    private static SceneLoadManager instance;
    public static SceneLoadManager Instance => instance;

    [SerializeField] private PlayerController player;
    [SerializeField] private SceneFadeView fadeView;

    private bool isLoading;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
<<<<<<< Updated upstream

        if (player == null)
            player = FindFirstObjectByType<PlayerController>();

        SceneManager.sceneLoaded += HandleSceneLoaded;
=======
>>>>>>> Stashed changes
    }

    private void OnDestroy()
    {
        if (instance != this)
            return;
        instance = null;
    }

    public void LoadBossScene()
    {
<<<<<<< Updated upstream
        if (!TryCachePlayer())
            return;

        SceneManager.LoadScene(BossSceneName);
=======
        if (isLoading)
            return;

        LoadSceneAsync(sceneName).Forget();
>>>>>>> Stashed changes
    }

    private async UniTask LoadSceneAsync(string sceneName)
    {
<<<<<<< Updated upstream
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
=======
        isLoading = true;

        try
        {
            await fadeView.FadeInAsync(); // 현재 화면을 검게 덮음

            AsyncOperation operation = SceneManager.LoadSceneAsync(
                sceneName,
                LoadSceneMode.Single);

            if (operation == null)
            {
                Debug.LogError(
                    $"{nameof(SceneLoadManager)}: " +
                    $"{sceneName} 씬을 로드할 수 없습니다.",
                    this);

                await fadeView.FadeOutAsync();

                return;
            }

            await operation.ToUniTask(); // Scene 로드가 끝날 때까지 대기
            await UniTask.NextFrame(); // 새 Scene의 초기화를 위해 한 프레임 대기

            Scene loadedScene = SceneManager.GetActiveScene();

            SetUpPlayer(loadedScene);

            if (player != null)
            {
                MovePlayerToStartPoint();
                SetCinemachineTarget();
            }

            await fadeView.FadeOutAsync();
        }
        finally
        {
            isLoading = false;
        }
    }

    private void SetUpPlayer(Scene scene)
    {
        if (scene.name == TitleSceneName)
        {
            if (player != null)
            {
                Destroy(player.gameObject);
                player = null;
            }

            return;
        }

        if (player == null)
        {
            player =
                FindFirstObjectByType<PlayerController>();
        }
>>>>>>> Stashed changes
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