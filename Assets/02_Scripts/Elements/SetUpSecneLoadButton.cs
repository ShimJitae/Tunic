using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SetUpSecneLoadButton : MonoBehaviour
{
    [SerializeField] private string sceneName;

    private void Start()
    {
        SceneLoadManager sceneLoadManager = SceneLoadManager.Instance;
        if (sceneName == "Quit")
        {
            GetComponent<Button>().onClick.AddListener(() => sceneLoadManager.Quit());
        }
        else
        {
            GetComponent<Button>().onClick.AddListener(() => sceneLoadManager.LoadScene(sceneName));
        }
    }
}
