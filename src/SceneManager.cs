namespace _2d_td;

public static class SceneManager
{
    public enum Scene
    {
        Menu,
        Game
    }

    public delegate void SceneLoadedHandler(Scene loadedScene);
    public static event SceneLoadedHandler SceneLoaded;

    public static Scene CurrentScene { get; private set; } = Scene.Menu;

    public static void LoadMainMenu()
    {
        CurrentScene = Scene.Menu;
        OnSceneLoaded(CurrentScene);
    }

    public static void LoadGame()
    {
        CurrentScene = Scene.Game;
        OnSceneLoaded(CurrentScene);
    }

    private static void OnSceneLoaded(Scene loadedScene)
    {
        SceneLoaded?.Invoke(loadedScene);
    }
}
