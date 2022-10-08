using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] SerializableScene _serializableScene;
    [SerializeField] bool _preloadOnStart;
    [SerializeField] LoadSceneMode _loadSceneMode;
    [SerializeField] bool _useSceneManager;

    [Header("Events")]
    [SerializeField] UnityEvent _onSceneLoaded;
    [SerializeField] UnityEvent _onSceneActivated;
    [SerializeField] UnityEvent _onSceneUnloaded;

    [SerializeField] private AsyncOperation _asyncLoadOperation = null;
    private AsyncOperation _asyncUnloadOperation = null;
    private bool _isListeningForLoadCompletedEvent = false;
    private bool _isListeningForUnloadCompletedEvent = false;

    private WaitForEndOfFrame _waitforEndOfFrame = new WaitForEndOfFrame();



    IEnumerator Start()
    {
        yield return _waitforEndOfFrame;

        if (_preloadOnStart)
        {
            PreLoadAsync();
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneUnloaded += OnSceneUnloaded;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveStateChaned;
    }

    private void OnDisable()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveStateChaned;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log($"Scene loaded: {scene.name}, {(LoadSceneMode)mode}");

        if (scene == null) return;

        if (scene.name == _serializableScene.SceneName)
        {
            _onSceneLoaded?.Invoke();
        }
    }

    void OnSceneUnloaded(Scene scene)
    {
        if (scene == null) return;

        //Debug.Log($"Scene unloaded: {scene.name}");

        if (scene.name == _serializableScene.SceneName)
        {
            _onSceneUnloaded?.Invoke();
        }
    }

    void OnActiveStateChaned(Scene oldScene, Scene newScene)
    {
        //Debug.Log($"Scene changed from: {oldScene.name} to {newScene.name}");
    }

    void Load(LoadSceneMode loadScenMode)
    {
        if (_serializableScene != null && _serializableScene.IsValid())
        {
            LoadScene(_serializableScene.SceneName, loadScenMode);
        }
    }

    Task<AsyncOperation> LoadAsync(LoadSceneMode loadScenMode, bool allowSceneActivation = true)
    {
        if (_serializableScene != null && _serializableScene.IsValid())
        {
            return LoadSceneAsync(_serializableScene.SceneName, loadScenMode, allowSceneActivation);
        }

        return null;
    }

    Scene GetSceneIfActive()
    { 
        Scene scene = new Scene();

        if (_serializableScene != null)
        {
            scene = SceneManager.GetSceneByName(_serializableScene.SceneName);
        }

        return scene;
    }

    public void SetActive()
    { 
        Scene scene = GetSceneIfActive();
        if (!scene.IsValid())
        {
            throw new Exception($"Scene is invalid!");
        }

        SceneManager.SetActiveScene(scene);
    }

    public void Load()
    {
        Load(_loadSceneMode);
    }

    public async void LoadAsync()
    {
        if (_useSceneManager)
        {
            await SceneLoaderManager.Instance.LoadAsync(_serializableScene.SceneName);
        }
        else
        {
            Debug.Log($"Async Load Operation is null = {_asyncLoadOperation == null}");
            if(_asyncLoadOperation == null)
            {  
                await LoadAsync(_loadSceneMode);
            }
            else
            {
                if(_asyncLoadOperation.progress >= 0.9f)
                {
                    _asyncLoadOperation.allowSceneActivation = true;
                    DisableOnAsyncLoadCompletedListener();
                }
            }
        }
    }

    public async void PreLoadAsync()
    {
        if(_asyncLoadOperation == null)
        {
            await LoadAsync(_loadSceneMode, false);
        }
    }

    public void RestartCurrentScene()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    public void UnloadCurrentScene()
    {
        SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().name);
    }

    public void LoadScene(string sceneName, LoadSceneMode loadScenMode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(sceneName, loadScenMode);
    }

    public async Task<AsyncOperation> LoadSceneAsync(string sceneName, LoadSceneMode loadScenMode = LoadSceneMode.Single, bool allowSceneActivation = true)
    {
        DisableOnAsyncLoadCompletedListener();
        // Begin to load the Scene you have specified.
        _asyncLoadOperation = SceneManager.LoadSceneAsync(sceneName, loadScenMode);
        // Decide whether to let the scene activate until it loaded.
        _asyncLoadOperation.allowSceneActivation = allowSceneActivation;
        _asyncLoadOperation.completed += OnLoadCompleted;
        _isListeningForLoadCompletedEvent = true;
        return _asyncLoadOperation;
    }

    void OnLoadCompleted(AsyncOperation asyncOperation)
    {
        _asyncLoadOperation = null;
        if(_onSceneLoaded != null)
        {
            _onSceneLoaded.Invoke();
        }
    }

    private void DisableOnAsyncLoadCompletedListener() 
    {
        if(_isListeningForLoadCompletedEvent)
        {   
            if(_asyncLoadOperation != null)
            {
                _asyncLoadOperation.completed -= OnLoadCompleted;
                _asyncLoadOperation = null;
            }
            _isListeningForLoadCompletedEvent = false;
        }  
    }

    public void Unload()
    {
        SceneManager.UnloadSceneAsync(_serializableScene.SceneName);
    }

    public async void UnloadAsync()
    {
        if (_asyncUnloadOperation == null)
        {
            await UnloadSceneAsync(_serializableScene.SceneName);
        }
    }

    public async Task<AsyncOperation> UnloadSceneAsync(string sceneName)
    {
        DisableOnAsyncUnloadCompletedListener();
        _asyncUnloadOperation = SceneManager.UnloadSceneAsync(sceneName, UnloadSceneOptions.None);
        _asyncUnloadOperation.completed += OnAsyncUnloadCompleted;
        _isListeningForUnloadCompletedEvent = true;
        return _asyncUnloadOperation;
    }

    void OnAsyncUnloadCompleted(AsyncOperation asyncOperation)
    {
        _asyncUnloadOperation = null;
        if (_onSceneUnloaded != null)
        {
            _onSceneUnloaded.Invoke();
        }
    }

    private void DisableOnAsyncUnloadCompletedListener()
    {
        if (_isListeningForUnloadCompletedEvent)
        {
            if (_asyncUnloadOperation != null)
            {
                _asyncUnloadOperation.completed -= OnAsyncUnloadCompleted;
                _asyncUnloadOperation = null;
            }
            _isListeningForUnloadCompletedEvent = false;
        }
    }

    void OnDestroy()
    {
        DisableOnAsyncLoadCompletedListener();
        DisableOnAsyncUnloadCompletedListener();
    }

    //public IEnumerator _TestLoadSceneAsync(LoadSceneMode mode = LoadSceneMode.Single)
    //{
    //    yield return null;

    //    AsyncOperation asyncOp = SceneManager.LoadSceneAsync(SceneName, mode);

    //    yield return new WaitForSeconds(5f);

    //    yield return asyncOp;
    //}
}