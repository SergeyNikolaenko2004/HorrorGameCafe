using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Интерфейсы для различных действий меню
public interface IMenuAction
{
    void Execute();
}

public interface ISceneLoader
{
    void LoadScene(string sceneName);
}

public interface IApplicationManager
{
    void QuitApplication();
}

public interface IURLHandler
{
    void OpenURL(string url);
}

// Реализации интерфейсов
public class SceneLoader : ISceneLoader
{
    public void LoadScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("Scene name is null or empty");
            return;
        }

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' cannot be loaded. Make sure it's added in Build Settings.");
        }
    }
}

public class ApplicationManager : IApplicationManager
{
    public void QuitApplication()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}

public class URLHandler : IURLHandler
{
    public void OpenURL(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            Debug.LogError("URL is null or empty");
            return;
        }

        if (url.StartsWith("http://") || url.StartsWith("https://"))
        {
            Application.OpenURL(url);
        }
        else
        {
            Debug.LogError($"Invalid URL format: {url}");
        }
    }
}

// Конкретные действия меню
public class ExitGameAction : IMenuAction
{
    private readonly IApplicationManager _appManager;

    public ExitGameAction(IApplicationManager appManager)
    {
        _appManager = appManager;
    }

    public void Execute()
    {
        _appManager.QuitApplication();
    }
}

public class LoadSceneAction : IMenuAction
{
    private readonly ISceneLoader _sceneLoader;
    private readonly string _sceneName;

    public LoadSceneAction(ISceneLoader sceneLoader, string sceneName)
    {
        _sceneLoader = sceneLoader;
        _sceneName = sceneName;
    }

    public void Execute()
    {
        _sceneLoader.LoadScene(_sceneName);
    }
}

public class OpenURLAction : IMenuAction
{
    private readonly IURLHandler _urlHandler;
    private readonly string _url;

    public OpenURLAction(IURLHandler urlHandler, string url)
    {
        _urlHandler = urlHandler;
        _url = url;
    }

    public void Execute()
    {
        _urlHandler.OpenURL(_url);
    }
}

// Настройки меню
[System.Serializable]
public class MenuSettings
{
    [field: SerializeField] public string GameSceneName { get; private set; } = "CoffeeShopInteriorNIGHT";
    [field: SerializeField] public string GitHubURL { get; private set; } = "https://github.com/SergeyNikolaenko2004/HorrorGameCafe.git";
}

// Основной контроллер меню
public class MainMenuController : MonoBehaviour
{
    [Header("Настройки меню")]
    [SerializeField] private MenuSettings settings = new MenuSettings();

    [Header("Ссылки на кнопки")]
    [SerializeField] private Button exitButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button githubButton;

    private IMenuAction _exitAction;
    private IMenuAction _startGameAction;
    private IMenuAction _githubAction;

    private void Awake()
    {
        InitializeDependencies();
        ValidateComponents();
    }

    private void Start()
    {
        SetupButtonListeners();
    }

    private void InitializeDependencies()
    {
        var sceneLoader = new SceneLoader();
        var appManager = new ApplicationManager();
        var urlHandler = new URLHandler();

        _exitAction = new ExitGameAction(appManager);
        _startGameAction = new LoadSceneAction(sceneLoader, settings.GameSceneName);
        _githubAction = new OpenURLAction(urlHandler, settings.GitHubURL);
    }

    private void ValidateComponents()
    {
        if (exitButton == null)
            Debug.LogError("Exit Button is not assigned in MainMenuController");

        if (startGameButton == null)
            Debug.LogError("Start Game Button is not assigned in MainMenuController");

        if (githubButton == null)
            Debug.LogError("GitHub Button is not assigned in MainMenuController");
    }

    private void SetupButtonListeners()
    {
        // Выход из игры
        exitButton?.onClick.AddListener(() => _exitAction?.Execute());

        // Начало игры
        startGameButton?.onClick.AddListener(() => _startGameAction?.Execute());

        // Открытие GitHub
        githubButton?.onClick.AddListener(() => _githubAction?.Execute());
    }

    private void OnDestroy()
    {
        CleanupButtonListeners();
    }

    private void CleanupButtonListeners()
    {
        exitButton?.onClick.RemoveAllListeners();
        startGameButton?.onClick.RemoveAllListeners();
        githubButton?.onClick.RemoveAllListeners();
    }

    // Методы для тестирования и кастомной настройки
    public void SetExitAction(IMenuAction action) => _exitAction = action;
    public void SetStartGameAction(IMenuAction action) => _startGameAction = action;
    public void SetGitHubAction(IMenuAction action) => _githubAction = action;
}