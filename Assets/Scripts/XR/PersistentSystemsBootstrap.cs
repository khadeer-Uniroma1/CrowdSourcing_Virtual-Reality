/*

using UnityEngine;
using UnityEngine.SceneManagement;

namespace VRCrowdSourcing.XR
{
    public static class PersistentSystemsBootstrap
    {
        private const string PersistentSystemsSceneName = "PersistentSystems";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void EnsurePersistentSystemsLoaded()
        {
            Scene persistentSystemsScene = SceneManager.GetSceneByName(PersistentSystemsSceneName);

            if (persistentSystemsScene.IsValid() && persistentSystemsScene.isLoaded)
            {
                return;
            }

            SceneManager.LoadSceneAsync(PersistentSystemsSceneName, LoadSceneMode.Additive);
        }
    }
}

*/