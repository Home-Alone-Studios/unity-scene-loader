// ---------------------------------------------------------------------------- 
// Author: Ryan Hipple
// Date:   05/07/2018
// ----------------------------------------------------------------------------

using System;
using UnityEngine;

/// <summary>
/// Class used to serialize a reference to a scene asset that can be used
/// at runtime in a build, when the asset can no longer be directly
/// referenced. This caches the scene name based on the SceneAsset to use
/// at runtime to load.
/// </summary>
[Serializable]
public class SerializableScene : ISerializationCallbackReceiver
{
    /// <summary>
    /// Exception that is raised when there is an issue resolving and
    /// loading a scene reference.
    /// </summary>
    public class SceneLoadException : Exception
    {
        public SceneLoadException(string message) : base(message)
        {}
    }
    
#if UNITY_EDITOR
    public UnityEditor.SceneAsset Scene;
#endif

    [Tooltip("The name of the referenced scene. This may be used at runtime to load the scene.")]
    public string SceneName;

    [SerializeField]
    private int _sceneIndexInBuildSettings = -1;

    [SerializeField]
    private bool _isSceneEnabledInBuildSettings;

    private void ValidateScene()
    {
        if (string.IsNullOrEmpty(SceneName))
            throw new SceneLoadException("No scene specified.");
        
        if (_sceneIndexInBuildSettings < 0)
            throw new SceneLoadException($"Scene {SceneName} is not in the build settings");
        
        if (!_isSceneEnabledInBuildSettings)
            throw new SceneLoadException($"Scene {SceneName} is not enabled in the build settings");
    }

    public bool IsValid()
    {
        if (string.IsNullOrEmpty(SceneName)) return false;

        if (_sceneIndexInBuildSettings < 0) return false;

        if (!_isSceneEnabledInBuildSettings) return false;

        return true;
    }
    
    /// <summary>
    /// Makes it work with the existing Unity methods (LoadLevel/LoadScene)
    /// </summary>
    public static implicit operator string(SerializableScene serializableScene) 
    {
#if UNITY_EDITOR
        return System.IO.Path.GetFileNameWithoutExtension(UnityEditor.AssetDatabase.GetAssetPath(serializableScene.Scene));
#else
        return serializableScene.SceneName;
#endif
    }

    public void OnBeforeSerialize()
    {
#if UNITY_EDITOR
        if (Scene != null)
        {
            string sceneAssetPath = UnityEditor.AssetDatabase.GetAssetPath(Scene);
            string sceneAssetGUID = UnityEditor.AssetDatabase.AssetPathToGUID(sceneAssetPath);
            
            UnityEditor.EditorBuildSettingsScene[] scenes = 
                UnityEditor.EditorBuildSettings.scenes;

            _sceneIndexInBuildSettings = -1;
            for (int i = 0; i < scenes.Length; i++)
            {
                if (scenes[i].guid.ToString() == sceneAssetGUID)
                {
                    _sceneIndexInBuildSettings = i;
                    _isSceneEnabledInBuildSettings = scenes[i].enabled;
                    if (scenes[i].enabled)
                        SceneName = Scene.name;
                    break;
                }
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(SceneName))
            {
                SceneName = string.Empty;
            }
        }
#endif
    }

    public void OnAfterDeserialize() {}
}