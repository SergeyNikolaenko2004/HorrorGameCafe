using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    public string itemName = "Interactable Object";

    public abstract void Interact();

    // Метод для визуальной обратной связи
    public virtual void ShowHint()
    {
        Debug.Log($"Можно взаимодействовать с: {itemName}");
    }
}