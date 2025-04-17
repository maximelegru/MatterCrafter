using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashScreenLoader : MonoBehaviour
{
    [SerializeField] private Image progressBar;
    [SerializeField] private Text progressText;
    [SerializeField] private string sceneToLoad;
    [SerializeField] private float minimumDisplayTime = 2.0f;

    void Start()
    {
        StartCoroutine(LoadSceneAsync());
    }

    private IEnumerator LoadSceneAsync()
    {
        float startTime = Time.time;

        // Démarrer le chargement asynchrone
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncLoad.allowSceneActivation = false;

        while (!asyncLoad.isDone)
        {
            // Progression entre 0 et 0.9 (90%)
            float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

            // Mettre à jour les éléments UI
            if (progressBar != null)
                progressBar.fillAmount = progress;

            if (progressText != null)
                progressText.text = $"Chargement... {(int)(progress * 100)}%";

            // Si chargement terminé ET temps minimum écoulé
            if (asyncLoad.progress >= 0.9f && Time.time - startTime >= minimumDisplayTime)
            {
                asyncLoad.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}
