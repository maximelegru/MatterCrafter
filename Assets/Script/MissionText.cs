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
    private bool isCompleted = false; // Ajout de la variable isCompleted

    [SerializeField] private List<MissionRequirement> missions = new List<MissionRequirement>();

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

    public void UpdateFusionCount(int currentCount, int required)
    {
        if (missionText != null)
        {
            // Configuration du style de base
            missionText.fontSize = 5.4f;  // Taille de police par défaut
            // Vérification de la complétion

            string missionTextContent = $"- Faire une fusion";
            if (currentCount >= required)
            {
                isCompleted = true;
                Debug.Log("Mission complétée !");
                const string STRIKE_START = "<s>";
                const string STRIKE_END = "</s>";
                missionTextContent = $"{STRIKE_START}{missionTextContent}{STRIKE_END}";
                Debug.Log($"Mise à jour du texte avec barré : {missionTextContent}");
            }

            missionText.text = missionTextContent;
            Debug.Log($"Mise à jour du texte : {missionTextContent}");
        }
        else
        {
            Debug.LogError("Le composant TMP_Text n'est pas assigné !");
        }
    }

    public void CheckObjectExistence()
    {
        // Chercher spécifiquement le TextMeshPro avec le tag QuestMontagne
        GameObject questMontagne = GameObject.FindGameObjectWithTag("QuestMontagne");

        if (questMontagne != null)
        {
            TMP_Text montagneText = questMontagne.GetComponent<TMP_Text>();
            if (montagneText != null)
            {
                Debug.Log($"Texte de quête montagne trouvé : {questMontagne.name}");

                // Rechercher un objet avec le tag Montagne
                GameObject mountain = GameObject.FindGameObjectWithTag("Montagne");
                string questText = "- Créer une montagne";

                if (mountain != null)
                {
                    Debug.Log("Une montagne a été trouvée dans la scène!");
                    const string STRIKE_START = "<s>";
                    const string STRIKE_END = "</s>";
                    questText = $"{STRIKE_START}{questText}{STRIKE_END}";
                    isCompleted = true;
                }
                else
                {
                    Debug.Log("Aucune montagne trouvée dans la scène");
                    isCompleted = false;
                }

                // Mettre à jour le texte de la quête montagne
                montagneText.text = questText;
                Debug.Log($"Mise à jour du texte de la quête montagne : {questText}");
            }
        }
        else
        {
            Debug.LogWarning("Aucun GameObject avec le tag 'QuestMontagne' trouvé");
        }
    }
}