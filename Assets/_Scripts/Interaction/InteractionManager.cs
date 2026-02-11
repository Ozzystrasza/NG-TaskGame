using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    public static InteractionManager Instance { get; private set; }

    [Header("Input")]
    public InputActionReference interactAction;

    [Header("UI")]
    public InteractionUI interactionUIPrefab;
    [Header("Collected UI")]
    public ItemCollectedUI collectedUIPrefab;

    private InteractionUI uiInstance;
    private ItemCollectedUI collectedUIInstance;
    private IInteractable currentFocused;

    private PlayerController playerController;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        playerController = FindAnyObjectByType<PlayerController>();
    }

    void OnEnable()
    {
        if (interactAction != null && interactAction.action != null)
            interactAction.action.performed += OnInteractPerformed;
    }

    void OnDisable()
    {
        if (interactAction != null && interactAction.action != null)
            interactAction.action.performed -= OnInteractPerformed;
    }

    private void OnInteractPerformed(InputAction.CallbackContext ctx)
    {
        if (currentFocused != null)
        {
            currentFocused.OnInteract();

            var mb = currentFocused as MonoBehaviour;
            if (mb != null)
            {
                if (mb.TryGetComponent<PickupItem>(out var pickup))
                {
                    playerController?.TriggerCollect();
                }
            }

        }
    }

    public void SetFocus(IInteractable el)
    {
        if (el == null) return;
        if (currentFocused == el) return;

        ClearCurrentFocus();
        currentFocused = el;
        currentFocused.OnFocus();
        ShowUIFor(currentFocused);
    }

    public void ClearFocus(IInteractable el)
    {
        if (el == null) return;
        if (currentFocused != el) return;

        ClearCurrentFocus();
    }

    private void ClearCurrentFocus()
    {
        if (currentFocused == null) return;
        currentFocused.OnDefocus();
        HideUI();
        currentFocused = null;
    }

    private void ShowUIFor(IInteractable el)
    {
        if (interactionUIPrefab == null) return;
        if (uiInstance == null)
        {
            uiInstance = Instantiate(interactionUIPrefab);
        }

        var buttonText = GetInteractBindingDisplay();
        uiInstance.Show(el.InteractionText, buttonText);
    }

    private string GetInteractBindingDisplay()
    {
        if (interactAction == null || interactAction.action == null)
            return null;

        var action = interactAction.action;

        try
        {
            var disp = action.GetBindingDisplayString();
            if (!string.IsNullOrEmpty(disp))
                return disp;
        }
        catch { }

        if (action.bindings.Count > 0)
        {
            foreach (var binding in action.bindings)
            {
                if (binding.isPartOfComposite || binding.isComposite)
                    continue;

                var path = !string.IsNullOrEmpty(binding.effectivePath) ? binding.effectivePath : binding.path;
                if (string.IsNullOrEmpty(path))
                    continue;

                try
                {
                    var human = UnityEngine.InputSystem.InputControlPath.ToHumanReadableString(path, InputControlPath.HumanReadableStringOptions.OmitDevice);
                    if (!string.IsNullOrEmpty(human))
                        return human;
                }
                catch
                {
                    return path;
                }
            }
        }

        return null;
    }

    private void HideUI()
    {
        if (uiInstance != null)
            uiInstance.Hide();
    }

    public void ShowCollected(ItemDefinition item)
    {
        if (collectedUIPrefab == null || item == null) return;

        if (collectedUIInstance == null)
            collectedUIInstance = Instantiate(collectedUIPrefab);

        collectedUIInstance.Show(item);
    }
}
