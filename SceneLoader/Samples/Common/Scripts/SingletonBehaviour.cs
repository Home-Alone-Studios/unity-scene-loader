using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Singleton is basically an object that can only and always have one instance. 
/// The script therefore returns the first identified instance of a component in the scene. 
/// You get slapped with an exception if none exists
/// </summary>
public class SingletonBehaviour : MonoBehaviour
{
    const string PERSISTENCE_TOOL_TIP = "Note: The instance may get destroyed if it is not at the root of the scene hierarchy or is a child to a parent gameobject that may be destroyed";

    [SerializeField] Component _component;

    /// <summary>
    /// Flag to determine whether or not to destroy the instance when switching or loading scenes.
    /// <i>.
    /// </summary>
    [SerializeField, Tooltip(PERSISTENCE_TOOL_TIP)] bool _isPersistent;

    public static bool IsAlive
    {
        get
        {
            return _isAlive;
        }
    }

    public bool HasInstance
    {
        get
        {
            if (_instances == null || _instances.Count == 0) return false;

            if (_instances.ContainsKey(ComponentTypeName))
            {
                return true;
            }

            return false;
        }
    }

    public bool IsPersistent
    {
        get
        {
            return _isPersistent;
        }

        set
        {
            if (value)
            {
                EnablePersistence();
            }
            else
            {
                DisablePersistence();
            }

            _isPersistent = value;
        }
    }

    string ComponentTypeName => _component == null ? "" : _component.GetType().AssemblyQualifiedName;


    static bool _isAlive = true;
    static object _key = new object();
    static bool _savedPersistence;
    static Dictionary<string, Component> _instances = new Dictionary<string, Component>();


    static SingletonBehaviour()
    {
        Debug.Log("After Scene is loaded and game is running");
        _instances = new Dictionary<string, Component>();
    }

    #if UNITY_EDITOR
    public void OnValidate()
    {
        if (_isPersistent != _savedPersistence)
        {
            _savedPersistence = _isPersistent;

            if (Application.isPlaying)
            {
                IsPersistent = _savedPersistence;
            }
        }
    }
    #endif

    /// <summary>
    /// Method that initialises the singleton. If you need the Awake call, override OnAwake instead
    /// </summary>
    void Awake()
    {
        Debug.Log(ComponentTypeName);

        // check if there's already an instance of the object in the scene
        if (!HasInstance)
        {
            _instances.Add(ComponentTypeName, _component);

            if (_isPersistent)
            {
                EnablePersistence();
            }
        }
        // otherwise, remove any duplicates
        else
        {
            RemoveDuplicate();
        }
    }

    void RemoveDuplicate()
    {
        Debug.Log($"{nameof(RemoveDuplicate)}({gameObject.name})");

        if (HasInstance)
        {
            Destroy(this.gameObject);
        }
    }

    void OnDestroy()
    {
        if (HasInstance && GetInstance(_component) == _component)
        {
            _instances.Remove(ComponentTypeName);
        }
    }

    protected virtual void OnApplicationExit()
    {
        _isAlive = true;
        _instances = new Dictionary<string, Component>();
    }

    void EnablePersistence()
    {
        if (Application.isPlaying)
        {
            DontDestroyOnLoad(this.gameObject);
        }
    }

    void DisablePersistence()
    {
        Debug.Log($"{nameof(DisablePersistence)}({ (transform.parent == null ? "null" : transform.parent.name) })");

        Scene scene = SceneManager.GetActiveScene();
        if (scene != null && scene.name != gameObject.scene.name)
        {
            SceneManager.MoveGameObjectToScene(gameObject, scene);
        }
    }

    public static Component GetInstance(Component component)
    {
        if (_instances == null || _instances.Count == 0) return null;

        if (!_instances.ContainsKey(component.GetType().AssemblyQualifiedName)) return null;

        return _instances[component.GetType().AssemblyQualifiedName]; 
    }
}