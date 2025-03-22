using UnityEngine;
using UnityEngine.InputSystem;


public class MergeObjects : MonoBehaviour
{

    private Rigidbody rb;
    public bool triggerActivated = false;
    public GameObject objectToSpawn;
    public bool isMerging = false;
    private Camera mainCamera;
    private PlayerInput playerInput;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        playerInput = GetComponent<PlayerInput>();

    }

    // Update is called once per frame
    void Update()
    {
    }

    void OnCollisionEnter(Collision collision)
    {
        MergeObjects otherMergeScript = collision.gameObject.GetComponent<MergeObjects>();

        if (otherMergeScript != null && !isMerging && !otherMergeScript.isMerging)
        {// Vérifier si les objets ont le même tag
            if (gameObject.tag == collision.gameObject.tag)
            {
                isMerging = true;
                otherMergeScript.isMerging = true;

                // Grossir l'objet actuel
                Vector3 newScale = transform.localScale * 1.5f; // Augmente la taille de 50%
                transform.localScale = newScale;

                // Détruire l'autre objet
                Destroy(collision.gameObject);
            }
            else
            {
                // Comportement existant pour des objets de tags différents
                isMerging = true;
                otherMergeScript.isMerging = true;

                Vector3 spawnPosition = (transform.position + collision.transform.position) / 2;
                Instantiate(objectToSpawn, spawnPosition, Quaternion.identity);

                Destroy(collision.gameObject);
                Destroy(gameObject);
            }
        }

    }

    private GameObject GetMergedObject(MergeObjects obj1, MergeObjects obj2)
    {
        // Exemple de logique : Assigner manuellement les combinaisons
        if (obj1.objectToSpawn != obj2.objectToSpawn)
        {
            return obj1.objectToSpawn; // Choisir une logique selon le projet
        }
        return null;
    }
}

