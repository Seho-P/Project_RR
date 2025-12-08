using UnityEngine;

public abstract class PlayerState
{
    protected readonly PlayerController controller;

    protected PlayerState(PlayerController controller)
    {
        this.controller = controller;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Tick() { }
    public virtual void FixedTick() { }
}

