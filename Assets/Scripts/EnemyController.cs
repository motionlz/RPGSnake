using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class EnemyController : UnitManager
{
    private void OnTriggerEnter2D(Collider2D col) 
    {
        if (col.gameObject.tag == "LineHero")
        {
            TakeDamage(col.GetComponent<UnitManager>().status.atk);
            CheckDead();
        }
    }

    public override void CheckDead()
    {
        if (HpCheck())
        {
            GameManager.Instance.DeadEnemyRemove(this);
        }
        base.CheckDead();
    }
}
