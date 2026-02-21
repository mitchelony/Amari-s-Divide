using System;
using System.Collections;
using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    [SerializeField] private float restoreLockoutSeconds = 1.0f;

    public bool IsMarked { get; private set; }
    public bool RestoreAvailable { get; private set; }
    public float RestoreLockoutSeconds => restoreLockoutSeconds;

    public event Action<bool> OnMarkedChanged;
    public event Action OnRestoreAvailable;

    private Coroutine restoreRoutine;

    public void ApplyMarked()
    {
        if (restoreRoutine != null)
        {
            StopCoroutine(restoreRoutine);
            restoreRoutine = null;
        }

        IsMarked = true;
        RestoreAvailable = false;
        OnMarkedChanged?.Invoke(true);
        restoreRoutine = StartCoroutine(RestoreAvailabilityAfterDelay());
    }

    public void ClearMarked()
    {
        if (restoreRoutine != null)
        {
            StopCoroutine(restoreRoutine);
            restoreRoutine = null;
        }

        IsMarked = false;
        RestoreAvailable = false;
        OnMarkedChanged?.Invoke(false);
    }

    private IEnumerator RestoreAvailabilityAfterDelay()
    {
        if (restoreLockoutSeconds > 0f)
        {
            yield return new WaitForSeconds(restoreLockoutSeconds);
        }

        if (!IsMarked)
        {
            yield break;
        }

        RestoreAvailable = true;
        OnRestoreAvailable?.Invoke();
        restoreRoutine = null;
    }
}
