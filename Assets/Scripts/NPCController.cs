using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    // ��������� ����� ����������
    public Transform goal;

    private NavMeshAgent agent;
    private Animator animator;
    private bool isMoving = false;

    void Start()
    {
        // �������� ����������
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // ��������� ������� Animator
        if (animator == null)
        {
            Debug.LogError("Animator �� ������ �� NPC!");
            return;
        }

        // ��������� ����� ����������
        agent.destination = goal.position;

        // �������� �������� ������
        animator.SetBool("isWalking", true);
        isMoving = true;

        Debug.Log("NPC ����� �������� � ��������� ������");
    }

    void Update()
    {
        // ���� NPC ��������, ��������� ���������� ����
        if (isMoving && agent != null)
        {
            // ���������, ������ �� NPC ����
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                // ������������� �������� ������
                animator.SetBool("isWalking", false);
                isMoving = false;

                Debug.Log("NPC ������ ���� � �����������");
            }
        }
    }
}