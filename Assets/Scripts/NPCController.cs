using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    // Положение точки назначения
    public Transform goal;

    private NavMeshAgent agent;
    private Animator animator;
    private bool isMoving = false;

    void Start()
    {
        // Получаем компоненты
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Проверяем наличие Animator
        if (animator == null)
        {
            Debug.LogError("Animator не найден на NPC!");
            return;
        }

        // Указываем точку назначения
        agent.destination = goal.position;

        // Включаем анимацию ходьбы
        animator.SetBool("isWalking", true);
        isMoving = true;

        Debug.Log("NPC начал движение с анимацией ходьбы");
    }

    void Update()
    {
        // Если NPC движется, проверяем достижение цели
        if (isMoving && agent != null)
        {
            // Проверяем, достиг ли NPC цели
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                // Останавливаем анимацию ходьбы
                animator.SetBool("isWalking", false);
                isMoving = false;

                Debug.Log("NPC достиг цели и остановился");
            }
        }
    }
}