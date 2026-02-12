using UnityEngine;

/// <summary>
/// Global helper to indicate when gameplay input should be blocked
/// because a blocking UI (inventory, menus, popups) is open.
/// Uses a simple reference count so multiple UIs can overlap safely.
/// </summary>
public static class UIInputBlocker
{
    private static int _blockCount;

    /// <summary>
    /// True when one or more blocking UIs are active.
    /// </summary>
    public static bool IsBlocked => _blockCount > 0;

    /// <summary>
    /// Signal that a blocking UI has opened or become active.
    /// </summary>
    public static void Push()
    {
        _blockCount++;
#if UNITY_EDITOR
        // Debug.Log($"UIInputBlocker.Push -> count = {_blockCount}");
#endif
    }

    /// <summary>
    /// Signal that a blocking UI has closed or become inactive.
    /// </summary>
    public static void Pop()
    {
        _blockCount--;
        if (_blockCount < 0)
        {
            _blockCount = 0;
        }
#if UNITY_EDITOR
        // Debug.Log($"UIInputBlocker.Pop -> count = {_blockCount}");
#endif
    }

    /// <summary>
    /// Reset the blocker completely. Not usually needed in normal gameplay.
    /// </summary>
    public static void Clear()
    {
        _blockCount = 0;
    }
}

