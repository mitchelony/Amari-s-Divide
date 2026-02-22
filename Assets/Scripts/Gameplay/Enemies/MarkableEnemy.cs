using System;
using UnityEngine;

public class MarkableEnemy : MonoBehaviour
{
    public static MarkableEnemy Current { get; private set; }

    public bool IsMarked { get; private set; }

    public event Action OnMarked;
    public event Action OnUnmarked;

    public void Mark()
    {
        if (Current != null && Current != this)
        {
            Current.Unmark();
        }

        Current = this;
        IsMarked = true;
        OnMarked?.Invoke();
    }

    public void Unmark()
    {
        if (Current == this)
        {
            Current = null;
        }

        if (!IsMarked)
        {
            return;
        }

        IsMarked = false;
        OnUnmarked?.Invoke();
    }

    private void OnDisable()
    {
        if (Current == this)
        {
            Current = null;
        }

        IsMarked = false;
    }
}
