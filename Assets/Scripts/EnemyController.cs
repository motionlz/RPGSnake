using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class EnemyController : UnitManager
{
    protected override string OppositeSideTag => GlobalTag.LINE_HERO;
    public event Action<EnemyController> OnEnemyDead;
    [Header("Move Stat")]
    [SerializeField] bool canMove;
    [SerializeField] int turnToMove;
    public override void CheckDead()
    {
        if (IsDead())
            OnEnemyDead.Invoke(this);
        base.CheckDead();
    }
}

