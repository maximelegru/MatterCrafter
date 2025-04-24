using UnityEngine;
using System.Collections.Generic;

public class DeleteDuplicateObject : MonoBehaviour
{
    [Tooltip("Nombre maximum d'instances à conserver")]
    public int maxInstances = 3;

    private string objectTag;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        objectTag = gameObject.tag;

        if (objectTag == "Untagged")
        {
            Debug.LogWarning("L'objet avec le script DeleteDuplicateObject n'a pas de tag défini. Le script ne fonctionnera pas correctement.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        LimitObjectInstances();
    }

    void LimitObjectInstances()
    {
        if (objectTag == "Untagged")
            return;

        GameObject[] objects = GameObject.FindGameObjectsWithTag(objectTag);

        if (objects.Length > maxInstances)
        {
            // Trier les objets par ordre de création (en supposant que les plus récents ont un index plus élevé)
            List<GameObject> objectList = new List<GameObject>(objects);

            // Supprimer les objets en excès (garder seulement maxInstances)
            for (int i = maxInstances; i < objectList.Count; i++)
            {
                Destroy(objectList[i]);
            }
        }
    }

    void DetectDestroyObJectInBind()
    {
        // Cette méthode n'est plus nécessaire car la logique est dans les méthodes OnCollisionEnter et OnTriggerEnter
    }

    // Détecte les collisions (requiert un Collider)
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("BinDetector") && gameObject.tag.StartsWith("Duplicate"))
        {
            Destroy(gameObject);
        }
    }

    // Détecte les triggers (requiert un Collider configuré comme Trigger)
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("BinDetector") && gameObject.tag.StartsWith("Duplicate"))
        {
            Destroy(gameObject);
        }
    }
}
