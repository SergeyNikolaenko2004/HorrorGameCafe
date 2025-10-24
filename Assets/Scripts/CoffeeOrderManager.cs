using UnityEngine;

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

    // ������� ��� ������������ � �����
    public GameObject emptyCupPrefab;
    public GameObject filledCupPrefab;
    public GameObject lidPrefab;
    public GameObject sealedCoffeePrefab; // ������ ��������� ������� � �������

    // ������������ ������ ��� ��������� � ����� (�������� � ������)
    public Transform handPosition;

    // ������� ������� � �����
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

        // ��������� ������ ��������� � �����
        UpdateHandVisuals();
    }
    void UpdateHandVisuals()
    {
        // ������� ������ �������� � �����, ����� ������� ����� ��������� � CupInMachine
        // (������ ��� � CupInMachine ������ ��� � ������, � �� � �����)
        if (currentState != OrderState.CupInMachine)
        {
            if (currentCupInHand != null) Destroy(currentCupInHand);
            if (currentLidInHand != null) Destroy(currentLidInHand);
        }
        else
        {
            // ���� ��������� � ��������� CupInMachine - ��� ����� ������� ������ �� ���
            if (currentLidInHand != null) Destroy(currentLidInHand);
        }

        // ��������� ��� ��� ���������...
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
                // � ���� ��������� ������ � ����������, ������ �� ������� � �����
                break;
        }
    }

    // ����� ��� ������ ���������� ������� � ������� ���������
    void ResetObjectTransform(GameObject obj)
    {
        if (obj != null)
        {
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }
    }

    // ����� ��� ��������� �������� ������� � ����� (��� ����������)
    public GameObject GetCurrentCup()
    {
        return currentCupInHand;
    }

    // ����� ��� ������� ������� � ����� (����� �������� � ����������)
    public void ClearCurrentCup()
    {
        currentCupInHand = null;
    }

    // �������� ������ ��������� ����
    public GameObject GetSealedCoffeePrefab()
    {
        return sealedCoffeePrefab;
    }
}