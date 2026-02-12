using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Single dialogue line configuration for an NPC.
/// </summary>
[Serializable]
public class DialogueLine
{
    [TextArea]
    public string text;

    [Header("Optional one-time reward")]
    public bool isOneTimeReward;
    public ItemDefinition rewardItem;
}

/// <summary>
/// ScriptableObject that groups the dialogue lines for a given NPC or group of NPCs.
/// </summary>
[CreateAssetMenu(menuName = "Dialogue/Dialogue Definition", fileName = "NewDialogue")]
public class DialogueDefinition : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Id referenced by NPCDialogue.dialogueId.")]
    public string id = "npc_dialogue";

    [Header("Lines")]
    public List<DialogueLine> lines = new List<DialogueLine>();
}

