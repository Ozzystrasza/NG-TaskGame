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

