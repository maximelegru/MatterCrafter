using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class FusionObject : MonoBehaviour
{
    private Rigidbody rb;
    private bool isGrabbed = false;
    private bool canDuplicate = true;
    private float duplicateDelay = 2f;
    private float destroyDelay = 2f;
    private bool isReleased = false;
    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grabInteractable;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
            grabInteractable.selectExited.AddListener(OnRelease);
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        if (!isGrabbed && canDuplicate)
        {
            isGrabbed = true;
            rb.useGravity = false;

            // Ne dupliquer que si ce n'est pas déjà un duplicata
            if (!gameObject.tag.StartsWith("Duplicate"))
            {
                DuplicateObject();
                canDuplicate = false;
                StartCoroutine(DuplicateCooldown());
            }
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        isReleased = true;
        rb.useGravity = true;

        if (gameObject.tag.StartsWith("Duplicate"))
        {
            StartCoroutine(DestroyAfterDelay());
        }
    }

    private IEnumerator DuplicateCooldown()
    {
        yield return new WaitForSeconds(duplicateDelay);
        canDuplicate = true;
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        if (isReleased && gameObject.tag.StartsWith("Duplicate"))
        {
            Destroy(gameObject);
        }
    }

    private void DuplicateObject()
    {
        // Position de spawn avec un décalage
        Vector3 offset = new Vector3(0f, 0f, -0.5f);
        Vector3 spawnPosition = transform.position + offset;

        // Créer le duplicata
        GameObject duplicate = Instantiate(gameObject, spawnPosition, transform.rotation);

        // Définir le nouveau tag
        string newTag = "Duplicate" + gameObject.tag;
        duplicate.tag = newTag;

        // Configurer la physique du duplicata
        Rigidbody duplicateRb = duplicate.GetComponent<Rigidbody>();
        if (duplicateRb != null)
        {
            duplicateRb.linearVelocity = Vector3.zero;
            duplicateRb.angularVelocity = Vector3.zero;
            duplicateRb.useGravity = true;
            duplicateRb.AddForce(offset.normalized * 2f, ForceMode.Impulse);
            duplicateRb.isKinematic = false;
            duplicateRb.constraints = RigidbodyConstraints.None;
        }

        var duplicateGrab = duplicate.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (duplicateGrab != null)
        {
            // Désactiver les contraintes de mouvement
            duplicateGrab.trackPosition = true;
            duplicateGrab.trackRotation = true;
            duplicateGrab.smoothPosition = false;
            duplicateGrab.smoothRotation = false;
            duplicateGrab.throwOnDetach = true;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Vérifier si l'un des objets est un duplicata
        if (gameObject.tag.StartsWith("Duplicate"))
        {
            GameObject otherObject = collision.gameObject;

            // Vérifier si l'autre objet a le même tag de base
            if (otherObject.tag.StartsWith("Duplicate") && HasSameBaseTag(otherObject))
            {
                MergeObjects(otherObject);
            }
        }
    }

    private bool HasSameBaseTag(GameObject other)
    {
        string thisBaseTag = gameObject.tag.Replace("Duplicate", "");
        string otherBaseTag = other.tag.Replace("Duplicate", "");
        return thisBaseTag == otherBaseTag;
    }

    private void MergeObjects(GameObject other)
    {
        // Calculer la position moyenne
        Vector3 mergedPosition = (transform.position + other.transform.position) / 2f;

        // Augmenter la taille de l'objet actuel
        transform.localScale *= 1.2f;
        transform.position = mergedPosition;

        // Détruire l'autre objet
        Destroy(other);
    }
}
