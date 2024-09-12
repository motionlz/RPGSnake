using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class UnitManager : MonoBehaviour
{
    [SerializeField] Rigidbody2D rigidbody;
    [SerializeField] BoxCollider2D boxCollider;

    [Header("UI")]
    [SerializeField] StatUIManager statUI;

    [Header("Stat")]
    public CharacterStatus status;
    //[SerializeField] HeroStatus upgradeStat;
    private float currentHp;

    public virtual void Awake() 
    {
        currentHp = status.hp;
    }
    private void OnEnable() 
    {
        UpdateUI();
    }
    private void Reset()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();


    }

    public virtual void UpdateUI()
    {
        statUI.SetValue(currentHp, status.hp, status.atk, status.def);
    }

    public virtual void ActiveUI(bool active)
    {
        statUI.SetActive(active);
    }

    public virtual void TakeDamage(float attackerDamage)
    {
        currentHp -= Mathf.Clamp(attackerDamage - status.def, 0, status.hp);
        UpdateUI();
    }

    public virtual void CheckDead()
    {
        if (HpCheck())
        {
            this.gameObject.SetActive(false);
            //Destroy(this.gameObject);
        }
    }
    public bool HpCheck()
    {
        if(currentHp <= 0)
            return true;
        return false;
    }
}
