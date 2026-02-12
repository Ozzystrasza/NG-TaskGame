using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ItemCollectedUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameLabel;
    public float displayTime = 1.6f;

    void Awake()
    {
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        UIInputBlocker.Push();
    }

    void OnDisable()
    {
        UIInputBlocker.Pop();
        InteractionManager.Instance?.ShowInteractionPromptIfFocused();
    }

    public void Show(ItemDefinition def)
    {
        if (def == null) return;

        if (icon != null) icon.sprite = def.icon;
        if (nameLabel != null) nameLabel.text = def.itemName;

        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(HideAfter());
    }

    private IEnumerator HideAfter()
    {
        yield return new WaitForSeconds(displayTime);
        gameObject.SetActive(false);
    }
}
