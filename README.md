# MatterCrafter

## Description de la structure :

- Le dossier Prefabs contient tous les différents prefabs des fusions donc chaque objet fusionné
- Le dossier Script contient les scripts de fusions, suppression des items, gestion de la duplications et aussi les scripts liés au menu de chargement du jeu
- Tous les assets se trouves directement dans le dossier asset

## Codage intéressant :

- Fusions :

  - Les fusions sont gérées grâce aux tags des éléments comme par exemple : DuplicateEau + DuplicateFeu = Vapeur
  - Pour voir si une fusions existe, je parcours une list des fusions disponibles pour chaque élément

  ```csharp
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
  ```

  ```csharp
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
  ```

  - Impossible de fusionner les éléments d'origine (dont le tag ne comprends pas Duplicate) afin de pouvoir continuer à faire des fusions avec les éléments de bases

- Duplication :

  - Quand un élément est dupliqué, il prend le tag "Duplicate" + le tag de l'objet d'origine

  ```csharp
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
  ```

  ```csharp
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
  ```

  - Quand un objet est laché après l'avoir récupérer, il disparait (fonction DestroyAfterDelay())

  ```csharp
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
  ```

  ```csharp
  private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        if (isReleased && gameObject.tag.StartsWith("Duplicate"))
        {
            Destroy(gameObject);
        }
    }
  ```

  - On peut dupliquer que 3 instances maximal par objet afin d'éviter les lags

  ```csharp
   public int maxInstances = 3;
  ```

  - On ne peux pas dupliquer les objets qui ont le tag Duplicate

  ```csharp
  // Ne dupliquer que si ce n'est pas déjà un duplicata
            if (!gameObject.tag.StartsWith("Duplicate"))
            {
                DuplicateObject();
                canDuplicate = false;
                StartCoroutine(DuplicateCooldown());
            }
  ```

- Suppression :

  - Si il y a plus de trois objets ayant le même tag, alors, il supprime tous les objets en trop pour en n'avoir que trois

  ```csharp
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
  ```
