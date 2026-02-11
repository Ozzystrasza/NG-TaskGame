using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InteractionUI : MonoBehaviour
{
    public TextMeshProUGUI label;

    public TextMeshProUGUI buttonLabel;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    public void Show(string text, string buttonText = null)
    {
        if (label != null)
            label.text = text;
        if (buttonLabel != null)
            buttonLabel.text = buttonText ?? string.Empty;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
