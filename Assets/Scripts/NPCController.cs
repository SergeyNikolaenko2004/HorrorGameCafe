using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
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
    public float timeBetweenLines = 2f; // ����� ����� �����

    [Header("���������")]
    public GameObject hintPanel; // ������ ���������
    public TMP_Text hintText; // ����� ���������
    public string hintMessage = "������� E ����� ����� ������";
    public float hintDisplayTime = 5f; // ����� ������ ���������

    [Header("����� ������")]
    public GameObject deathPanel; // ������ "�� ������"
    public TMP_Text deathText; // ����� �� ������ ������
    public string deathMessage = "�� ������";
    public float deathScreenTime = 3f; // ����� ������ ������ ������
    public string mainMenuSceneName = "MainMenu"; // �������� ����� �������� ����

    [Header("������")]
    public float chaseSpeed = 5f; // �������� ������
    public float reactionTime = 1f; // ����� ������� ����� �������
    public float catchDistance = 2f; // ��������� �� ������� NPC ����� ������

    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    private bool isMoving = false;
    private bool hasReached = false;
    private bool isChasing = false;
    private bool isPlayerDead = false;
    private int currentLine = 0;
    private bool isTyping = false;
    private Coroutine dialogueCoroutine;
    private Coroutine hintCoroutine;
    private Transform player;

    void Start()
    {
        // �������� ����������
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // ���� ������
        FindPlayer();

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

        // �������� ��� ������ � ������
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(false);
        }
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
        }
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }

        // ��������� ��������� �� ����
        if (goal == null)
        {
            Debug.LogError("Goal �� �������� � ����������!");
            return;
        }

        // ��������� ����� ����������
        agent.destination = goal.position;

        // �������� �������� ������
        animator.SetBool("isWalking", true);
        isMoving = true;

        Debug.Log("NPC ����� �������� � ��������� ������");
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("����� ������: " + player.name);
        }
        else
        {
            Debug.LogError("����� �� ������! ��������� ��� ���� ������ � ����� 'Player'");
        }
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

        // ���� ���� ������ - ���������� ������
        if (isChasing && !isPlayerDead)
        {
            if (player == null)
            {
                FindPlayer();
                return;
            }

            agent.destination = player.position;

            // ���������, ������ �� NPC ������
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= catchDistance)
            {
                Debug.Log($"NPC ������ ������! ���������: {distance}");
                CatchPlayer();
            }
            else
            {
                // ���������� ���������� � ���������
                if (Time.frameCount % 60 == 0) // ������ �������
                {
                    Debug.Log($"��������� �� ������: {distance}");
                }
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
        if (dialogueSound != null)
        {
            AudioManager.Instance.PlayDialogueSound();
        }

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

        // ���������� ��������� ����� �������
        ShowHint();
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
    }

    // �������� ���������
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

    // ������ ���������
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

    // NPC ������ ������
    void CatchPlayer()
    {
        if (!isPlayerDead)
        {
            isPlayerDead = true;
            Debug.Log("NPC ������ ������! ����� ����.");

            // ������������� ������
            isChasing = false;
            agent.isStopped = true;

            // ��������� �������� ����� ���� ����
            if (animator != null)
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", false);
                animator.SetTrigger("Attack");
            }

            // ���������� ����� ������
            ShowDeathScreen();
        }
    }

    // �������� ����� ������
    void ShowDeathScreen()
    {
        if (deathPanel != null && deathText != null)
        {
            deathText.text = deathMessage;
            deathPanel.SetActive(true);
            Debug.Log("������� ����� ������");

            // ��������� ���������� �������
            DisablePlayer();

            // ��������� ������� � ������� ����
            StartCoroutine(ReturnToMainMenu());
        }
        else
        {
            Debug.LogError("DeathPanel ��� deathText �� ��������� � ����������!");
            // ���� ������ �� ���������, ��� ����� ��������� � ����
            StartCoroutine(ReturnToMainMenu());
        }
    }

    // ��������� ���������� �������
    void DisablePlayer()
    {
        if (player == null) return;

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("���������� ������� ���������");
        }
        else
        {
            Debug.LogWarning("PlayerController �� ������ �� ������");
        }

        // ����� ��������� ������ ��������� ���������� ����������
        MonoBehaviour[] playerComponents = player.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour component in playerComponents)
        {
            if (component != this && component.enabled)
            {
                component.enabled = false;
                Debug.Log($"�������� ���������: {component.GetType().Name}");
            }
        }

        // ��������� ������
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // ��������� � ������� ����
    // ��������� � ������� ����
    IEnumerator ReturnToMainMenu()
    {
        Debug.Log($"����� {deathScreenTime} ������ ������� �: {mainMenuSceneName}");

        // ���� ����� ���������
        yield return new WaitForSeconds(deathScreenTime);

        // ���������� ��� ����� ����� ���������
        StopAllAudio();

        // ��������� ���������� �� �����
        if (Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            // ��������� ������� ����
            SceneManager.LoadScene(mainMenuSceneName);
            Debug.Log("������� � ������� ����: " + mainMenuSceneName);
        }
        else
        {
            Debug.LogError($"����� {mainMenuSceneName} �� �������! ��������� �����:");
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                Debug.Log($"- {sceneName}");
            }
        }
    }

    // ���������� ��� �����
    void StopAllAudio()
    {
        // ������������� AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllSounds();
            Debug.Log("��� ����� ����������� ����� AudioManager");
        }

        // ������������� ��� ����� ��������� � �����
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audioSource in allAudioSources)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        Debug.Log($"����������� {allAudioSources.Length} ����� ����������");

        // ����� ������������� ��� ������� ������
        if (SimpleFearEffectsManager.Instance != null)
        {
            SimpleFearEffectsManager.Instance.StopChaseEffects();
            Debug.Log("������� ������ �����������");
        }
    }

    // ����� ��� ������ ���� � NPC (���������� �� CoffeeOrderManager)
    public void ThrowCoffeeAtNPC()
    {
        if (!isChasing)
        {
            Debug.Log("���� ������� � NPC! ���������� ������...");

            // �������� ��������� ���� ��� �������
            HideHint();

            StartCoroutine(StartChase());
        }
    }

    IEnumerator StartChase()
    {
        Debug.Log("NPC � ������! ������ ���������� ����� " + reactionTime + " ������");

        AudioManager.Instance.StartChase();
        SimpleFearEffectsManager.Instance.StartChaseEffects();

        // ���� ����� �������
        yield return new WaitForSeconds(reactionTime);

        // �������� ������
        isChasing = true;
        agent.speed = chaseSpeed;

        if (animator != null)
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isRunning", true);
        }

        Debug.Log("������ ��������! NPC ����� �� �������!");
    }

    // �������������� ������ ��� ���������� ���������� �����
    public void ShowCustomHint(string message, float displayTime = 5f)
    {
        if (hintPanel != null && hintText != null)
        {
            hintText.text = message;
            hintPanel.SetActive(true);

            if (hintCoroutine != null)
            {
                StopCoroutine(hintCoroutine);
            }
            hintCoroutine = StartCoroutine(HideHintAfterCustomDelay(displayTime));
        }
    }

    IEnumerator HideHintAfterCustomDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideHint();
    }

    // ��� ������� � ���������
    void OnDrawGizmosSelected()
    {
        // ������ ����� ������� catchDistance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchDistance);
    }
}