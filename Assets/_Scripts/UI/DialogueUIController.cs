using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Simple dialogue UI controller that shows a single line of text
/// and closes when the action button is pressed.
/// </summary>
public class DialogueUIController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI dialogueLabel;
    [Tooltip("Optional hint text like 'Press [Action] to continue'.")]
    public TextMeshProUGUI hintLabel;

    private Action _onFinished;

    private void Awake()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        UIInputBlocker.Push();
    }

    private void OnDisable()
    {
        UIInputBlocker.Pop();
    }

    /// <summary>
    /// Show a dialogue line and register a callback for when it is closed.
    /// </summary>
    public void Show(string text, Action onFinished)
    {
        _onFinished = onFinished;

        if (dialogueLabel != null)
        {
            dialogueLabel.text = text;
        }

        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Update the hint text (e.g. \"Press E to continue\").
    /// </summary>
    public void SetHint(string text)
    {
        if (hintLabel == null)
            return;

        hintLabel.text = text;
    }

    /// <summary>
    /// Hide the dialogue UI and invoke the finished callback if any.
    /// </summary>
    public void Hide()
    {
        if (gameObject.activeSelf)
        {
            gameObject.SetActive(false);
        }

        var cb = _onFinished;
        _onFinished = null;
        cb?.Invoke();
    }
}

