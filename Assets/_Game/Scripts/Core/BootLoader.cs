using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DirtDash.Core
{
    /// <summary>
    /// Initial startup coordinator for Dirt Dash.
    /// Bootstraps core singleton services (GameManager, AudioManager, SaveSystem)
    /// and smoothly transitions to the Main Menu.
    /// </summary>
    public class BootLoader : MonoBehaviour
    {
        [SerializeField] private float splashDuration = 0.5f;

        private IEnumerator Start()
        {
            // Ensure core persistent managers exist
            EnsureManager<SaveSystem>("SaveSystem");
            EnsureManager<AudioManager>("AudioManager");
            EnsureManager<GameManager>("GameManager");

            yield return new WaitForSeconds(splashDuration);

            SceneManager.LoadScene("MainMenu");
        }

        private void EnsureManager<T>(string name) where T : Component
        {
            if (FindFirstObjectByType<T>() == null)
            {
                var go = new GameObject(name);
                go.AddComponent<T>();
                DontDestroyOnLoad(go);
            }
        }
    }
}
