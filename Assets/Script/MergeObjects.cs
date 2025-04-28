using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using System.Collections.Generic;
using System.Linq;

public class MergeObjects : MonoBehaviour
{
    private Rigidbody rb;
    public bool triggerActivated = false;
    public bool isMerging = false;
    private Camera mainCamera;
    private PlayerInput playerInput;
    private static int fusionCount = 0;
    [SerializeField] private int requiredFusions = 1;
    [SerializeField] private Transform rackTransform;

    [Serializable]
    public struct MergeRule
    {
        public string ruleName;
        public string tag1;
        public string tag2;
        public GameObject resultPrefab;
    }

    [SerializeField]
    private List<MergeRule> mergeRules = new List<MergeRule>();

    [SerializeField] private MissionText missionText;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();

        Debug.Log($"MergeObjects initialisé sur {gameObject.name} avec tag: {gameObject.tag}");
        Debug.Log($"Nombre de règles de fusion: {mergeRules.Count}");

        if (rackTransform == null)
        {
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

    void Update()
    {
    }

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

        MergeObjects otherMergeScript = collision.gameObject.GetComponent<MergeObjects>();
        if (otherMergeScript == null)
        {
            Debug.Log("L'autre objet n'a pas de script MergeObjects");
            return;
        }

        if (isMerging || otherMergeScript.isMerging)
        {
            Debug.Log("Un des objets est déjà en train de fusionner");
            return;
        }

        Debug.Log($"Tentative de fusion entre tags: {gameObject.tag} et {collision.gameObject.tag}");
        var mergeRule = GetMergedRule(gameObject.tag, collision.gameObject.tag);

        if (mergeRule == null)
        {
            Debug.Log("Aucune règle de fusion trouvée pour ces tags");
            return;
        }

        if (mergeRule.Value.resultPrefab == null)
        {
            Debug.LogError($"Le prefab résultant de la règle {mergeRule.Value.ruleName} est null");
            return;
        }

        isMerging = true;
        otherMergeScript.isMerging = true;

        Vector3 rackPosition = rackTransform != null ? rackTransform.position :
            (transform.position + collision.transform.position) / 2;

        Vector3 playerPosition = Camera.main.transform.position + Camera.main.transform.forward * 2f;

        GameObject rackObject = Instantiate(mergeRule.Value.resultPrefab, rackPosition, Quaternion.identity);
        GameObject playerObject = Instantiate(mergeRule.Value.resultPrefab, playerPosition, Quaternion.identity);

        ConfigureNewObject(rackObject, true);
        ConfigureNewObject(playerObject, false);

        // Passer le RuleName ici 👇
        HandleFusionCompletion(playerObject, mergeRule.Value.ruleName);

        Destroy(collision.gameObject);
        Destroy(gameObject);
    }

    private MergeRule? GetMergedRule(string tag1, string tag2)
    {
        foreach (MergeRule rule in mergeRules)
        {
            if ((rule.tag1 == tag1 && rule.tag2 == tag2) || (rule.tag1 == tag2 && rule.tag2 == tag1))
            {
                Debug.Log($"Règle de fusion trouvée : {rule.ruleName}");
                return rule;
            }
        }
        return null;
    }

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

    private void HandleFusionCompletion(GameObject newObject, string ruleName)
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

        CheckAndAssignDiscovery(ruleName); // Utilise maintenant la ruleName
    }

    private void CheckAndAssignDiscovery(string discoveryName)
    {
        bool alreadyDiscovered = false;
        foreach (var slot in FindObjectsOfType<SlotManager>())
        {
            if (slot.gameObject.tag == discoveryName)
            {
                alreadyDiscovered = true;
                break;
            }
        }

        if (!alreadyDiscovered)
        {
            var slotManager = FindObjectOfType<SlotManager>();
            if (slotManager != null)
            {
                slotManager.AssignNewDiscovery(discoveryName);
                Debug.Log($"Nouvelle découverte : {discoveryName}");
            }
        }
    }

    private void ArrangeObjectsInRack()
    {
        if (rackTransform == null) return;

        float spacing = 1.2f;
        float heightOffset = 1f;

        List<Transform> rackObjects = new List<Transform>();
        for (int i = 0; i < rackTransform.childCount; i++)
        {
            rackObjects.Add(rackTransform.GetChild(i));
        }

        int objectCount = rackObjects.Count;
        float startOffset = -(objectCount - 1) * spacing / 2f;

        for (int i = 0; i < objectCount; i++)
        {
            Transform child = rackObjects[i];
            Vector3 newPosition = rackTransform.position + new Vector3(startOffset + i * spacing, heightOffset, 0f);
            StartCoroutine(MoveObjectSmoothly(child, newPosition));
        }
    }

    private IEnumerator MoveObjectSmoothly(Transform objectTransform, Vector3 targetPosition)
    {
        float duration = 0.5f;
        float elapsedTime = 0f;
        Vector3 startingPosition = objectTransform.position;

        while (elapsedTime < duration)
        {
            objectTransform.position = Vector3.Lerp(startingPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        objectTransform.position = targetPosition;
    }

    public void ReorganizeRack()
    {
        if (rackTransform != null)
        {
            ArrangeObjectsInRack();
        }
    }
}
