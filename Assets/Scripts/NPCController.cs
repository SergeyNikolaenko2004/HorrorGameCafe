using TMPro;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPCController : MonoBehaviour
{
    [Header("Настройки NPC")]
    public Transform goal; // Точка назначения

    [Header("UI и звук")]
    public GameObject dialoguePanel; // Ссылка на UI панель
    public AudioClip dialogueSound; // Звук диалога
    public TMP_Text dialogueText; // Текст для диалога
    public string[] dialogueLines; // Массив строк диалога
    public float textSpeed = 0.05f; // Скорость появления текста
    public float timeBetweenLines = 2f; // Время между строками

    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    private bool isMoving = false;
    private bool hasReached = false;
    private int currentLine = 0;
    private bool isTyping = false;
    private Coroutine dialogueCoroutine;

    void Start()
    {
        // Получаем компоненты
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Если AudioSource нет - добавляем
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Проверяем наличие Animator
        if (animator == null)
        {
            Debug.LogError("Animator не найден на NPC!");
            return;
        }

        // Скрываем панель диалога в начале
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
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
        if (isMoving && agent != null && !hasReached)
        {
            // Проверяем, достиг ли NPC цели
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                OnReachedDestination();
            }
        }
    }

    void OnReachedDestination()
    {
        // Останавливаем анимацию ходьбы
        animator.SetBool("isWalking", false);
        isMoving = false;
        hasReached = true;

        Debug.Log("NPC достиг цели и остановился");

        // Запускаем диалог
        StartDialogue();
    }

    void StartDialogue()
    {
        // Показываем панель диалога
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        // Воспроизводим звук
        if (dialogueSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dialogueSound);
        }

        // Запускаем автоматический диалог
        dialogueCoroutine = StartCoroutine(AutoDialogue());
    }

    IEnumerator AutoDialogue()
    {
        // Проходим по всем строкам диалога
        for (currentLine = 0; currentLine < dialogueLines.Length; currentLine++)
        {
            // Печатаем текущую строку
            yield return StartCoroutine(TypeText(dialogueLines[currentLine]));

            // Ждем перед следующей строкой (кроме последней)
            if (currentLine < dialogueLines.Length - 1)
            {
                yield return new WaitForSeconds(timeBetweenLines);
            }
        }

        // Завершаем диалог после небольшой паузы
        yield return new WaitForSeconds(1f);
        EndDialogue();
    }

    IEnumerator TypeText(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        Debug.Log($"Показываем строку {currentLine + 1}: {line}");

        // Постепенно выводим текст
        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
        Debug.Log($"Строка {currentLine + 1} завершена");
    }

    void EndDialogue()
    {
        // Останавливаем корутину если она еще работает
        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
        }

        // Скрываем панель диалога
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        Debug.Log("Диалог завершен");

        // Здесь можно добавить переход к следующей части игры
        // Например: StartCoroutine(StartChaseScene());
    }
}