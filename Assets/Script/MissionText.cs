using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class MissionText : MonoBehaviour
{
    private TMP_Text missionText;  // Changement ici pour utiliser TMP_Text
    private bool isCompleted = false; // Ajout de la variable isCompleted

    [System.Serializable]
    public class MissionRequirement
    {
        public string missionDescription;
        public string requiredObject;
        public bool isCompleted;
    }

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

        foreach (var mission in missions)
        {
            Debug.Log($"Comparaison avec mission : {mission.missionDescription} (tag requis : {mission.requiredObject})");

            if (mission.requiredObject == createdObjectTag)
            {
                mission.isCompleted = true;
                Debug.Log($"Mission complétée : {mission.missionDescription}");
                // Optionnel : Mettre à jour le texte de la mission
                missionText.text = $"Mission : {mission.missionDescription} - Complétée !";
                // Vous pouvez également ajouter une couleur ou un style différent pour le texte complété
                missionText.color = Color.green;  // Change la couleur du texte en vert
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
}