using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoffeeOrderManager : MonoBehaviour
{
    public static CoffeeOrderManager Instance;

    public enum OrderState
    {
        CanTakeCup,     // ����� ����� ������
        HasEmptyCup,    // � ����� ������ ������
        CupInMachine,   // ������ � ���������� (����������)
        HasFilledCup,   // � ����� ����������� ������
        HasLid,         // � ����� ������
        CoffeeReady     // ���� ��������� �����
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
        if (Instance == null)
        {
            Instance = this;
            // �� ���������� DontDestroyOnLoad - ����� �������� �������������� �� ������
        }
        else
        {
            // ���� ��� ���� ��������� - ���������� �����
            Destroy(gameObject);
            return;
        }

        ResetManager();
    }

    void Start()
    {
        // �������������� ������������� ����� Awake
        ResetManager();
    }

    void Update()
    {
        if (currentState == OrderState.CoffeeReady && Input.GetMouseButtonDown(0))
        {
            ThrowCoffee();
        }
    }

    // ����� ��� ������� ������ ���������
    public void ResetManager()
    {
        currentState = OrderState.CanTakeCup;
        ClearHandPosition();
        Debug.Log("CoffeeOrderManager ������� � ��������� ���������");
    }

    public void ChangeState(OrderState newState)
    {
        currentState = newState;
        UpdateHandVisuals();
    }

    void UpdateHandVisuals()
    {
        // ������� ������� ������� � ����
        if (currentState != OrderState.CupInMachine)
        {
            if (currentCupInHand != null)
            {
                Destroy(currentCupInHand);
                currentCupInHand = null;
            }
            if (currentLidInHand != null)
            {
                Destroy(currentLidInHand);
                currentLidInHand = null;
            }
        }
        else
        {
            if (currentLidInHand != null)
            {
                Destroy(currentLidInHand);
                currentLidInHand = null;
            }
        }

        // ������� ����� ������� � ����������� �� ���������
        switch (currentState)
        {
            case OrderState.HasEmptyCup:
                if (emptyCupPrefab != null && handPosition != null)
                {
                    currentCupInHand = Instantiate(emptyCupPrefab, handPosition);
                    ResetObjectTransform(currentCupInHand);
                    Debug.Log("������ ������ ������ � ����");
                }
                break;

            case OrderState.HasFilledCup:
                if (filledCupPrefab != null && handPosition != null)
                {
                    currentCupInHand = Instantiate(filledCupPrefab, handPosition);
                    ResetObjectTransform(currentCupInHand);
                    Debug.Log("������ ����������� ������ � ����");
                }
                break;

            case OrderState.HasLid:
                if (lidPrefab != null && handPosition != null)
                {
                    currentLidInHand = Instantiate(lidPrefab, handPosition);
                    ResetObjectTransform(currentLidInHand);
                    Debug.Log("������� ������ � ����");
                }
                break;

            case OrderState.CoffeeReady:
                if (sealedCoffeePrefab != null && handPosition != null)
                {
                    currentCupInHand = Instantiate(sealedCoffeePrefab, handPosition);
                    ResetObjectTransform(currentCupInHand);
                    Debug.Log("������ ������� ���� � ����");
                }
                break;

            case OrderState.CupInMachine:
                Debug.Log("������ � ���������� - ���� ������");
                break;

            case OrderState.CanTakeCup:
                Debug.Log("����� ����� ������ - ���� ������");
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

    // ������ ���� � NPC
    void ThrowCoffee()
    {
        if (currentCupInHand != null && targetNPC != null)
        {
            Debug.Log("������� ���� � NPC!");
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
            currentState = OrderState.CanTakeCup;
            Debug.Log("��������� �������� ��: " + currentState);

            targetNPC.ThrowCoffeeAtNPC();
            Destroy(thrownCoffee, 3f);

            Debug.Log("���� �������! ���� ������.");
        }
        else
        {
            Debug.LogWarning("�� ���� ������� ����: " +
                (currentCupInHand == null ? "���� ��� � �����" : "NPC �� ��������"));
        }
    }

    // ����� ��� ������ ������� handPosition �� ���� �������� ��������
    void ClearHandPosition()
    {
        if (handPosition != null)
        {
            // ���������� ��� �������� �������
            foreach (Transform child in handPosition)
            {
                Destroy(child.gameObject);
            }
            Debug.Log("HandPosition ������ �� ���� �������� ��������");
        }

        // ����� ������� ������
        currentCupInHand = null;
        currentLidInHand = null;
    }

    // ����� ��� ��������������� ������ ��� �������� �����
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ��� �������� ����� ����� ���������� ��������
        ResetManager();
    }
}