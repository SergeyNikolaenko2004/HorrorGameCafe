using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    public string itemName = "Interactable Object";

    public abstract void Interact();

    public virtual void ShowHint()
    {
        Debug.Log($"Можно взаимодействовать с: {itemName}");
    }
}