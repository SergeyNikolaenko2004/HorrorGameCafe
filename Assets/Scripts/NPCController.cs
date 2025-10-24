using TMPro;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class NPCController : MonoBehaviour
{
    [Header("��������� NPC")]
    public Transform goal; // ����� ����������

    [Header("UI � ����")]
    public GameObject dialoguePanel; // ������ �� UI ������
    public AudioClip dialogueSound; // ���� �������
    public TMP_Text dialogueText; // ����� ��� �������
    public string[] dialogueLines; // ������ ����� �������
    public float textSpeed = 0.05f; // �������� ��������� ������
    public float timeBetweenLines = 2f; // ����� ����� ��������

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
        // �������� ����������
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // ���� AudioSource ��� - ���������
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // ��������� ������� Animator
        if (animator == null)
        {
            Debug.LogError("Animator �� ������ �� NPC!");
            return;
        }

        // �������� ������ ������� � ������
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
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
        if (isMoving && agent != null && !hasReached)
        {
            // ���������, ������ �� NPC ����
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                OnReachedDestination();
            }
        }
    }

    void OnReachedDestination()
    {
        // ������������� �������� ������
        animator.SetBool("isWalking", false);
        isMoving = false;
        hasReached = true;

        Debug.Log("NPC ������ ���� � �����������");

        // ��������� ������
        StartDialogue();
    }

    void StartDialogue()
    {
        // ���������� ������ �������
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
        }

        // ������������� ����
        if (dialogueSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(dialogueSound);
        }

        // ��������� �������������� ������
        dialogueCoroutine = StartCoroutine(AutoDialogue());
    }

    IEnumerator AutoDialogue()
    {
        // �������� �� ���� ������� �������
        for (currentLine = 0; currentLine < dialogueLines.Length; currentLine++)
        {
            // �������� ������� ������
            yield return StartCoroutine(TypeText(dialogueLines[currentLine]));

            // ���� ����� ��������� ������� (����� ���������)
            if (currentLine < dialogueLines.Length - 1)
            {
                yield return new WaitForSeconds(timeBetweenLines);
            }
        }

        // ��������� ������ ����� ��������� �����
        yield return new WaitForSeconds(1f);
        EndDialogue();
    }

    IEnumerator TypeText(string line)
    {
        isTyping = true;
        dialogueText.text = "";

        Debug.Log($"���������� ������ {currentLine + 1}: {line}");

        // ���������� ������� �����
        foreach (char letter in line.ToCharArray())
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(textSpeed);
        }

        isTyping = false;
        Debug.Log($"������ {currentLine + 1} ���������");
    }

    void EndDialogue()
    {
        // ������������� �������� ���� ��� ��� ��������
        if (dialogueCoroutine != null)
        {
            StopCoroutine(dialogueCoroutine);
        }

        // �������� ������ �������
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }

        Debug.Log("������ ��������");

        // ����� ����� �������� ������� � ��������� ����� ����
        // ��������: StartCoroutine(StartChaseScene());
    }
}