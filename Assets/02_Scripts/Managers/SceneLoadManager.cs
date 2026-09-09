using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoadManager : MonoBehaviour
{
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

        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (instance != this)
            return;

        SceneManager.sceneLoaded -= HandleSceneLoaded;
        instance = null;
    }

    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    private void HandleSceneLoaded(
        Scene scene,
        LoadSceneMode loadSceneMode)
    {
        SetUpPlayer();
        MovePlayerToStartPoint();
        SetCinemachineTarget();
    }

    private void SetUpPlayer()
    {
        if (SceneManager.GetActiveScene().name == "TitleScene" && player != null)
        {
            GameObject.Destroy(player.gameObject);
            return;
        }

        if (player == null)
            player = FindFirstObjectByType<PlayerController>();
    }

    private void MovePlayerToStartPoint()
    {
        GameObject startPoint = GameObject.Find(StartPointName);

        if (startPoint == null)
        {
            // Debug.LogError(
            //     $"{nameof(SceneLoadManager)}: " +
            //     $"{SceneManager.GetActiveScene().name}에서 {StartPointName}를 찾지 못했습니다.",
            //     this);

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
            // Debug.LogError(
            //     $"{nameof(SceneLoadManager)}: " +
            //     "CinemachineCamera를 찾지 못했습니다.",
            //     this);

            return;
        }

        cinemachineCamera.Target.TrackingTarget = player.transform;
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