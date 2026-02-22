using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class TricksterEnemy : EnemyController
{
    [Header("Trickster Mark")]
    [SerializeField] private float markRange = 3.5f;
    [SerializeField] private float markCooldown = 6f;
    [SerializeField] private float windupTime = 0.5f;

    private bool markInProgress;
    private float nextMarkTime;

    protected override void OnEnable()
    {
        base.OnEnable();
        markInProgress = false;
        nextMarkTime = 0f;
    }

    protected override void Update()
    {
        base.Update();
        TryMarkPlayer();
    }

    private void TryMarkPlayer()
    {
        if (markInProgress || Time.time < nextMarkTime)
        {
            return;
        }

        Transform player = PlayerTransform;
        if (player == null)
        {
            return;
        }

        if (Vector2.Distance(transform.position, player.position) > markRange)
        {
            return;
        }

        PlayerStatus status = player.GetComponent<PlayerStatus>();
        if (status == null)
        {
            return;
        }

        StartCoroutine(MarkRoutine(status));
    }

    private IEnumerator MarkRoutine(PlayerStatus targetStatus)
    {
        markInProgress = true;
        TriggerAttackAnimation();
        Debug.Log($"{name} is winding up player mark.");

        if (windupTime > 0f)
        {
            yield return new WaitForSeconds(windupTime);
        }

        Transform player = PlayerTransform;
        if (player != null && targetStatus != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            if (distance <= markRange)
            {
                targetStatus.ApplyMarked();
            }
        }

        nextMarkTime = Time.time + markCooldown;
        markInProgress = false;
    }
}
