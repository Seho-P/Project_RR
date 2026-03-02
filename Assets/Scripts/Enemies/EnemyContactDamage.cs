using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class EnemyContactDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private float damage = 10f;
    [SerializeField] private float tickInterval = 0.5f;

    [Header("Target")]
    [SerializeField] private LayerMask targetMask = ~0;
    [SerializeField] private bool searchDamageableInParent = true;

    private readonly Dictionary<int, float> nextDamageTimeByTarget = new();

    // 비활성화 시 데미지 시간 초기화
    private void OnDisable()
    {
        nextDamageTimeByTarget.Clear();
    }

    // 충돌 시 데미지를 입히는 메서드
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision == null) return;
        TryDealDamage(collision.collider);
    }

    // 트리거 충돌 시 데미지를 입히는 메서드
    private void OnTriggerStay2D(Collider2D other)
    {
        TryDealDamage(other);
    }

    // 데미지를 입히는 메서드
    private void TryDealDamage(Collider2D other)
    {
        if (other == null) return;
        if (!IsInTargetMask(other.gameObject.layer)) return;
        // 자기 자신은 데미지를 입히지 않음
        if (other.transform.root == transform.root) return;

        IDamageable damageable = searchDamageableInParent
            ? other.GetComponentInParent<IDamageable>()
            : other.GetComponent<IDamageable>();

        if (damageable == null) return;
        if (damageable is not Component damageableComponent) return;

        int targetId = damageableComponent.GetInstanceID();
        if (nextDamageTimeByTarget.TryGetValue(targetId, out float nextDamageTime) && Time.time < nextDamageTime)
        {
            return;
        }

        Vector2 hitDirection = ((Vector2)other.bounds.center - (Vector2)transform.position).normalized;
        if (hitDirection.sqrMagnitude < 0.0001f)
        {
            hitDirection = Vector2.up;
        }

        damageable.TakeDamage(damage, hitDirection);
        nextDamageTimeByTarget[targetId] = Time.time + Mathf.Max(0f, tickInterval);
    }

    // 데미지를 입힐 수 있는 레이어인지 확인
    private bool IsInTargetMask(int layer)
    {
        return ((1 << layer) & targetMask.value) != 0;
    }
}
