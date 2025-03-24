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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();

        Debug.Log($"MergeObjects initialisé sur {gameObject.name} avec tag: {gameObject.tag}");
        Debug.Log($"Nombre de règles de fusion: {mergeRules.Count}");

    }

    // Update is called once per frame
    void Update()
    {
    }

    [Serializable]
    public struct MergeRule
    {
        public string tag1;
        public string tag2;
        public GameObject resultPrefab;
    }

    [SerializeField]
    private List<MergeRule> mergeRules = new List<MergeRule>();

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

        // Le reste du code reste identique
        isMerging = true;
        otherMergeScript.isMerging = true;
        Vector3 spawnPosition = (transform.position + collision.transform.position) / 2;
        Instantiate(resultObject, spawnPosition, Quaternion.identity);
        Destroy(collision.gameObject);
        Destroy(gameObject);
    }

    private GameObject GetMergedObject(string tag1, string tag2)
    {
        // Chercher dans les règles de fusion
        foreach (MergeRule rule in mergeRules)
        {
            if ((rule.tag1 == tag1 && rule.tag2 == tag2) ||
                (rule.tag1 == tag2 && rule.tag2 == tag1))
            {
                return rule.resultPrefab;
            }
        }
        return null;
    }
}

