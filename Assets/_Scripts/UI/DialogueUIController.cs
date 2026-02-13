using System;
using TMPro;
using UnityEngine;

public class DialogueUIController : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI dialogueLabel;
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
        transform.SetAsLastSibling();
    }

    public void SetHint(string text)
    {
        if (hintLabel == null)
            return;

        hintLabel.text = text;
    }

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

