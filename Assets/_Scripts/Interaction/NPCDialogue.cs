using UnityEngine;

public class NPCDialogue : InteractiveElement
{
    [Header("Dialogue")]
    public string dialogueId = "npc_dialogue";

    public override void OnInteract()
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("NPCDialogue: No DialogueManager in scene, cannot start dialogue.");
            return;
        }

        DialogueManager.Instance.StartDialogue(dialogueId);
    }
}
