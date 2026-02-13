using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogueLine
{
    [TextArea]
    public string text;

    [Header("Optional one-time reward")]
    public bool isOneTimeReward;
    public ItemDefinition rewardItem;
}

[CreateAssetMenu(menuName = "Dialogue/Dialogue Definition", fileName = "NewDialogue")]
public class DialogueDefinition : ScriptableObject
{
    [Header("Identity")]
    public string id = "npc_dialogue";

    [Header("Lines")]
    public List<DialogueLine> lines = new List<DialogueLine>();
}

