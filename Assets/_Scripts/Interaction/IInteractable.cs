using UnityEngine;

public interface IInteractable
{
    void OnFocus();
    void OnDefocus();
    void OnInteract();
    string InteractionText { get; }
}
