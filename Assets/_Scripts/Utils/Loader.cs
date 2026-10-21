using UnityEngine;
using UnityEngine.SceneManagement;

public static class Loader
{
    // Enum names must match the scene names in the Build Profile scene list.
    public enum Scene
    {
        MainMenu,
        Game,
    }

    public static void Load(Scene targetScene)
    {
        // Sair do jogo pelo menu de pause deixaria o timeScale em 0 na próxima cena.
        Time.timeScale = 1f;
        SceneManager.LoadScene(targetScene.ToString());
    }
}
