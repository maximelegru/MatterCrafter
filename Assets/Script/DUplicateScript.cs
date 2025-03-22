using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;


public class DUplicateScript : MonoBehaviour
{

    private Rigidbody rb;
    private bool isGrabbed = false;
    private bool canDuplicate = true;
    private float duplicateDelay = 1f; // Délai en secondes
    private float destroyDelay = 3f; // Délai avant destruction
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
                StartCoroutine(DuplicateCooldown());
            }
        }
    }

    private IEnumerator DuplicateCooldown()
    {
        canDuplicate = false;
        yield return new WaitForSeconds(duplicateDelay);
        canDuplicate = true;
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
            canDuplicate = true;
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
        }
    }

}
