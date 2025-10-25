using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
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
    public float timeBetweenLines = 2f; // Время между строк

    [Header("Подсказка")]
    public GameObject hintPanel; // Панель подсказки
    public TMP_Text hintText; // Текст подсказки
    public string hintMessage = "Нажмите E чтобы взять стакан";
    public float hintDisplayTime = 5f; // Время показа подсказки

    [Header("Экран смерти")]
    public GameObject deathPanel; // Панель "Вы мертвы"
    public TMP_Text deathText; // Текст на экране смерти
    public string deathMessage = "ВЫ МЕРТВЫ";
    public float deathScreenTime = 3f; // Время показа экрана смерти
    public string mainMenuSceneName = "MainMenu"; // Название сцены главного меню

    [Header("Погоня")]
    public float chaseSpeed = 5f; // Скорость погони
    public float reactionTime = 1f; // Время реакции перед погоней
    public float catchDistance = 2f; // Дистанция на которой NPC ловит игрока

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
        // Получаем компоненты
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        // Ищем игрока
        FindPlayer();

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

        // Скрываем все панели в начале
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

        // Проверяем назначена ли цель
        if (goal == null)
        {
            Debug.LogError("Goal не назначен в инспекторе!");
            return;
        }

        // Указываем точку назначения
        agent.destination = goal.position;

        // Включаем анимацию ходьбы
        animator.SetBool("isWalking", true);
        isMoving = true;

        Debug.Log("NPC начал движение с анимацией ходьбы");
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("Игрок найден: " + player.name);
        }
        else
        {
            Debug.LogError("Игрок не найден! Убедитесь что есть объект с тегом 'Player'");
        }
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

        // Если идет погоня - преследуем игрока
        if (isChasing && !isPlayerDead)
        {
            if (player == null)
            {
                FindPlayer();
                return;
            }

            agent.destination = player.position;

            // Проверяем, догнал ли NPC игрока
            float distance = Vector3.Distance(transform.position, player.position);
            if (distance <= catchDistance)
            {
                Debug.Log($"NPC догнал игрока! Дистанция: {distance}");
                CatchPlayer();
            }
            else
            {
                // Отладочная информация о дистанции
                if (Time.frameCount % 60 == 0) // Каждую секунду
                {
                    Debug.Log($"Дистанция до игрока: {distance}");
                }
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
        if (dialogueSound != null)
        {
            AudioManager.Instance.PlayDialogueSound();
        }

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

        // Показываем подсказку после диалога
        ShowHint();
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
    }

    // Показать подсказку
    void ShowHint()
    {
        if (hintPanel != null && hintText != null)
        {
            hintText.text = hintMessage;
            hintPanel.SetActive(true);
            Debug.Log("Показана подсказка: " + hintMessage);

            // Запускаем таймер скрытия подсказки
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

    // Скрыть подсказку
    void HideHint()
    {
        if (hintPanel != null)
        {
            hintPanel.SetActive(false);
            Debug.Log("Подсказка скрыта");
        }

        if (hintCoroutine != null)
        {
            StopCoroutine(hintCoroutine);
            hintCoroutine = null;
        }
    }

    // NPC догнал игрока
    void CatchPlayer()
    {
        if (!isPlayerDead)
        {
            isPlayerDead = true;
            Debug.Log("NPC догнал игрока! Игрок умер.");

            // Останавливаем погоню
            isChasing = false;
            agent.isStopped = true;

            // Запускаем анимацию атаки если есть
            if (animator != null)
            {
                animator.SetBool("isWalking", false);
                animator.SetBool("isRunning", false);
                animator.SetTrigger("Attack");
            }

            // Показываем экран смерти
            ShowDeathScreen();
        }
    }

    // Показать экран смерти
    void ShowDeathScreen()
    {
        if (deathPanel != null && deathText != null)
        {
            deathText.text = deathMessage;
            deathPanel.SetActive(true);
            Debug.Log("Показан экран смерти");

            // Отключаем управление игроком
            DisablePlayer();

            // Запускаем переход в главное меню
            StartCoroutine(ReturnToMainMenu());
        }
        else
        {
            Debug.LogError("DeathPanel или deathText не назначены в инспекторе!");
            // Если панель не назначена, все равно переходим в меню
            StartCoroutine(ReturnToMainMenu());
        }
    }

    // Отключить управление игроком
    void DisablePlayer()
    {
        if (player == null) return;

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
            Debug.Log("Управление игроком отключено");
        }
        else
        {
            Debug.LogWarning("PlayerController не найден на игроке");
        }

        // Также отключаем другие возможные компоненты управления
        MonoBehaviour[] playerComponents = player.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour component in playerComponents)
        {
            if (component != this && component.enabled)
            {
                component.enabled = false;
                Debug.Log($"Отключен компонент: {component.GetType().Name}");
            }
        }

        // Отключаем курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Вернуться в главное меню
    // Вернуться в главное меню
    IEnumerator ReturnToMainMenu()
    {
        Debug.Log($"Через {deathScreenTime} секунд переход в: {mainMenuSceneName}");

        // Ждем перед переходом
        yield return new WaitForSeconds(deathScreenTime);

        // ОСТАНОВИТЬ ВСЕ ЗВУКИ ПЕРЕД ПЕРЕХОДОМ
        StopAllAudio();

        // Проверяем существует ли сцена
        if (Application.CanStreamedLevelBeLoaded(mainMenuSceneName))
        {
            // Загружаем главное меню
            SceneManager.LoadScene(mainMenuSceneName);
            Debug.Log("Переход в главное меню: " + mainMenuSceneName);
        }
        else
        {
            Debug.LogError($"Сцена {mainMenuSceneName} не найдена! Доступные сцены:");
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string scenePath = SceneUtility.GetScenePathByBuildIndex(i);
                string sceneName = System.IO.Path.GetFileNameWithoutExtension(scenePath);
                Debug.Log($"- {sceneName}");
            }
        }
    }

    // Остановить все звуки
    void StopAllAudio()
    {
        // Останавливаем AudioManager
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopAllSounds();
            Debug.Log("Все звуки остановлены через AudioManager");
        }

        // Останавливаем все аудио источники в сцене
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        foreach (AudioSource audioSource in allAudioSources)
        {
            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        Debug.Log($"Остановлено {allAudioSources.Length} аудио источников");

        // Также останавливаем все эффекты страха
        if (SimpleFearEffectsManager.Instance != null)
        {
            SimpleFearEffectsManager.Instance.StopChaseEffects();
            Debug.Log("Эффекты страха остановлены");
        }
    }

    // Метод для броска кофе в NPC (вызывается из CoffeeOrderManager)
    public void ThrowCoffeeAtNPC()
    {
        if (!isChasing)
        {
            Debug.Log("Кофе брошено в NPC! Начинается погоня...");

            // Скрываем подсказку если она активна
            HideHint();

            StartCoroutine(StartChase());
        }
    }

    IEnumerator StartChase()
    {
        Debug.Log("NPC в ярости! Погоня начинается через " + reactionTime + " секунд");

        AudioManager.Instance.StartChase();
        SimpleFearEffectsManager.Instance.StartChaseEffects();

        // Ждем время реакции
        yield return new WaitForSeconds(reactionTime);

        // Начинаем погоню
        isChasing = true;
        agent.speed = chaseSpeed;

        if (animator != null)
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isRunning", true);
        }

        Debug.Log("ПОГОНЯ НАЧАЛАСЬ! NPC бежит за игроком!");
    }

    // Дополнительные методы для управления подсказкой извне
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

    // Для отладки в редакторе
    void OnDrawGizmosSelected()
    {
        // Рисуем сферу радиуса catchDistance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, catchDistance);
    }
}