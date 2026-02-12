using UnityEngine;

public class NPCDialogue : InteractiveElement
{
    [Header("Dialogue")]
    [Tooltip("Assign the DialogueDefinition asset for this NPC. Its id is used to start dialogue.")]
    public DialogueDefinition dialogueDefinition;

    public override void OnInteract()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("NPCDialogue: No DialogueManager in scene, cannot start dialogue.");
            return;
        }

        if (dialogueDefinition == null)
        {
            Debug.LogWarning("NPCDialogue: No DialogueDefinition assigned. Assign the asset in the Inspector.");
            return;
        }

        DialogueManager.Instance.StartDialogue(dialogueDefinition.id);
    }
}
