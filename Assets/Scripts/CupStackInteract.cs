using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CupStackInteract : InteractableObject
{
    public override void Interact()
    {
        // ����� ����� ������ ������ ���� � ��� ��� ������� � �� �� � �������� ���������
        if (CoffeeOrderManager.Instance.currentState == CoffeeOrderManager.OrderState.CanTakeCup ||
            CoffeeOrderManager.Instance.currentState == CoffeeOrderManager.OrderState.HasLid)
        {
            // ����� ���������
            CoffeeOrderManager.Instance.ChangeState(CoffeeOrderManager.OrderState.HasEmptyCup);
            Debug.Log("���� ��������� �� ������!");
        }
        else
        {
            Debug.Log("��� ���� ��������� ��� ���� ����������!");
        }
    }
}
