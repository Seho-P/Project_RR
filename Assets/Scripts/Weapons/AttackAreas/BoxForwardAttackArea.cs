using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/AttackAreas/BoxForward")]
public class BoxForwardAttackArea : AttackAreaBase
{
	public Vector2 size = new Vector2(1.5f, 0.8f);

	public override Collider2D[] FindTargets(Vector2 origin, Vector2 direction, LayerMask mask, float range, float radius)
	{
		Vector2 center = origin + direction.normalized * (size.x * 0.5f);
		float angleZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		return Physics2D.OverlapBoxAll(center, size, angleZ, mask);
	}

	public override void DrawGizmos(Vector2 origin, Vector2 direction, float range, float radius, Color color)
	{
		Gizmos.color = color;
		Vector2 center = origin + direction.normalized * (size.x * 0.5f);
		float angleZ = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		Matrix4x4 prev = Gizmos.matrix;
		Gizmos.matrix = Matrix4x4.TRS(center, Quaternion.Euler(0, 0, angleZ), Vector3.one);
		Gizmos.DrawWireCube(Vector3.zero, size);
		Gizmos.matrix = prev;
	}
}


