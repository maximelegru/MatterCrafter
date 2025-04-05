using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]  // Important pour la sérialisation dans Unity
public struct MissionRequirement  // Utiliser struct au lieu de class
{
    [SerializeField] public string missionDescription;  // Ajouter [SerializeField]
    [SerializeField] public string requiredObject;      // Ajouter [SerializeField]
    [SerializeField] public bool isCompleted;           // Ajouter [SerializeField]
}

public class MissionText : MonoBehaviour
{
    private TMP_Text missionText;  // Changement ici pour utiliser TMP_Text
    private static int questsCompleted = 0; // Compteur de quêtes complétées
    private static int totalQuests = 3; // Nombre total de quêtes (fusion, montagne, pluie)
    [SerializeField] private GameObject questCounterText; // Référence au texte du compteur

    [SerializeField] private List<MissionRequirement> missions = new List<MissionRequirement>();

    // Au lieu d'un seul booléen isCompleted, utilisons un dictionnaire pour suivre l'état de chaque quête
    private Dictionary<string, bool> questStatus = new Dictionary<string, bool>() {
        { "Fusion", false },
        { "Montagne", false },
        { "Pluie", false },
        { "Tutoriel", false },  // Ajouter le statut pour le tutoriel
        { "Fiole", false }    // Ajouter le statut pour la fiole
    };

    void Start()
    {
        missionText = GetComponent<TMP_Text>();
        if (missionText != null)
        {
            missionText.richText = true;  // Active le support des balises Rich Text
            missionText.parseCtrlCharacters = true;  // Active le parsing des caractères de contrôle
        }
        if (missionText == null)
        {
            Debug.LogError("Composant TMP_Text manquant sur " + gameObject.name);
            return;
        }

        // Vérification des missions configurées
        Debug.Log($"Nombre de missions configurées : {missions.Count}");
        foreach (var mission in missions)
        {
            Debug.Log($"Mission : {mission.missionDescription}, Tag requis : {mission.requiredObject}");
        }

    }

    public void CheckMission(string createdObjectTag)
    {
        Debug.Log($"Vérification de mission pour l'objet avec tag : {createdObjectTag}");

        for (int i = 0; i < missions.Count; i++)
        {
            var mission = missions[i];
            Debug.Log($"Comparaison avec mission : {mission.missionDescription} (tag requis : {mission.requiredObject})");

            if (mission.requiredObject == createdObjectTag)
            {
                // Créer une copie modifiée de la mission
                var updatedMission = missions[i];
                updatedMission.isCompleted = true;
                missions[i] = updatedMission;  // Réassigner la mission modifiée

                Debug.Log($"Mission complétée : {mission.missionDescription}");

                // Mettre à jour le texte de la mission
                missionText.text = $"Mission : {mission.missionDescription} - Complétée !";
                missionText.color = Color.green;
            }
            else
            {
                Debug.Log($"Mission non complétée : {mission.missionDescription}");
            }
        }
    }

    private void IncrementQuestCounter()
    {
        questsCompleted++;
        UpdateQuestCounter();
    }

    private void UpdateQuestCounter()
    {
        // Trouver le texte du compteur
        GameObject counter = GameObject.FindGameObjectWithTag("QuestCounter");
        if (counter != null)
        {
            TMP_Text counterText = counter.GetComponent<TMP_Text>();
            if (counterText != null)
            {
                string text = $"Quêtes : {questsCompleted}/{totalQuests}";

                // Ajouter la couleur verte si toutes les quêtes sont complétées
                if (questsCompleted >= totalQuests)
                {
                    text = $"<color=#00FF00>{text}</color>";
                }

                counterText.text = text;
                Debug.Log($"Compteur mis à jour : {text}");
            }
        }
    }

    public void UpdateFusionCount(int currentCount, int required)
    {
        if (missionText != null && !questStatus["Fusion"])
        {
            if (currentCount >= required)
            {
                missionText.text = $"<s>- Faire une fusion</s>";
                questStatus["Fusion"] = true;
                IncrementQuestCounter();
            }
        }
    }

