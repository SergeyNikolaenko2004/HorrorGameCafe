using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public string itemName;

    public virtual void Interact()
    {
        Debug.Log("Interacted with: " + itemName);
    }
}