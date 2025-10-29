using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/AttackAreas/Sector")]
public class SectorAttackArea : AttackAreaBase
{
	[Range(0f, 360f)] public float angle = 120f;
    [SerializeField] private float edgeTolerance = 0.05f; // 가장자리 포함 여유

	public override Collider2D[] FindTargets(Vector2 origin, Vector2 direction, LayerMask mask, float range, float radius)
	{
		Collider2D[] candidates = Physics2D.OverlapCircleAll(origin, radius, mask);
		float half = angle * 0.5f;
		List<Collider2D> list = new List<Collider2D>(candidates.Length);
		for (int i = 0; i < candidates.Length; i++)
		{
			// 콜라이더의 가장 가까운 점을 기준으로 각도/거리 판정(가장자리 누락 방지)
			Vector2 closest = candidates[i].ClosestPoint(origin);
			Vector2 to = closest - origin;
			float ang = Vector2.Angle(direction, to);
			if (ang <= half + 0.001f && to.magnitude <= radius + edgeTolerance)
			{
				list.Add(candidates[i]);
			}
		}
		return list.ToArray();
	}

	public override void DrawGizmos(Vector2 origin, Vector2 direction, float range, float radius, Color color)
	{
		Gizmos.color = color;
		float half = angle * 0.5f;
		Quaternion leftR = Quaternion.AngleAxis(-half, Vector3.forward);
		Quaternion rightR = Quaternion.AngleAxis(half, Vector3.forward);
		Vector3 left = (Vector3)(leftR * direction.normalized) * radius;
		Vector3 right = (Vector3)(rightR * direction.normalized) * radius;
		Gizmos.DrawLine(origin, origin + (Vector2)left);
		Gizmos.DrawLine(origin, origin + (Vector2)right);
		int steps = 20;
		for (int i = 0; i < steps; i++)
		{
			float t0 = -half + (angle / steps) * i;
			float t1 = -half + (angle / steps) * (i + 1);
			Vector3 p0 = origin + (Vector2)(Quaternion.AngleAxis(t0, Vector3.forward) * direction.normalized) * radius;
			Vector3 p1 = origin + (Vector2)(Quaternion.AngleAxis(t1, Vector3.forward) * direction.normalized) * radius;
			Gizmos.DrawLine(p0, p1);
		}
	}
}


