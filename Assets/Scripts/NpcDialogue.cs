using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

/// <summary>
/// Диалог NPC: при приближении показывает реплики над персонажем с эффектом печатной машинки.
/// Фразы сменяются автоматически. Скип через VR-триггер или Space.
/// </summary>
public class NpcDialogue : MonoBehaviour
{
    [Header("Реплики")]
    [TextArea(2, 6)]
    public string[] phrases = new string[]
    {
        "Опилишь ты меня, Хозяйка, совсем...",
        "Видишь форму? Чувствуешь?\nА живинки в ней нет.",
        "Как у того Цветка... Не выходит.",
        "И следы твои волшебные, Серебряное, не помогают.\nЗапутал ты меня."
    };

    [Header("UI")]
    public GameObject dialogueCanvas;    // World Space Canvas над головой NPC
    public TextMeshProUGUI dialogueText; // Текст реплики

    [Header("Зона диалога")]
    public float triggerRadius = 2.5f;
    public Vector3 triggerOffset = new Vector3(0, 1f, 0);

    [Header("Печатная машинка")]
    public float charDelay = 0.05f;        // пауза между символами
    public float pauseAfterPhrase = 1.5f;  // пауза после полной фразы перед следующей

    [Header("VR Input (назначь в инспекторе)")]
    public InputActionReference skipActionLeft;
    public InputActionReference skipActionRight;

    private int currentPhrase;
    private bool isActive;
    private bool playerInRange;
    private Transform playerTransform;
    private Coroutine typingCoroutine;
    private bool isTyping;

    void Start()
    {
        if (dialogueCanvas == null)
            dialogueCanvas = GetComponentInChildren<Canvas>(true)?.gameObject;
        if (dialogueText == null)
            dialogueText = GetComponentInChildren<TextMeshProUGUI>(true);

        if (dialogueCanvas == null) { Debug.LogError("[NpcDialogue] Canvas не найден!"); return; }
        if (dialogueText == null)   { Debug.LogError("[NpcDialogue] TextMeshProUGUI не найден!"); return; }

        dialogueCanvas.SetActive(false);

        Camera cam = Camera.main ?? FindObjectOfType<Camera>();
        if (cam != null)
            playerTransform = cam.transform;
        else
            Debug.LogError("[NpcDialogue] Камера не найдена!");
    }

    void OnEnable()
    {
        Subscribe(skipActionLeft, OnSkip);
        Subscribe(skipActionRight, OnSkip);
    }

    void OnDisable()
    {
        Unsubscribe(skipActionLeft, OnSkip);
        Unsubscribe(skipActionRight, OnSkip);
    }

    void Update()
    {
        if (playerTransform == null) return;

        float dist = Vector3.Distance(transform.position + triggerOffset, playerTransform.position);
        bool inRange = dist <= triggerRadius;

        if (inRange && !playerInRange)
        {
            playerInRange = true;
            if (isActive)
                dialogueCanvas.SetActive(true); // продолжаем с места
            else
                StartDialogue();
        }
        else if (!inRange && playerInRange)
        {
            playerInRange = false;
            dialogueCanvas.SetActive(false); // скрываем, но не сбрасываем
        }

        if (!isActive) return;

        // Текст всегда смотрит на игрока
        Vector3 dir = dialogueCanvas.transform.position - playerTransform.position;
        dialogueCanvas.transform.rotation = Quaternion.LookRotation(dir);

        // Скип с клавиатуры (для теста без VR)
        if (Input.GetKeyDown(KeyCode.Space))
            OnSkipPressed();
    }

    void OnSkip(InputAction.CallbackContext ctx)
    {
        if (isActive) OnSkipPressed();
    }

    // Если идёт печать — досрочно показываем всю фразу; если уже готова — следующая
    void OnSkipPressed()
    {
        if (isTyping)
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            isTyping = false;
            dialogueText.text = phrases[currentPhrase];
            typingCoroutine = StartCoroutine(WaitThenNext());
        }
        else
        {
            if (typingCoroutine != null) StopCoroutine(typingCoroutine);
            AdvancePhrase();
        }
    }

    void StartDialogue()
    {
        if (isActive) return;
        isActive = true;
        currentPhrase = 0;
        dialogueCanvas.SetActive(true);
        typingCoroutine = StartCoroutine(TypePhrase(currentPhrase));
    }

    void AdvancePhrase()
    {
        currentPhrase++;
        if (currentPhrase >= phrases.Length)
            EndDialogue();
        else
            typingCoroutine = StartCoroutine(TypePhrase(currentPhrase));
    }

    IEnumerator TypePhrase(int index)
    {
        isTyping = true;
        dialogueText.text = "";
        string phrase = phrases[index];

        foreach (char c in phrase)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(charDelay);
        }

        isTyping = false;
        typingCoroutine = StartCoroutine(WaitThenNext());
    }

    IEnumerator WaitThenNext()
    {
        yield return new WaitForSeconds(pauseAfterPhrase);
        AdvancePhrase();
    }

    void EndDialogue()
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = null;
        isTyping = false;
        isActive = false;
        playerInRange = false;
        dialogueCanvas.SetActive(false);
        currentPhrase = 0;
    }

    void Subscribe(InputActionReference reference, System.Action<InputAction.CallbackContext> handler)
    {
        if (reference != null)
        {
            reference.action.Enable();
            reference.action.performed += handler;
        }
    }

    void Unsubscribe(InputActionReference reference, System.Action<InputAction.CallbackContext> handler)
    {
        if (reference != null)
            reference.action.performed -= handler;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + triggerOffset, triggerRadius);
    }
}
