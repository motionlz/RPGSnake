using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class HeroController : UnitManager
{
    private void OnTriggerEnter2D(Collider2D col) 
    {
        if (col.gameObject.tag == "Enemy")
        {
            TakeDamage(col.GetComponent<UnitManager>().status.atk);
            CheckDead();
        }
    }

    public override async void CheckDead()
    {
        if (HpCheck())
        {
            await PlayerManager.Instance.RemoveHeroFromLine(this);
        }
        base.CheckDead();
    }
}
