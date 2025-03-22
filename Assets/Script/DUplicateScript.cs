using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;


public class DUplicateScript : MonoBehaviour
{

    private Rigidbody rb;
    private bool isGrabbed = false;
    private bool canDuplicate = true;
    private float duplicateDelay = 2.6f; // Délai en secondes
    private float destroyDelay = 2f; // Délai avant destruction
    private bool isReleased = false;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        // Ajouter des listeners pour les événements de grab
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (!isGrabbed && canDuplicate)
        {
            isGrabbed = true;
            rb.useGravity = false; // Désactiver la gravité quand l'objet est attrapé
            if (gameObject.tag != "Duplicate")
            {
                DuplicateObject();
                canDuplicate = false;
                StartCoroutine(DuplicateCooldown());
            }
        }
    }

    private IEnumerator DuplicateCooldown()
    {
        Debug.Log("Début du cooldown");
        // Attendre le délai spécifié
        yield return new WaitForSeconds(duplicateDelay);
        // Réactiver la possibilité de dupliquer
        canDuplicate = true;
        Debug.Log("Fin du cooldown - Duplication à nouveau possible");
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        isReleased = true;
        rb.useGravity = true; // Réactiver la gravité quand l'objet est relâché

        if (gameObject.tag == "Duplicate")
        {
            StartCoroutine(DestroyAfterDelay());
        }
        else
        {
            isReleased = false;
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        // Vérifier si l'objet est toujours relâché et que le tag est "Duplicate"
        if (isReleased && gameObject.tag == "Duplicate")
        {
            Destroy(gameObject);
        }
    }

    void DuplicateObject()
    {
        if (isReleased) return; // Ne pas dupliquer si relâché

        // Calculer une position légèrement décalée pour le duplicata
        Vector3 offset = new Vector3(0f, 0f, 1f); // Décalage de 1 unité sur l'axe X
        Vector3 spawnPosition = transform.position + offset;

        // Créer le duplicata
        GameObject duplicate = Instantiate(gameObject, spawnPosition, transform.rotation);

        try
        {
            duplicate.tag = "Duplicate";
        }
        catch (UnityException e)
        {
            Debug.Log("Impossible de changer le tag de l'objet : " + e.Message);
            Destroy(duplicate);
            return;
        }

        // Réinitialiser la physique du duplicata
        Rigidbody duplicateRb = duplicate.GetComponent<Rigidbody>();
        if (duplicateRb != null)
        {
            duplicateRb.linearVelocity = Vector3.zero;
            duplicateRb.angularVelocity = Vector3.zero;
            duplicateRb.useGravity = true;
            // Ajouter une force pour éloigner les duplicatas
            duplicateRb.AddForce(offset * 100f, ForceMode.Impulse);
            // Enlever les contraintes de position et de rotation
            duplicateRb.constraints = RigidbodyConstraints.None;
        }
    }

}
