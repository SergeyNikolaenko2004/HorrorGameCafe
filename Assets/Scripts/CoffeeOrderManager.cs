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

    // Префабы для визуализации в руках
    public GameObject emptyCupPrefab;
    public GameObject filledCupPrefab;
    public GameObject lidPrefab;
    public GameObject sealedCoffeePrefab; // Префаб закрытого стакана с крышкой

    // Родительский объект для предметов в руках (дочерний к камере)
    public Transform handPosition;

    // Текущие объекты в руках
    private GameObject currentCupInHand;
    private GameObject currentLidInHand;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void ChangeState(OrderState newState)
    {
        currentState = newState;
        Debug.Log("State changed to: " + newState);

        // Обновляем визуал предметов в руках
        UpdateHandVisuals();
    }
    void UpdateHandVisuals()
    {
        // Удаляем старые предметы в руках, КРОМЕ случаев когда переходим В CupInMachine
        // (потому что в CupInMachine стакан уже в машине, а не в руках)
        if (currentState != OrderState.CupInMachine)
        {
            if (currentCupInHand != null) Destroy(currentCupInHand);
            if (currentLidInHand != null) Destroy(currentLidInHand);
        }
        else
        {
            // Если переходим В состояние CupInMachine - все равно удаляем крышку из рук
            if (currentLidInHand != null) Destroy(currentLidInHand);
        }

        // Остальной код без изменений...
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
                // В этом состоянии стакан в кофемашине, ничего не создаем в руках
                break;
        }
    }

    // Метод для сброса трансформа объекта к нулевым значениям
    void ResetObjectTransform(GameObject obj)
    {
        if (obj != null)
        {
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }
    }

    // Метод для получения текущего стакана в руках (для кофемашины)
    public GameObject GetCurrentCup()
    {
        return currentCupInHand;
    }

    // Метод для очистки стакана в руках (после передачи в кофемашину)
    public void ClearCurrentCup()
    {
        currentCupInHand = null;
    }

    // Получить префаб закрытого кофе
    public GameObject GetSealedCoffeePrefab()
    {
        return sealedCoffeePrefab;
    }
}