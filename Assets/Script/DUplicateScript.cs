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
    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor currentInteractor;

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
        isGrabbed = true;
        rb.useGravity = false;
        currentInteractor = args.interactorObject as UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInteractor;

        // Configurer l'objet pour qu'il se positionne exactement au niveau du contrôleur
        if (grabInteractable != null)
        {
            grabInteractable.attachTransform = null; // Utiliser la transformation de l'objet lui-même
            grabInteractable.trackPosition = true;
            grabInteractable.trackRotation = true;
            grabInteractable.smoothPosition = false; // Désactiver le lissage pour un suivi direct
            grabInteractable.smoothRotation = false;
        }

        if (canDuplicate)
        {
            // Ne dupliquer que si ce n'est pas déjà un duplicata
            if (!gameObject.tag.StartsWith("Duplicate"))
            {
                DuplicateObject();
                canDuplicate = false;
                StartCoroutine(DuplicateCooldown());
            }
        }
    }

    void Update()
    {
        // Si l'objet est saisi, on peut forcer sa position au niveau du contrôleur
        if (isGrabbed && currentInteractor != null)
        {
            // Cette méthode est optionnelle si la configuration du XRGrabInteractable ne suffit pas
            // transform.position = currentInteractor.transform.position;
            // transform.rotation = currentInteractor.transform.rotation;
        }
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        isGrabbed = false;
        isReleased = true;
        rb.useGravity = true;
        currentInteractor = null;

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
        Vector3 spawnPosition;
        Quaternion spawnRotation;

        if (currentInteractor != null)
        {
            // Positionner le duplicata exactement à la position du contrôleur
            spawnPosition = currentInteractor.transform.position;
            spawnRotation = currentInteractor.transform.rotation;
        }
        else
        {
            // Fallback au cas où l'interacteur n'est pas disponible
            Vector3 offset = new Vector3(0f, 0f, -0.5f);
            spawnPosition = transform.position + offset;
            spawnRotation = transform.rotation;
        }

        // Créer le duplicata
        GameObject duplicate = Instantiate(gameObject, spawnPosition, spawnRotation);

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
            if (currentInteractor != null)
            {
                // Appliquer une légère force dans la direction du contrôleur
                duplicateRb.AddForce(currentInteractor.transform.forward * 2f, ForceMode.Impulse);
            }
            duplicateRb.isKinematic = false;
            duplicateRb.constraints = RigidbodyConstraints.None;
        }

        var duplicateGrab = duplicate.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (duplicateGrab != null)
        {
            // Configurer l'interactable du duplicata
            duplicateGrab.attachTransform = null; // Utiliser la transformation de l'objet lui-même
            duplicateGrab.trackPosition = true;
            duplicateGrab.trackRotation = true;
            duplicateGrab.smoothPosition = false;
            duplicateGrab.smoothRotation = false;
            duplicateGrab.throwOnDetach = true;
        }

        // Récupérer la référence FusionObject du duplicata et réinitialiser son état
        FusionObject duplicateFusion = duplicate.GetComponent<FusionObject>();
        if (duplicateFusion != null)
        {
            duplicateFusion.isGrabbed = false;
            duplicateFusion.isReleased = false;
            duplicateFusion.currentInteractor = null;
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
