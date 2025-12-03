using UnityEngine;

public abstract class EnemyState
{
    protected readonly EnemyController controller;

    protected EnemyState(EnemyController controller)
    {
        this.controller = controller;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Tick() { }
}


