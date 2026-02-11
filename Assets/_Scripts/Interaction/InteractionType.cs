using UnityEngine;

[CreateAssetMenu(menuName = "Interaction/Interaction Type", fileName = "NewInteractionType")]
public class InteractionType : ScriptableObject
{
    public string typeName = "NewType";

    [TextArea]
    public string interactionText = "Interact";
}
