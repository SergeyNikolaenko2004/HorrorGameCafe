using System.Collections;
using TMPro;
using UnityEngine;

public class CoffeeOrderManager : MonoBehaviour
{
    public static CoffeeOrderManager Instance;

    public enum OrderState
    {
        CanTakeCup,     // Можно взять стакан
        HasEmptyCup,    // В руках пустой стакан
        CupInMachine,   // Стакан в кофемашине (наливается)
        HasFilledCup,   // В руках наполненный стакан
        HasLid,         // В руках крышка
        CoffeeReady     // Кофе полностью готов
    }

    public OrderState currentState = OrderState.CanTakeCup;
    public GameObject emptyCupPrefab;
    public GameObject filledCupPrefab;
    public GameObject lidPrefab;
    public GameObject sealedCoffeePrefab; 
    public Transform handPosition;
    public NPCController targetNPC;
    public float throwForce = 10f;
    private GameObject currentCupInHand;
    private GameObject currentLidInHand;


    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    void Update()
    {
        if (currentState == OrderState.CoffeeReady && Input.GetMouseButtonDown(0))
        {
            ThrowCoffee();
        }
    }

    public void ChangeState(OrderState newState)
    {
        currentState = newState;
        UpdateHandVisuals();
    }

    void UpdateHandVisuals()
    {
        if (currentState != OrderState.CupInMachine)
        {
            if (currentCupInHand != null) Destroy(currentCupInHand);
            if (currentLidInHand != null) Destroy(currentLidInHand);
        }
        else
        {
            if (currentLidInHand != null) Destroy(currentLidInHand);
        }

        switch (currentState)
        {
            case OrderState.HasEmptyCup:
                if (emptyCupPrefab != null && handPosition != null)
                {
                    currentCupInHand = Instantiate(emptyCupPrefab, handPosition);
                    ResetObjectTransform(currentCupInHand);
                }
                break;

            case OrderState.HasFilledCup:
                if (filledCupPrefab != null && handPosition != null)
                {
                    currentCupInHand = Instantiate(filledCupPrefab, handPosition);
                    ResetObjectTransform(currentCupInHand);
                }
                break;

            case OrderState.HasLid:
                if (lidPrefab != null && handPosition != null)
                {
                    currentLidInHand = Instantiate(lidPrefab, handPosition);
                    ResetObjectTransform(currentLidInHand);
                }
                break;

            case OrderState.CoffeeReady:
                if (sealedCoffeePrefab != null && handPosition != null)
                {
                    currentCupInHand = Instantiate(sealedCoffeePrefab, handPosition);
                    ResetObjectTransform(currentCupInHand);
                }
                break;

            case OrderState.CupInMachine:
                break;

            case OrderState.CanTakeCup:
                break;
        }
    }

    void ResetObjectTransform(GameObject obj)
    {
        if (obj != null)
        {
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }
    }

    public GameObject GetCurrentCup()
    {
        return currentCupInHand;
    }

    public void ClearCurrentCup()
    {
        currentCupInHand = null;
    }

    public GameObject GetSealedCoffeePrefab()
    {
        return sealedCoffeePrefab;
    }

    // Бросок кофе в NPC
    void ThrowCoffee()
    {
        if (currentCupInHand != null && targetNPC != null)
        {
            Debug.Log("Бросаем кофе в NPC!");
            GameObject thrownCoffee = Instantiate(sealedCoffeePrefab,
                handPosition.position,
                handPosition.rotation);

            Rigidbody rb = thrownCoffee.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = thrownCoffee.AddComponent<Rigidbody>();
            }
            Vector3 throwDirection = handPosition.forward + Vector3.up * 0.2f;
            rb.AddForce(throwDirection * throwForce, ForceMode.Impulse);
            ClearHandPosition();
            currentCupInHand = null;
            currentState = OrderState.CanTakeCup;
            Debug.Log("Состояние изменено на: " + currentState);
            targetNPC.ThrowCoffeeAtNPC();
            Destroy(thrownCoffee, 3f);

            Debug.Log("Кофе брошено! Руки пустые.");
        }
        else
        {
            Debug.LogWarning("Не могу бросить кофе: " +
                (currentCupInHand == null ? "кофе нет в руках" : "NPC не назначен"));
        }
    }

    // Метод для полной очистки handPosition от всех дочерних объектов
    void ClearHandPosition()
    {
        if (handPosition != null)
        {
            // Уничтожаем все дочерние объекты
            foreach (Transform child in handPosition)
            {
                Destroy(child.gameObject);
            }
            Debug.Log("HandPosition очищен от всех дочерних объектов");
        }

        // Также очищаем ссылки
        currentCupInHand = null;
        currentLidInHand = null;
    }
}