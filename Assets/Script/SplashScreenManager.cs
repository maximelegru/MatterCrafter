using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SplashScreenManager : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private string sceneACharger = "BasicScene"; // Nom de la scène à charger
    [SerializeField] private bool afficherBarreProgression = false;

    [Header("Références UI")]
    [SerializeField] private Image barreProgressionImage;
    [SerializeField] private Text texteProgression;
    [SerializeField] private CanvasGroup elementsAFader;
    [SerializeField] private Button boutonCharger; // Référence au bouton de chargement

    private bool chargementTermine = false;

    void Start()
    {
        // Masquer la barre de progression si non utilisée
        if (barreProgressionImage != null && !afficherBarreProgression)
            barreProgressionImage.gameObject.SetActive(false);

        // Configurer le bouton de chargement
        if (boutonCharger != null)
        {
            boutonCharger.gameObject.SetActive(true);
            boutonCharger.onClick.AddListener(ChargerScene);
        }

        // Afficher le message "Prêt à démarrer"
        if (texteProgression != null)
            texteProgression.text = "Prêt à démarrer";

        // Indiquer que le chargement est terminé
        chargementTermine = true;
    }

    // Méthode appelée lors du clic sur le bouton
    public void ChargerScene()
    {
        if (chargementTermine)
        {
            StartCoroutine(FaderEtCharger());
        }
    }

    private IEnumerator FaderEtCharger()
    {
        // Option: Fondu avant de charger la scène
        if (elementsAFader != null)
        {
            float dureeTransition = 0.5f;
            float tempsTransition = 0;

            while (tempsTransition < dureeTransition)
            {
                tempsTransition += Time.deltaTime;
                elementsAFader.alpha = 1 - (tempsTransition / dureeTransition);
                yield return null;
            }
        }

        // Charger la scène suivante
        SceneManager.LoadScene(sceneACharger);
    }
}
