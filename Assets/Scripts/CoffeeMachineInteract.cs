using System.Collections;
using TMPro;
using UnityEngine;

public class CoffeeMachineInteract : InteractableObject
{
    public float brewTime = 3f;
    public Transform cupPlacePosition; 
    private AudioSource audioSource;
    private bool isBrewing = false;
    private GameObject placedCup; 
    private bool isCoffeeSealed = false; 

    [Header("Подсказка")]
    public GameObject hintPanel;
    public TMP_Text hintText;
    public string hintMessage = "Нажмите ЛКМ, чтобы кинуть  в недоброжелательного клиента ";
    public float hintDisplayTime = 5f;
    private Coroutine hintCoroutine;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Если позиция не назначена - создаем ее автоматически
        if (cupPlacePosition == null)
        {
            GameObject cupPos = new GameObject("CupPosition");
            cupPos.transform.SetParent(transform);
            cupPos.transform.localPosition = new Vector3(0, 0.1f, 0.2f);
            cupPlacePosition = cupPos.transform;
            Debug.Log("Автоматически создана позиция для стакана");
        }
    }

    public override void Interact()
    {
        Debug.Log("=== ВЗАИМОДЕЙСТВИЕ С КОФЕМАШИНОЙ ===");
        Debug.Log("Состояние: " + CoffeeOrderManager.Instance.currentState);
        Debug.Log("Напивается: " + isBrewing);
        Debug.Log("Закрыт крышкой: " + isCoffeeSealed);
        Debug.Log("Стакан в машине: " + (placedCup != null));

        if (isBrewing)
        {
            Debug.Log("Кофе еще наливается!");
            return;
        }

        // Если в руках крышка И есть стакан в машине - закрываем крышкой (ВЫСШИЙ ПРИОРИТЕТ)
        if (CoffeeOrderManager.Instance.currentState == CoffeeOrderManager.OrderState.HasLid &&
            placedCup != null && !isCoffeeSealed)
        {
            Debug.Log("Закрываем кофе крышкой...");
            SealCoffeeWithLid();
            return; // Важно: return после успешного действия
        }
        // Если в руках пустой стакан - начинаем наливание
        else if (CoffeeOrderManager.Instance.currentState == CoffeeOrderManager.OrderState.HasEmptyCup)
        {
            Debug.Log("Начинаем наливание кофе...");
            PlaceCupInMachine();
            StartBrewing();
        }
        // Если кофе готов в машине - забираем
        else if (placedCup != null && !isBrewing)
        {
            Debug.Log("Забираем готовый кофе...");
            TakeCoffee();
        }
        else
        {
            Debug.Log("Не могу взаимодействовать. Состояние: " + CoffeeOrderManager.Instance.currentState);
        }
    }

    void OnDrawGizmos()
    {
        // Рисуем красную сферу в месте где должен стоять стакан
        if (cupPlacePosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(cupPlacePosition.position, 0.05f);
        }
        else
        {
            // Если cupPlacePosition не назначен, рисуем сферу на позиции кофемашины
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, 0.05f);
        }
    }

    void PlaceCupInMachine()
    {
        // Получаем стакан из рук
        GameObject cupFromHand = CoffeeOrderManager.Instance.GetCurrentCup();

        if (cupFromHand != null && cupPlacePosition != null)
        {
            Debug.Log("Найден стакан в руках: " + cupFromHand.name);

            // Сначала меняем родителя
            cupFromHand.transform.SetParent(cupPlacePosition);

            // ПОЛНОСТЬЮ сбрасываем трансформацию
            cupFromHand.transform.localPosition = Vector3.zero;
            cupFromHand.transform.localRotation = Quaternion.identity;
            cupFromHand.transform.localScale = Vector3.one;

            placedCup = cupFromHand;
            isCoffeeSealed = false;

            // Очищаем ссылку на стакан в руках
            CoffeeOrderManager.Instance.ClearCurrentCup();

            Debug.Log("Поставил стакан в кофемашину на позицию: " + cupPlacePosition.position);
        }
        else
        {
            Debug.LogError("Не могу поставить стакан: cupFromHand=" + cupFromHand + ", cupPlacePosition=" + cupPlacePosition);
        }
    }

    void StartBrewing()
    {
        isBrewing = true;
        CoffeeOrderManager.Instance.ChangeState(CoffeeOrderManager.OrderState.CupInMachine);

        AudioManager.Instance.PlayCoffeeBrewSound();

        Debug.Log("Кофе наливается... Ждем " + brewTime + " секунд");

        // Запускаем таймер готовности
        Invoke("FinishBrewing", brewTime);
    }

    void FinishBrewing()
    {
        isBrewing = false;

        // Меняем стакан на наполненный
        if (placedCup != null)
        {
            Debug.Log("Заменяем пустой стакан на наполненный...");

            // Запоминаем текущего родителя
            Transform currentParent = placedCup.transform.parent;

            Destroy(placedCup);

            // Создаем наполненный стакан на том же месте
            if (CoffeeOrderManager.Instance.filledCupPrefab != null && currentParent != null)
            {
                placedCup = Instantiate(CoffeeOrderManager.Instance.filledCupPrefab, currentParent);
                placedCup.transform.localPosition = Vector3.zero;
                placedCup.transform.localRotation = Quaternion.identity;
                placedCup.transform.localScale = Vector3.one;
                Debug.Log("Создан наполненный стакан");
            }
        }

        Debug.Log("Кофе готов! Можно закрыть крышкой или забрать");
    }

    void SealCoffeeWithLid()
    {
        if (placedCup != null && !isCoffeeSealed)
        {
            GameObject sealedPrefab = CoffeeOrderManager.Instance.GetSealedCoffeePrefab();
            if (sealedPrefab != null)
            {
                Debug.Log("Закрываем кофе крышкой...");

                // Запоминаем текущего родителя
                Transform currentParent = placedCup.transform.parent;

                // Уничтожаем текущий стакан
                Destroy(placedCup);

                // Создаем закрытый стакан на том же месте
                placedCup = Instantiate(sealedPrefab, currentParent);
                placedCup.transform.localPosition = Vector3.zero;
                placedCup.transform.localRotation = Quaternion.identity;
                placedCup.transform.localScale = Vector3.one;

                isCoffeeSealed = true;

                CoffeeOrderManager.Instance.ChangeState(CoffeeOrderManager.OrderState.CupInMachine);
                ShowHint();
                Debug.Log("Кофе закрыто крышкой! Теперь можно забирать");
            }
            else
            {
                Debug.LogError("Префаб закрытого кофе не назначен в CoffeeOrderManager!");
            }
        }
        else
        {
            Debug.LogError("Не могу закрыть крышкой: placedCup=" + placedCup + ", isCoffeeSealed=" + isCoffeeSealed);
        }
    }
    void ShowHint()
    {
        if (hintPanel != null && hintText != null)
        {
            hintText.text = hintMessage;
            hintPanel.SetActive(true);
            Debug.Log("Показана подсказка: " + hintMessage);

            // Запускаем таймер скрытия подсказки
            if (hintCoroutine != null)
            {
                StopCoroutine(hintCoroutine);
            }
            hintCoroutine = StartCoroutine(HideHintAfterDelay());
        }
    }
    IEnumerator HideHintAfterDelay()
    {
        yield return new WaitForSeconds(hintDisplayTime);
        HideHint();
    }

    void HideHint()
    {
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
            Debug.Log("Подсказка скрыта");
        }

        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            hintCoroutine = null;
        }
    }

    void TakeCoffee()
    {
        // Возвращаем стакан в руку
        if (placedCup != null)
        {
            placedCup.transform.SetParent(CoffeeOrderManager.Instance.handPosition);
            placedCup.transform.localPosition = Vector3.zero;
            placedCup.transform.localRotation = Quaternion.identity;
            placedCup.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            // Обновляем состояние в зависимости от того, закрыт ли стакан
            if (isCoffeeSealed)
            {
                CoffeeOrderManager.Instance.ChangeState(CoffeeOrderManager.OrderState.CoffeeReady);
            }
            else
            {
                CoffeeOrderManager.Instance.ChangeState(CoffeeOrderManager.OrderState.HasFilledCup);
            }

            placedCup = null;
            isCoffeeSealed = false;
            Debug.Log("Забрал кофе из машины!");
        }
        else
        {
            Debug.LogError("Нет стакана для забора!");
        }
    }
}