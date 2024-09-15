using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class HeroController : UnitManager
{
    protected override string OppositeSideTag => GlobalTag.ENEMY;
    public event Action<HeroController> OnHeroDead;
    public override void CheckDead()
    {
        if (IsDead())
            OnHeroDead.Invoke(this);
        base.CheckDead();
    }
}
