using UnityEngine;

public abstract class AttackAreaBase : ScriptableObject
{
	public abstract Collider2D[] FindTargets(Vector2 origin, Vector2 direction, LayerMask mask, float range, float radius);
	public abstract void DrawGizmos(Vector2 origin, Vector2 direction, float range, float radius, Color color);
}


