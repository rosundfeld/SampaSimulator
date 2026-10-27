using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    private static Scene targetSceneIndex;
    // Enum names must match the scene names in the Build Profile scene list.
    public enum Scene
    {
        MainMenuScene,
        LoadingScene,
        GameScene,
    }

    public static void Load(Scene targetScene)
    {
        Loader.targetSceneIndex = targetScene;
        Time.timeScale = 1f;
        SceneManager.LoadScene(Scene.LoadingScene.ToString());
    }

    public static void LoaderCallback()
    {
        SceneManager.LoadScene(targetSceneIndex.ToString());
    }
}
