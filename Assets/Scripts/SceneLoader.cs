using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    [SerializeField] SerializableScene _serializableScene;
    [SerializeField] LoadSceneMode _loadSceneMode;
    [SerializeField] bool _useSceneManager;
    [SerializeField] UnityEvent _onLoadComplete;

    private AsyncOperation _asyncOperation = null;
    private bool _isListeningForCompletedEvent = false;

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
            if(_asyncOperation == null)
            {  
                await LoadAsync(_loadSceneMode);
            }
            else
            {
                if(_asyncOperation.progress >= 0.9f)
                {
                    _asyncOperation.allowSceneActivation = true;
                    DisableOnCompletedListener();
                }
            }
        }
    }

    public async void PreLoadAsync()
    {
        if(_asyncOperation == null)
        {
            await LoadAsync(_loadSceneMode, false);
        }
    }

    public void RestartCurrentScene()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadScene(string sceneName, LoadSceneMode loadScenMode = LoadSceneMode.Single)
    {
        SceneManager.LoadScene(sceneName, loadScenMode);
    }

    public async Task<AsyncOperation> LoadSceneAsync(string sceneName, LoadSceneMode loadScenMode = LoadSceneMode.Single, bool allowSceneActivation = true)
    {
        DisableOnCompletedListener();
        // Begin to load the Scene you have specified.
        _asyncOperation = SceneManager.LoadSceneAsync(sceneName, loadScenMode);
        // Decide whether to let the scene activate until it loaded.
        _asyncOperation.allowSceneActivation = allowSceneActivation;
        _asyncOperation.completed += OnLoadCompleted;
        return _asyncOperation;
    }

    void OnLoadCompleted(AsyncOperation asyncOperation)
    {
        _asyncOperation = null;
        if(_onLoadComplete != null)
        {
            _onLoadComplete.Invoke();
        }
    }

    private void DisableOnCompletedListener() 
    {
        if(_isListeningForCompletedEvent)
        {   
            if(_asyncOperation != null)
            {
                _asyncOperation.completed -= OnLoadCompleted;
            }
            _isListeningForCompletedEvent = false;
        }  
    }

    public void Unload()
    {
        UnloadSceneAsync(_serializableScene.SceneName);
    }

    public void UnloadSceneAsync(string sceneName)
    {
        SceneManager.UnloadSceneAsync(sceneName, UnloadSceneOptions.None);
    }

    void OnDestroy()
    {
        DisableOnCompletedListener();
    }

    //public IEnumerator _TestLoadSceneAsync(LoadSceneMode mode = LoadSceneMode.Single)
    //{
    //    yield return null;

    //    AsyncOperation asyncOp = SceneManager.LoadSceneAsync(SceneName, mode);

    //    yield return new WaitForSeconds(5f);

    //    yield return asyncOp;
    //}
}