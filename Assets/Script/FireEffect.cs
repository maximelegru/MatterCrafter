using UnityEngine;

public class FireEffect : MonoBehaviour
{
    public GameObject flamePrefab; // Assigne le prefab du FX dans l’Inspector
    private GameObject activeFlame;

    void Start()
    {
        if (flamePrefab != null)
        {
            activeFlame = Instantiate(flamePrefab, transform.position, Quaternion.identity);
            activeFlame.transform.parent = transform; // Attache-le à l’objet Feu
        }
    }

    public void ActivateFire()
    {
        if (activeFlame != null)
        {
            activeFlame.SetActive(true);
        }
    }

    public void DeactivateFire()
    {
        if (activeFlame != null)
        {
            activeFlame.SetActive(false);
        }
    }
}

