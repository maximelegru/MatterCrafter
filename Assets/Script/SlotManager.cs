using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;
using System.Collections.Generic;

public class SlotManager : MonoBehaviour
{
    [Header("Associations entre Tags, Sprites et Prefabs")]
    public List<TagAssociation> tagAssociations;

    private Dictionary<string, TagAssociation> tagAssociationDict;
    private Image slotImage;
    private string previousTag;
    private XRInteractionManager interactionManager;

    // 🔥 Nouvelle liste pour suivre tous les objets instanciés par ce Slot
    private List<GameObject> spawnedObjects = new List<GameObject>();

    private void Awake()
    {
        interactionManager = FindObjectOfType<XRInteractionManager>();
        if (interactionManager == null)
        {
            Debug.LogError("XRInteractionManager non trouvé dans la scène !");
        }

        tagAssociationDict = new Dictionary<string, TagAssociation>();
        foreach (var assoc in tagAssociations)
        {
            if (!tagAssociationDict.ContainsKey(assoc.tag))
            {
                tagAssociationDict.Add(assoc.tag, assoc);
            }
        }

        slotImage = GetComponentInChildren<Image>();

        var interactable = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnGrabSlot);
            interactable.selectExited.AddListener(OnReleaseSlot);
        }
        else
        {
            Debug.LogError("Aucun XRSimpleInteractable trouvé sur ce GameObject !");
        }
    }

    private void Update()
    {
        if (previousTag != gameObject.tag)
        {
            UpdateSlotImage();
            previousTag = gameObject.tag;
        }
    }

    private void UpdateSlotImage()
    {
        if (slotImage != null && tagAssociationDict.TryGetValue(gameObject.tag, out TagAssociation assoc))
        {
            slotImage.sprite = assoc.sprite;
        }
    }

    private void OnGrabSlot(SelectEnterEventArgs args)
    {
        if (!tagAssociationDict.TryGetValue(gameObject.tag, out TagAssociation assoc))
        {
            Debug.LogWarning($"Aucune association trouvée pour le tag : {gameObject.tag}");
            return;
        }

        if (assoc.prefab == null)
        {
            Debug.LogWarning($"Prefab manquant pour le tag : {gameObject.tag}");
            return;
        }

        // 🔥 Instancier un nouvel objet pour CE grab
        GameObject newObject = Instantiate(assoc.prefab, transform.position, transform.rotation);
        newObject.transform.SetParent(null, true);

        // Ajouter Rigidbody si nécessaire
        var rb = newObject.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = newObject.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;
        rb.useGravity = true;

        // Ajouter XRGrabInteractable si nécessaire
        var newInteractable = newObject.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (newInteractable == null)
        {
            newInteractable = newObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        }

        // Enregistrer auprès du Interaction Manager
        if (interactionManager != null)
        {
            interactionManager.RegisterInteractable((UnityEngine.XR.Interaction.Toolkit.Interactables.IXRInteractable)newInteractable);
        }

        var interactor = args.interactorObject;
        if (interactor == null)
        {
            Debug.LogWarning("Interactor est null !");
            return;
        }

        // Vérifier si c'est un interactor valide
        if (interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor selectInteractor)
        {
            // 🔥 Pas besoin de libérer ici

            // Utiliser une coroutine pour attendre un frame
            StartCoroutine(ForceSelectNextFrame(selectInteractor, newInteractable));
        }
        else
        {
            Debug.LogWarning("Interactor n'est pas un IXRSelectInteractor valide !");
        }

        // 🔥 Ajouter à la liste des objets spawnés
        spawnedObjects.Add(newObject);
    }

    private IEnumerator ForceSelectNextFrame(UnityEngine.XR.Interaction.Toolkit.Interactors.IXRSelectInteractor interactor, UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable)
    {
        yield return null; // attendre un frame

        if (interactionManager != null && interactor != null && interactable != null)
        {
            interactionManager.SelectEnter(interactor, interactable);
        }
    }

    private void OnReleaseSlot(SelectExitEventArgs args)
    {
        // 🔥 Trouver l'objet relâché
        GameObject releasedObject = args.interactableObject?.transform?.gameObject;

        if (releasedObject != null && spawnedObjects.Contains(releasedObject))
        {
            // Retirer et détruire seulement cet objet
            spawnedObjects.Remove(releasedObject);
            Destroy(releasedObject);
        }
        
    }

    public List<SlotManager> orderedSlots;
    public void AssignNewDiscovery(string newTag)
    {
        foreach (var slot in orderedSlots)
        {
            if (slot.gameObject.tag == "NotDefined")
            {
                slot.gameObject.tag = newTag;
                slot.UpdateSlotImage();
                Debug.Log($"Découverte assignée : {newTag} au slot {slot.name}");
                break;
            }
        }
    }
}

[System.Serializable]
public class TagAssociation
{
    public string tag;
    public Sprite sprite;
    public GameObject prefab;
}
