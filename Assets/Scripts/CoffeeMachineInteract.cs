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

    [Header("���������")]
    public GameObject hintPanel;
    public TMP_Text hintText;
    public string hintMessage = "������� ���, ����� ������  � ������������������� ������� ";
    public float hintDisplayTime = 5f;
    private Coroutine hintCoroutine;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // ���� ������� �� ��������� - ������� �� �������������
        if (cupPlacePosition == null)
        {
            GameObject cupPos = new GameObject("CupPosition");
            cupPos.transform.SetParent(transform);
            cupPos.transform.localPosition = new Vector3(0, 0.1f, 0.2f);
            cupPlacePosition = cupPos.transform;
            Debug.Log("������������� ������� ������� ��� �������");
        }
    }

    public override void Interact()
    {
        Debug.Log("=== �������������� � ����������� ===");
        Debug.Log("���������: " + CoffeeOrderManager.Instance.currentState);
        Debug.Log("����������: " + isBrewing);
        Debug.Log("������ �������: " + isCoffeeSealed);
        Debug.Log("������ � ������: " + (placedCup != null));

        if (isBrewing)
        {
            Debug.Log("���� ��� ����������!");
            return;
        }

        // ���� � ����� ������ � ���� ������ � ������ - ��������� ������� (������ ���������)
        if (CoffeeOrderManager.Instance.currentState == CoffeeOrderManager.OrderState.HasLid &&
            placedCup != null && !isCoffeeSealed)
        {
            Debug.Log("��������� ���� �������...");
            SealCoffeeWithLid();
            return; // �����: return ����� ��������� ��������
        }
        // ���� � ����� ������ ������ - �������� ���������
        else if (CoffeeOrderManager.Instance.currentState == CoffeeOrderManager.OrderState.HasEmptyCup)
        {
            Debug.Log("�������� ��������� ����...");
            PlaceCupInMachine();
            StartBrewing();
        }
        // ���� ���� ����� � ������ - ��������
        else if (placedCup != null && !isBrewing)
        {
            Debug.Log("�������� ������� ����...");
            TakeCoffee();
        }
        else
        {
            Debug.Log("�� ���� �����������������. ���������: " + CoffeeOrderManager.Instance.currentState);
        }
    }

    void OnDrawGizmos()
    {
        // ������ ������� ����� � ����� ��� ������ ������ ������
        if (cupPlacePosition != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(cupPlacePosition.position, 0.05f);
        }
        else
        {
            // ���� cupPlacePosition �� ��������, ������ ����� �� ������� ����������
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(transform.position, 0.05f);
        }
    }

    void PlaceCupInMachine()
    {
        // �������� ������ �� ���
        GameObject cupFromHand = CoffeeOrderManager.Instance.GetCurrentCup();

        if (cupFromHand != null && cupPlacePosition != null)
        {
            Debug.Log("������ ������ � �����: " + cupFromHand.name);

            // ������� ������ ��������
            cupFromHand.transform.SetParent(cupPlacePosition);

            // ��������� ���������� �������������
            cupFromHand.transform.localPosition = Vector3.zero;
            cupFromHand.transform.localRotation = Quaternion.identity;
            cupFromHand.transform.localScale = Vector3.one;

            placedCup = cupFromHand;
            isCoffeeSealed = false;

            // ������� ������ �� ������ � �����
            CoffeeOrderManager.Instance.ClearCurrentCup();

            Debug.Log("�������� ������ � ���������� �� �������: " + cupPlacePosition.position);
        }
        else
        {
            Debug.LogError("�� ���� ��������� ������: cupFromHand=" + cupFromHand + ", cupPlacePosition=" + cupPlacePosition);
        }
    }

    void StartBrewing()
    {
        isBrewing = true;
        CoffeeOrderManager.Instance.ChangeState(CoffeeOrderManager.OrderState.CupInMachine);

        AudioManager.Instance.PlayCoffeeBrewSound();

        Debug.Log("���� ����������... ���� " + brewTime + " ������");

        // ��������� ������ ����������
        Invoke("FinishBrewing", brewTime);
    }

    void FinishBrewing()
    {
        isBrewing = false;

        // ������ ������ �� �����������
        if (placedCup != null)
        {
            Debug.Log("�������� ������ ������ �� �����������...");

            // ���������� �������� ��������
            Transform currentParent = placedCup.transform.parent;

            Destroy(placedCup);

            // ������� ����������� ������ �� ��� �� �����
            if (CoffeeOrderManager.Instance.filledCupPrefab != null && currentParent != null)
            {
                placedCup = Instantiate(CoffeeOrderManager.Instance.filledCupPrefab, currentParent);
                placedCup.transform.localPosition = Vector3.zero;
                placedCup.transform.localRotation = Quaternion.identity;
                placedCup.transform.localScale = Vector3.one;
                Debug.Log("������ ����������� ������");
            }
        }

        Debug.Log("���� �����! ����� ������� ������� ��� �������");
    }

    void SealCoffeeWithLid()
    {
        if (placedCup != null && !isCoffeeSealed)
        {
            GameObject sealedPrefab = CoffeeOrderManager.Instance.GetSealedCoffeePrefab();
            if (sealedPrefab != null)
            {
                Debug.Log("��������� ���� �������...");

                // ���������� �������� ��������
                Transform currentParent = placedCup.transform.parent;

                // ���������� ������� ������
                Destroy(placedCup);

                // ������� �������� ������ �� ��� �� �����
                placedCup = Instantiate(sealedPrefab, currentParent);
                placedCup.transform.localPosition = Vector3.zero;
                placedCup.transform.localRotation = Quaternion.identity;
                placedCup.transform.localScale = Vector3.one;

                isCoffeeSealed = true;

                CoffeeOrderManager.Instance.ChangeState(CoffeeOrderManager.OrderState.CupInMachine);
                ShowHint();
                Debug.Log("���� ������� �������! ������ ����� ��������");
            }
            else
            {
                Debug.LogError("������ ��������� ���� �� �������� � CoffeeOrderManager!");
            }
        }
        else
        {
            Debug.LogError("�� ���� ������� �������: placedCup=" + placedCup + ", isCoffeeSealed=" + isCoffeeSealed);
        }
    }
    void ShowHint()
    {
        if (hintPanel != null && hintText != null)
        {
            hintText.text = hintMessage;
            hintPanel.SetActive(true);
            Debug.Log("�������� ���������: " + hintMessage);

            // ��������� ������ ������� ���������
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
            Debug.Log("��������� ������");
        }

        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            hintCoroutine = null;
        }
    }

    void TakeCoffee()
    {
        // ���������� ������ � ����
        if (placedCup != null)
        {
            placedCup.transform.SetParent(CoffeeOrderManager.Instance.handPosition);
            placedCup.transform.localPosition = Vector3.zero;
            placedCup.transform.localRotation = Quaternion.identity;
            placedCup.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            // ��������� ��������� � ����������� �� ����, ������ �� ������
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
            Debug.Log("������ ���� �� ������!");
        }
        else
        {
            Debug.LogError("��� ������� ��� ������!");
        }
    }
}