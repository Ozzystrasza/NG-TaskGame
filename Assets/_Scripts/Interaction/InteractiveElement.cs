using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public abstract class InteractiveElement : MonoBehaviour, IInteractable
{
    [Header("Interaction")]
    [SerializeField] protected InteractionType interactionType;

    protected string playerTag = "Player";

    public string InteractionText => interactionType.interactionText;

    protected virtual void Reset()
    {
        var col = GetComponent<SphereCollider>();
        if (col != null)
            col.isTrigger = true;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        InteractionManager.Instance?.SetFocus(this);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
            return;

        InteractionManager.Instance?.ClearFocus(this);
    }

    public virtual void OnFocus()
    {
    }

    public virtual void OnDefocus()
    {
    }

    public abstract void OnInteract();
}
