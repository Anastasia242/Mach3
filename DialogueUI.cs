using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public TextMeshProUGUI dialogueText;
    public Button continueButton;

    public Action OnDialogueFinished;

    private void Start()
    {
        if (continueButton != null)
            continueButton.onClick.AddListener(HideDialogue);
    }

    public void ShowDialogue(string message)
    {
        panel.SetActive(true);
        dialogueText.text = message;
    }

    public void HideDialogue()
    {
        panel.SetActive(false);
        OnDialogueFinished?.Invoke();
    }
}
