using UnityEngine;

public class ToggleActive : MonoBehaviour
{
    [Header("Targets To Toggle")]
    [SerializeField] private GameObject[] targets;

    [Header("Initial State")]
    [SerializeField] private bool startActive = false;

    private void Awake()
    {
        if (targets == null) return;

        foreach (var t in targets)
        {
            if (t != null)
            {
                t.SetActive(startActive);
            }
        }
    }

    /// <summary>
    /// Toggle the active state of all assigned targets.
    /// Hook this up to a UI Button's OnClick event.
    /// </summary>
    public void Toggle()
    {
        if (targets == null) return;

        foreach (var t in targets)
        {
            if (t == null) continue;

            bool newState = !t.activeSelf;
            t.SetActive(newState);
        }
    }
}

