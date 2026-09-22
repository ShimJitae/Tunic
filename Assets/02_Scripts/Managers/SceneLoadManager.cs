using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
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
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    public void LoadScene(string sceneName)
    {
        if (isLoading)
            return;

        LoadSceneAsync(sceneName).Forget();
    }

    private async UniTask LoadSceneAsync(string sceneName)
    {
        isLoading = true;

        try
        {
            await fadeView.FadeInAsync();

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

            if (operation == null)
            {
                Debug.LogError($"{nameof(SceneLoadManager)}: " + $"{sceneName} 씬을 로드할 수 없습니다.", this);

                await fadeView.FadeOutAsync();

                return;
            }

            await operation.ToUniTask();

            await UniTask.NextFrame();

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
            player = FindFirstObjectByType<PlayerController>();
        }
    }

    private void MovePlayerToStartPoint()
    {
        GameObject startPoint = GameObject.Find(StartPointName);

        if (startPoint == null)
            return;


        PlayerMoveModule moveModule = player.GetComponent<PlayerMoveModule>();

        if (moveModule != null)
        {
            moveModule.Stop();
            moveModule.CancelDodge();
        }


        CharacterController characterController = player.GetComponent<CharacterController>();

        bool wasControllerEnabled = characterController != null && characterController.enabled;

        if (wasControllerEnabled)
            characterController.enabled = false;


        player.transform.SetPositionAndRotation(startPoint.transform.position, startPoint.transform.rotation);

        if (wasControllerEnabled)
            characterController.enabled = true;
    }

    private void SetCinemachineTarget()
    {
        CinemachineCamera cinemachineCamera =
            FindFirstObjectByType<CinemachineCamera>();

        if (cinemachineCamera == null)
            return;

        cinemachineCamera.Target.TrackingTarget =
            player.transform;
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}