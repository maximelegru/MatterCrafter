using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using System.Linq; // Ajout de cette ligne pour utiliser LINQ

public class MergeObjects : MonoBehaviour
{

    private Rigidbody rb;
    public bool triggerActivated = false;
    public bool isMerging = false;
    private Camera mainCamera;
    private PlayerInput playerInput;
    private static int fusionCount = 0;  // Static pour partager le compteur entre tous les objets
    [SerializeField] private int requiredFusions = 1;
    [SerializeField] private Transform rackTransform; // Référence au Rack

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();

        Debug.Log($"MergeObjects initialisé sur {gameObject.name} avec tag: {gameObject.tag}");
        Debug.Log($"Nombre de règles de fusion: {mergeRules.Count}");

        if (rackTransform == null)
        {
            // Chercher automatiquement le Rack s'il n'est pas assigné
            GameObject rack = GameObject.FindGameObjectWithTag("Rack");
            if (rack != null)
            {
                rackTransform = rack.transform;
            }
            else
            {
                Debug.LogWarning("Aucun Rack trouvé dans la scène!");
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    [Serializable]
    public struct MergeRule
    {
        public string ruleName; // Nom de la règle de fusion
        public string tag1;
        public string tag2;
        public GameObject resultPrefab;
    }

    [SerializeField]
    private List<MergeRule> mergeRules = new List<MergeRule>();

    [SerializeField] private MissionText missionText; // Référence au script MissionText


    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Collision détectée avec {collision.gameObject.name}");
        Debug.Log($"L'objet est-il actif? {collision.gameObject.activeInHierarchy}");

        Component[] components = collision.gameObject.GetComponents<Component>();
        string componentList = "";
        foreach (Component c in components)
        {
            componentList += c.GetType().Name + ", ";
        }
        Debug.Log($"Components sur l'objet: {componentList}");

        // Récupération du script MergeObjects de l'autre objet
        MergeObjects otherMergeScript = collision.gameObject.GetComponent<MergeObjects>();

        if (otherMergeScript == null)
        {
            Debug.Log("L'autre objet n'a pas de MergeObjects script");
            return;
        }

        if (isMerging || otherMergeScript.isMerging)
        {
            Debug.Log("Un des objets est déjà en train de fusionner");
            return;
        }

        Debug.Log($"Tentative de fusion entre tags: {gameObject.tag} et {collision.gameObject.tag}");
        GameObject resultObject = GetMergedObject(gameObject.tag, collision.gameObject.tag);

        if (resultObject == null)
        {
            Debug.Log("Aucune règle de fusion trouvée pour ces tags");
            return;
        }

        if (resultObject != null)
        {
            isMerging = true;
            otherMergeScript.isMerging = true;

            // Position dans le Rack
            Vector3 rackPosition = rackTransform != null ? rackTransform.position :
                (transform.position + collision.transform.position) / 2;

            // Position devant le joueur
            Vector3 playerPosition = Camera.main.transform.position + Camera.main.transform.forward * 2f;

            // Instancier l'objet dans le rack
            GameObject rackObject = Instantiate(resultObject, rackPosition, Quaternion.identity);

            // Instancier l'objet devant le joueur
            GameObject playerObject = Instantiate(resultObject, playerPosition, Quaternion.identity);

            // Configurer l'objet du rack
            ConfigureNewObject(rackObject, true);  // true pour indiquer que c'est l'objet du rack

            // Configurer l'objet du joueur
            ConfigureNewObject(playerObject, false);  // false pour indiquer que ce n'est pas l'objet du rack

            // Incrémenter le compteur de fusion et mettre à jour les missions
            HandleFusionCompletion(playerObject);

            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }

    private void ArrangeObjectsInRack()
    {
        if (rackTransform == null) return;

        // Configuration de l'arrangement
        float spacing = 1.0f;  // Réduire l'espacement entre les objets
        float startOffset = 0f; // Offset de départ pour centrer les objets
        float heightOffset = 1f; // Hauteur des objets par rapport au rack

        // Récupérer tous les enfants du Rack
        int childCount = rackTransform.childCount;

        // Calculer l'offset de départ pour centrer les objets
        startOffset = -(childCount - 1) * spacing / 2f;

        // Arranger chaque objet
        for (int i = 0; i < childCount; i++)
        {
            Transform child = rackTransform.GetChild(i);

            // Calculer la nouvelle position
            Vector3 newPosition = rackTransform.position + new Vector3(startOffset + i * spacing, heightOffset, 0f);
            child.position = newPosition;
        }
    }

    private GameObject GetMergedObject(string tag1, string tag2)
    {
        // Chercher dans les règles de fusion
        foreach (MergeRule rule in mergeRules)
        {
            if ((rule.tag1 == tag1 && rule.tag2 == tag2) ||
                (rule.tag1 == tag2 && rule.tag2 == tag1))
            {
                Debug.Log($"Règle de fusion trouvée : {rule.ruleName}");
                Debug.Log($"Tags : {rule.tag1} et {rule.tag2}");
                Debug.Log($"Objet résultant : {rule.resultPrefab.name}");

                // Vérifier uniquement si le prefab est null
                if (rule.resultPrefab == null)
                {
                    Debug.LogError($"Le prefab résultant de la règle {rule.ruleName} est null");
                    return null;
                }

                return rule.resultPrefab;
            }
        }
        return null;
    }

    // Nouvelle méthode pour configurer les objets
    private void ConfigureNewObject(GameObject newObject, bool isRackObject)
    {
        if (!newObject.GetComponent<MergeObjects>())
        {
            MergeObjects newMergeScript = newObject.AddComponent<MergeObjects>();
            newMergeScript.mergeRules = this.mergeRules;
            newMergeScript.rackTransform = this.rackTransform;
            newMergeScript.missionText = this.missionText;
        }

        if (!newObject.GetComponent<Rigidbody>())
        {
            Rigidbody rb = newObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        if (isRackObject && rackTransform != null)
        {
            newObject.transform.SetParent(rackTransform);
            ArrangeObjectsInRack();
        }
    }

    // Nouvelle méthode pour gérer la complétion de fusion
    private void HandleFusionCompletion(GameObject newObject)
    {
        fusionCount++;
        Debug.Log($"Fusion effectuée ! Total : {fusionCount}/{requiredFusions}");

        if (missionText != null)
        {
            missionText.UpdateFusionCount(fusionCount, requiredFusions);

            if (newObject.CompareTag("Montagne"))
            {
                missionText.CheckObjectExistence();
            }
            else if (newObject.CompareTag("Pluie"))
            {
                missionText.CheckRainObjectExistence();
            }
            else if (newObject.CompareTag("FioleEau"))
            {
                missionText.CheckWaterBottleExistence();
            }
        }
    }
}

