using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : Singleton<SceneLoader>
{
    public bool IsLoading { get; private set; }

    public void LoadScene(string sceneName)
    {
        if (!IsLoading)
            StartCoroutine(LoadSceneAsync(sceneName));
    }

    public void LoadScene(int sceneIndex)
    {
        if (!IsLoading)
            StartCoroutine(LoadSceneAsync(sceneIndex));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        IsLoading = true;
        EventBus.Publish(new SceneLoadStartedEvent(sceneName));

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        while (!operation.isDone)
            yield return null;

        IsLoading = false;
        EventBus.Publish(new SceneLoadCompletedEvent(sceneName));
    }

    private IEnumerator LoadSceneAsync(int sceneIndex)
    {
        string sceneName = SceneUtility.GetScenePathByBuildIndex(sceneIndex);
        yield return LoadSceneAsync(sceneName);
    }
}

public struct SceneLoadStartedEvent
{
    public string SceneName;
    public SceneLoadStartedEvent(string name) => SceneName = name;
}

public struct SceneLoadCompletedEvent
{
    public string SceneName;
    public SceneLoadCompletedEvent(string name) => SceneName = name;
}
