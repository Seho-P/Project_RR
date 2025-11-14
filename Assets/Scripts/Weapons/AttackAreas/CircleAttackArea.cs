using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/AttackAreas/Circle")]
public class CircleAttackArea : AttackAreaBase
{
	public override Collider2D[] FindTargets(Vector2 origin, Vector2 direction, LayerMask mask, float range, float radius)
	{
		return Physics2D.OverlapCircleAll(origin, radius, mask);
	}

	public override void DrawGizmos(Vector2 origin, Vector2 direction, float range, float radius, Color color)
	{
		Gizmos.color = color;
		Gizmos.DrawWireSphere(origin, radius);
	}
}


