using UnityEngine;

/// <summary>
/// Entry point for the game. Ensures core systems are initialised before any
/// other scene logic runs. Place this on a GameObject in the first boot scene.
/// </summary>
public class BootStrapper : MonoBehaviour
{
    [SerializeField] private GameManager gameManagerPrefab;
    [SerializeField] private SceneLoader sceneLoaderPrefab;
    [SerializeField] private AudioManager audioManagerPrefab;

    [SerializeField] private string firstScene = "MainMenu";

    private void Awake()
    {
        InitialiseCoreSystem(gameManagerPrefab);
        InitialiseCoreSystem(sceneLoaderPrefab);
        InitialiseCoreSystem(audioManagerPrefab);
    }

    private void Start()
    {
        SceneLoader.Instance.LoadScene(firstScene);
    }

    private void InitialiseCoreSystem<T>(T prefab) where T : MonoBehaviour
    {
        if (prefab != null && FindObjectOfType<T>() == null)
            Instantiate(prefab);
    }
}