    public void CheckObjectExistence()
    {
        GameObject mountain = GameObject.FindGameObjectWithTag("Montagne");
        GameObject questText = GameObject.FindGameObjectWithTag("QuestMontagne");

        if (questText != null && !questStatus["Montagne"])
        {
            TMP_Text montagneText = questText.GetComponent<TMP_Text>();
            if (mountain != null)
            {
                montagneText.text = "<s>- Créer une montagne</s>";
                questStatus["Montagne"] = true;
                IncrementQuestCounter();
            }
        }
    }

    public void CheckRainObjectExistence()
    {
        GameObject rain = GameObject.FindGameObjectWithTag("Pluie");
        GameObject questText = GameObject.FindGameObjectWithTag("QuestPluie");

        if (questText != null && !questStatus["Pluie"])
        {
            TMP_Text pluieText = questText.GetComponent<TMP_Text>();
            if (rain != null)
            {
                pluieText.text = "<s>- Créer de la pluie</s>";
                questStatus["Pluie"] = true;
                IncrementQuestCounter();
            }
        }
    }

    // Nouvelle méthode pour vérifier la carte du tutoriel
    public void CheckTutorialCard()
    {
        Debug.Log("=== VÉRIFICATION CARTE TUTORIEL 5 ===");

        // Rechercher la carte 5 spécifiquement
        GameObject tutorialCard = GameObject.FindGameObjectWithTag("TutorialCard5");
        GameObject questText = GameObject.FindGameObjectWithTag("QuestTutorial");

        if (tutorialCard != null)
        {
            Debug.Log($"Carte 5 trouvée - État actif: {tutorialCard.activeInHierarchy}");

            // Vérifier si c'est la carte active
            if (tutorialCard.activeInHierarchy && !questStatus["Tutoriel"])
            {
                Debug.Log("La carte 5 est actuellement affichée!");

                if (questText != null)
                {
                    TMP_Text tutorialText = questText.GetComponent<TMP_Text>();
                    if (tutorialText != null)
                    {
                        tutorialText.text = "<s>- Lire le tutoriel</s>";
                        questStatus["Tutoriel"] = true;
                        IncrementQuestCounter();
                        Debug.Log("=== QUÊTE TUTORIEL COMPLÉTÉE ===");
                    }
                }
            }
        }
    }

    public void CheckWaterBottleExistence()
    {
        Debug.Log("=== DÉBUT VÉRIFICATION FIOLE D'EAU ===");

        // Rechercher la fiole d'eau et le texte de la quête
        GameObject waterBottle = GameObject.FindGameObjectWithTag("FioleEau");
        Debug.Log($"Fiole d'eau trouvée ? {waterBottle != null}");
        if (waterBottle != null)
        {
            Debug.Log($"Nom de l'objet fiole : {waterBottle.name}");
        }

        GameObject questText = GameObject.FindGameObjectWithTag("QuestFiole");
        Debug.Log($"Texte de quête trouvé ? {questText != null}");
        if (questText != null)
        {
            Debug.Log($"Nom du texte de quête : {questText.name}");
        }

        Debug.Log($"État actuel de la quête Fiole : {questStatus["Fiole"]}");

        if (questText != null && !questStatus["Fiole"])
        {
            TMP_Text fioleText = questText.GetComponent<TMP_Text>();
            Debug.Log($"Composant TMP_Text trouvé ? {fioleText != null}");

            if (waterBottle != null)
            {
                Debug.Log("Conditions remplies pour compléter la quête!");
                fioleText.text = "<s>- Créer une fiole d'eau</s>";
                questStatus["Fiole"] = true;
                IncrementQuestCounter();
                Debug.Log("=== QUÊTE FIOLE COMPLÉTÉE ===");
            }
            else
            {
                Debug.Log("La fiole d'eau n'est pas encore créée");
            }
        }
        else
        {
            if (questStatus["Fiole"])
            {
                Debug.Log("La quête est déjà complétée");
            }
            else
            {
                Debug.Log("Le texte de la quête n'est pas trouvé");
            }
        }

        Debug.Log("=== FIN VÉRIFICATION FIOLE D'EAU ===");
    }
}