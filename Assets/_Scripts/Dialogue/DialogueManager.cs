using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

/// <summary>
/// Central manager for handling NPC dialogue lookup, random selection and rewards.
/// </summary>
public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Data")]
    [Tooltip("All dialogue definitions available in the game.")]
    public List<DialogueDefinition> dialogueDefinitions = new List<DialogueDefinition>();

    [Header("UI")]
    [Tooltip("Reference to the dialogue UI controller in the scene.")]
    public DialogueUIController dialogueUI;

    [Header("Input")]
    [Tooltip("Input action used to advance / close dialogue (usually the same as interact).")]
    public InputActionReference continueAction;

    /// <summary>
    /// Tracks which one-time reward lines have already been consumed.
    /// Key: dialogueId, Value: set of line indices.
    /// </summary>
    private readonly Dictionary<string, HashSet<int>> _consumedRewardLines =
        new Dictionary<string, HashSet<int>>();

    /// <summary>
    /// Tracks which line indices have been shown at least once per dialogue id.
    /// Used for gating reward lines and improving randomness.
    /// </summary>
    private readonly Dictionary<string, HashSet<int>> _shownLines =
        new Dictionary<string, HashSet<int>>();

    /// <summary>
    /// Last chosen line index per dialogue id, to avoid immediate repeats.
    /// </summary>
    private readonly Dictionary<string, int> _lastChosenIndex =
        new Dictionary<string, int>();

    private bool _dialogueOpen;
    private Action _onDialogueFinished;

    // Pending reward info, granted after dialogue closes
    private ItemDefinition _pendingRewardItem;
    private string _pendingRewardDialogueId;
    private int _pendingRewardLineIndex = -1;

    public bool IsDialogueOpen => _dialogueOpen;

    /// <summary>
    /// When true, InteractionManager should ignore the next interact (so the keypress that closed dialogue doesn't reopen it).
    /// </summary>
    public static bool IgnoreNextInteract { get; private set; }

    /// <summary>
    /// If IgnoreNextInteract is true, clears it and returns true; otherwise returns false.
    /// </summary>
    public static bool ConsumeIgnoreNextInteract()
    {
        if (!IgnoreNextInteract) return false;
        IgnoreNextInteract = false;
        return true;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }


    /// <summary>
    /// Entry point used by NPCDialogue to start a dialogue by id.
    /// </summary>
    public void StartDialogue(string dialogueId)
    {
        Debug.Log($"[DialogueManager] StartDialogue called for '{dialogueId}', open={_dialogueOpen}");

        if (string.IsNullOrEmpty(dialogueId))
        {
            Debug.LogWarning("DialogueManager.StartDialogue called with empty id.");
            return;
        }

        if (_dialogueOpen)
        {
            // Already showing a dialogue; ignore for now to avoid overlap.
            return;
        }

        var def = FindDefinition(dialogueId);
        if (def == null)
        {
            Debug.LogWarning($"DialogueManager: No DialogueDefinition found for id '{dialogueId}'.");
            return;
        }

        if (def.lines == null || def.lines.Count == 0)
        {
            Debug.LogWarning($"DialogueManager: DialogueDefinition '{dialogueId}' has no lines.");
            return;
        }

        int chosenIndex = ChooseLineIndex(def, dialogueId);
        if (chosenIndex < 0 || chosenIndex >= def.lines.Count)
        {
            Debug.LogWarning($"DialogueManager: Could not choose a valid line for id '{dialogueId}'.");
            return;
        }

        var line = def.lines[chosenIndex];
        Debug.Log($"[DialogueManager] Dialogue '{dialogueId}' chose line index {chosenIndex}, reward={line.isOneTimeReward}");
        if (dialogueUI == null)
        {
            Debug.LogWarning("DialogueManager: No DialogueUIController assigned, cannot show dialogue.");
            return;
        }

        // Track that this line has been shown at least once.
        GetShownSet(dialogueId).Add(chosenIndex);
        _lastChosenIndex[dialogueId] = chosenIndex;

        // If this is a one-time reward line and not yet consumed, defer the reward
        // until after the dialogue closes.
        if (line.isOneTimeReward && line.rewardItem != null && !IsRewardConsumed(dialogueId, chosenIndex))
        {
            _pendingRewardItem = line.rewardItem;
            _pendingRewardDialogueId = dialogueId;
            _pendingRewardLineIndex = chosenIndex;
        }

        OpenDialogue(line.text);
    }

    private DialogueDefinition FindDefinition(string id)
    {
        for (int i = 0; i < dialogueDefinitions.Count; i++)
        {
            var def = dialogueDefinitions[i];
            if (def != null && def.id == id)
            {
                return def;
            }
        }
        return null;
    }

    private int ChooseLineIndex(DialogueDefinition def, string id)
    {
        var consumedRewards = GetConsumedSet(id);
        var shown = GetShownSet(id);

        var normalIndices = new List<int>();
        var rewardIndices = new List<int>();

        for (int i = 0; i < def.lines.Count; i++)
        {
            var line = def.lines[i];
            if (line == null) continue;

            if (line.isOneTimeReward)
            {
                if (!consumedRewards.Contains(i))
                {
                    rewardIndices.Add(i);
                }
            }
            else
            {
                normalIndices.Add(i);
            }
        }

        Debug.Log($"[DialogueManager] '{id}' normals={normalIndices.Count}, rewards={rewardIndices.Count}, shown={shown.Count}");

        var candidates = new List<int>();

        // Normal lines are always eligible.
        candidates.AddRange(normalIndices);

        // Gift gating: only allow reward lines once all normal lines have been shown at least once.
        if (rewardIndices.Count > 0 && AllNormalShown(normalIndices, shown))
        {
            candidates.AddRange(rewardIndices);
        }

        Debug.Log($"[DialogueManager] '{id}' candidateCount={candidates.Count}");

        if (candidates.Count == 0)
        {
            Debug.LogWarning($"[DialogueManager] '{id}' has no eligible lines (all consumed/gated).");
            return -1;
        }

        // Prefer unseen candidates, fall back to seen, avoid immediate repeat when possible.
        var unseen = new List<int>();
        var seen = new List<int>();

        foreach (var idx in candidates)
        {
            if (shown.Contains(idx))
                seen.Add(idx);
            else
                unseen.Add(idx);
        }

        var pool = unseen.Count > 0 ? unseen : seen;

        if (pool.Count == 1)
        {
            return pool[0];
        }

        int lastIndex;
        bool hasLast = _lastChosenIndex.TryGetValue(id, out lastIndex);

        // Build a filtered pool that avoids the last index when possible.
        var filtered = new List<int>();
        foreach (var idx in pool)
        {
            if (!hasLast || idx != lastIndex)
            {
                filtered.Add(idx);
            }
        }

        var finalPool = filtered.Count > 0 ? filtered : pool;

        int randomIdx = UnityEngine.Random.Range(0, finalPool.Count);
        return finalPool[randomIdx];
    }

    private HashSet<int> GetConsumedSet(string id)
    {
        if (!_consumedRewardLines.TryGetValue(id, out var set))
        {
            set = new HashSet<int>();
            _consumedRewardLines[id] = set;
        }
        return set;
    }

    private HashSet<int> GetShownSet(string id)
    {
        if (!_shownLines.TryGetValue(id, out var set))
        {
            set = new HashSet<int>();
            _shownLines[id] = set;
        }
        return set;
    }

    private bool AllNormalShown(List<int> normalIndices, HashSet<int> shown)
    {
        if (normalIndices.Count == 0)
            return false;

        for (int i = 0; i < normalIndices.Count; i++)
        {
            if (!shown.Contains(normalIndices[i]))
                return false;
        }
        return true;
    }

    private bool IsRewardConsumed(string id, int lineIndex)
    {
        var set = GetConsumedSet(id);
        return set.Contains(lineIndex);
    }

    private void MarkRewardConsumed(string id, int lineIndex)
    {
        var set = GetConsumedSet(id);
        set.Add(lineIndex);
    }

    private void GiveReward(ItemDefinition reward)
    {
        if (reward == null) return;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.Add(reward);
        }

        // Reuse existing collected item UI if available.
        InteractionManager.Instance?.ShowCollected(reward);
    }

    private void OpenDialogue(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        _dialogueOpen = true;
        _onDialogueFinished = OnDialogueFinishedInternal;

        if (dialogueUI == null)
        {
            Debug.LogWarning("DialogueManager: Dialogue UI reference is not assigned. Assign the DialogueUIController in the Inspector.");
            return;
        }

        var bindingDisplay = GetContinueBindingDisplay();
        if (!string.IsNullOrEmpty(bindingDisplay))
        {
            dialogueUI.SetHint($"Press {bindingDisplay} to continue");
        }
        dialogueUI.Show(text, _onDialogueFinished);
        InteractionManager.Instance?.HideInteractionPrompt();
    }

    /// <summary>
    /// Called by InteractionManager when the player presses interact while dialogue is open.
    /// </summary>
    public void RequestCloseDialogue()
    {
        CloseDialogue();
    }

    private void CloseDialogue()
    {
        if (!_dialogueOpen) return;

        _dialogueOpen = false;

        dialogueUI.Hide();
    }

    private void OnDialogueFinishedInternal()
    {
        _dialogueOpen = false;

        bool gaveReward = false;

        // Grant any pending reward now that the dialogue is closed.
        if (_pendingRewardItem != null && !string.IsNullOrEmpty(_pendingRewardDialogueId) && _pendingRewardLineIndex >= 0)
        {
            if (!IsRewardConsumed(_pendingRewardDialogueId, _pendingRewardLineIndex))
            {
                GiveReward(_pendingRewardItem);
                MarkRewardConsumed(_pendingRewardDialogueId, _pendingRewardLineIndex);
                gaveReward = true;
            }
        }

        _pendingRewardItem = null;
        _pendingRewardDialogueId = null;
        _pendingRewardLineIndex = -1;

        if (!gaveReward)
            InteractionManager.Instance?.ShowInteractionPromptIfFocused();
    }

    private string GetContinueBindingDisplay()
    {
        if (continueAction == null || continueAction.action == null)
            return null;

        var action = continueAction.action;

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
                    var human = InputControlPath.ToHumanReadableString(path, InputControlPath.HumanReadableStringOptions.OmitDevice);
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
}

