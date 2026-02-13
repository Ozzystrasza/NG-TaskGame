using UnityEngine;

public static class UIInputBlocker
{
    private static int _blockCount;

    public static bool IsBlocked => _blockCount > 0;

    public static void Push()
    {
        _blockCount++;
    }

    public static void Pop()
    {
        _blockCount--;
        if (_blockCount < 0)
        {
            _blockCount = 0;
        }
    }

    public static void Clear()
    {
        _blockCount = 0;
    }
}

