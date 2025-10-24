using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LidStackInteract : InteractableObject
{
    public override void Interact()
    {
        if (CoffeeOrderManager.Instance.currentState == CoffeeOrderManager.OrderState.HasFilledCup ||
            CoffeeOrderManager.Instance.currentState == CoffeeOrderManager.OrderState.CupInMachine ||
            CoffeeOrderManager.Instance.currentState == CoffeeOrderManager.OrderState.CanTakeCup)
        {
            CoffeeOrderManager.Instance.ChangeState(CoffeeOrderManager.OrderState.HasLid);
            Debug.Log("���� ������!");
        }
        else
        {
            Debug.Log("�� ���� ����� ������. ������� ���������: " + CoffeeOrderManager.Instance.currentState);
        }
    }
}