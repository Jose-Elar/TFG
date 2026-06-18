using System;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TextManager : MonoBehaviour
{
    public static TextManager Instance;

    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.05f;
    [SerializeField] private float panelOpenDelay = 0.5f;

    [Header("Speaker Colors")]
    [Tooltip("Map each speaker name (must match the 'speaker' field in the JSON) to a color.")]
    [SerializeField] private SpeakerColor[] speakerColors;

    [Tooltip("Used when a speaker name isn't found in the list above.")]
    [SerializeField] private Color defaultSpeakerColor = Color.white;

    [Serializable]
    public struct SpeakerColor
    {
        public string speaker;
        public Color  color;
    }

    public event Action OnDialogueEnded;

    private MainDrone_Actions mainDrone_Actions;

    private DialogueDatabase dialogueDatabase;
    private Dialogue currentDialogue;

    private int currentLine;

    private bool isTyping;
    private bool dialogueStarting;

    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        mainDrone_Actions = new MainDrone_Actions();

        LoadDialogueDatabase();

        dialoguePanel.SetActive(false);
    }

    private void OnEnable()
    {
        mainDrone_Actions.Drone.Enable();
        mainDrone_Actions.Drone.Next_Text.performed += Next_Text_performed;
    }

    private void OnDisable()
    {
        mainDrone_Actions.Drone.Next_Text.performed -= Next_Text_performed;
        mainDrone_Actions.Drone.Disable();
    }

    private void LoadDialogueDatabase()
    {
        string path = Path.Combine(
            Application.dataPath,
            "TextFiles",
            "TextTrys.json"
        );

        if (!File.Exists(path))
        {
            Debug.LogError($"Dialogue file not found:\n{path}");
            return;
        }

        string json = File.ReadAllText(path);

        dialogueDatabase = JsonUtility.FromJson<DialogueDatabase>(json);
    }

    public void StartDialogue(string dialogueId)
    {
        currentDialogue = Array.Find(
            dialogueDatabase.dialogues,
            dialogue => dialogue.id == dialogueId
        );

        if (currentDialogue == null)
        {
            Debug.LogError(
                $"Dialogue ID '{dialogueId}' not found."
            );

            return;
        }

        currentLine = 0;

        // Apply the speaker's color for this whole dialogue
        ApplySpeakerColor(currentDialogue.speaker);

        dialoguePanel.SetActive(true);

        StartCoroutine(BeginDialogueAfterDelay());
    }

    /// <summary>
    /// Looks up the speaker in the Inspector-configured list and applies
    /// the matching color to the dialogue text. Falls back to defaultSpeakerColor.
    /// </summary>
    private void ApplySpeakerColor(string speaker)
    {
        foreach (SpeakerColor entry in speakerColors)
        {
            if (entry.speaker == speaker)
            {
                dialogueText.color = entry.color;
                return;
            }
        }

        Debug.LogWarning($"[TextManager] No color set for speaker '{speaker}', using default.");
        dialogueText.color = defaultSpeakerColor;
    }

    private IEnumerator BeginDialogueAfterDelay()
    {
        dialogueStarting = true;

        dialogueText.text = "";

        yield return new WaitForSeconds(panelOpenDelay);

        dialogueStarting = false;

        StartTypingCurrentLine();
    }

    private void Next_Text_performed(InputAction.CallbackContext context)
    {
        if (currentDialogue == null)
            return;

        if (dialogueStarting)
            return;

        if (isTyping)
        {
            StopCoroutine(typingCoroutine);

            dialogueText.text =
                currentDialogue.lines[currentLine];

            isTyping = false;

            return;
        }

        currentLine++;

        if (currentLine >= currentDialogue.lines.Length)
        {
            EndDialogue();
            return;
        }

        StartTypingCurrentLine();
    }

    private void StartTypingCurrentLine()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        typingCoroutine =
            StartCoroutine(
                TypeLine(
                    currentDialogue.lines[currentLine]
                )
            );
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;

        dialogueText.text = "";

        foreach (char letter in line)
        {
            dialogueText.text += letter;

            yield return new WaitForSeconds(
                typingSpeed
            );
        }

        isTyping = false;
    }

    private void EndDialogue()
    {
        currentDialogue = null;
        dialogueText.text = "";
        dialoguePanel.SetActive(false);

        OnDialogueEnded?.Invoke();
    }
}