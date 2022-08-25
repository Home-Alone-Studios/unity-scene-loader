using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class SceneLoaderManager : MonoBehaviour
{
    public static SceneLoaderManager Instance = null;

    [SerializeField] GameObject _preloader;
    [SerializeField] float _delay = 0f;
    [SerializeField, Range(0, 100)] float _loadingProgress = 0;
    [SerializeField] UnityEvent _onLoadStart;
    [SerializeField] UnityEvent<float> _onLoadProgress;
    [SerializeField] UnityEvent _onLoadComplete;

    private float _totalLoadProgress;
    private float _normalisedLoadProgress;
    private List<AsyncOperation> _scenesToLoad = new List<AsyncOperation>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        { 
            Destroy(gameObject);
        }

        SetPreloaderActive(false);
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
    }

    void OnSceneUnloaded(Scene scene)
    {
        //Debug.Log($"Scene unloaded: {scene.name}");
    }

    void OnActiveStateChaned(Scene oldScene, Scene newScene)
    {
        //Debug.Log($"Scene changed from: {oldScene.name} to {newScene.name}");
    }

    public async Task LoadAsync(string sceneName)
    {
        //SetLoadStatus(true);
        //await LoadSceneAsyncTask(sceneName);
        //SetLoadStatus(false);

        await LoadAsync(new string[] { sceneName });
    }

    async Task LoadSceneAsyncTask(string sceneName)
    {
        SetLoadStatus(true);
        var loading = SceneManager.LoadSceneAsync(sceneName);
        loading.allowSceneActivation = false;

        do
        {
            await Task.Delay(100);
            _totalLoadProgress = loading.progress;
        } while (loading.progress < 0.9f);

        await Task.Delay(TimeSpan.FromSeconds(1));

        loading.allowSceneActivation = true;

        await Task.Delay(TimeSpan.FromSeconds(_delay));

        SetLoadStatus(false);
    }

    public async Task LoadAsync(string[] sceneNames)
    {
        SetLoadStatus(true);

        foreach (string sceneName in sceneNames)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName);
            operation.allowSceneActivation = false;
            _scenesToLoad.Add(operation);
        }

        do
        {
            _totalLoadProgress = 0;
            foreach (AsyncOperation operation in _scenesToLoad)
            {
                await Task.Delay(100);
                _totalLoadProgress += operation.progress;
            }

            _normalisedLoadProgress = _totalLoadProgress / _scenesToLoad.Count;
            _loadingProgress = _normalisedLoadProgress * 100f;

            if (_onLoadProgress != null)
            {
                _onLoadProgress.Invoke(_normalisedLoadProgress);
            }
        } while (_normalisedLoadProgress < 0.9f);
        
        await Task.Delay(1000);

        foreach (AsyncOperation operation in _scenesToLoad)
        {
            operation.allowSceneActivation = true;
        }

        await Task.Delay(1000);

        await Task.Delay(TimeSpan.FromSeconds(_delay));

        SetLoadStatus(false);
    }

    bool IsLoadedAllScenes(string[] array)
    {
        int expectedToLoadScenes = array.Length;
        int loadedScenes = _scenesToLoad.FindAll(x => x.isDone).Count;
        Debug.Log($"{loadedScenes} - {expectedToLoadScenes}");
        return loadedScenes == expectedToLoadScenes;
    }

    void SetLoadStatus(bool flag)
    {
        if (flag)
        {
            _loadingProgress = _normalisedLoadProgress = _totalLoadProgress = 0;
            _scenesToLoad.Clear();
        }

        SetPreloaderActive(flag);

        if (flag)
        {
            if (_onLoadStart != null)
            {
                _onLoadStart.Invoke();
            }
        }
        else
        {
            if (_onLoadComplete != null)
            {
                _onLoadComplete.Invoke();
            }
        }
    }

    void SetPreloaderActive(bool flag)
    {
        if (_preloader != null)
        {
            _preloader.SetActive(flag);
        }
    }

     private void LateUpdate()
    {
        //_loadingProgress = Mathf.MoveTowards(_loadingProgress, _totalLoadingProgress, 3 * Time.deltaTime);
    }

    private void OnDestroy()
    {
        if (this == Instance)
        {
            Instance = null;
        }
    }
}