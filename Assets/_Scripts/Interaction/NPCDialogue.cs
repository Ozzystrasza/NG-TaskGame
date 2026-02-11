using UnityEngine;

public class NPCDialogue : InteractiveElement
{
    [Header("Dialogue")]
    public string dialogueId = "npc_dialogue";

    public override void OnInteract()
    {
        Debug.Log($"NPCDialogue: start dialogue '{dialogueId}'");
        // TODO: Open dialogue UI / dialogue system invocation
    }
}
