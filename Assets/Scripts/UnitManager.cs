using System;
using System.Threading.Tasks;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public abstract class UnitManager : MonoBehaviour
{
    [SerializeField] Rigidbody2D rigidbody;
    [SerializeField] BoxCollider2D boxCollider;

    [Header("UI")]
    [SerializeField] StatUIManager statUI;

    [Header("Stat")]
    public CharacterStatus status;
    private float currentHp;
    protected abstract string OppositeSideTag { get; }
    public virtual void Awake() 
    {
        RestoreHP();
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

    public void RestoreHP()
    {
        currentHp = status.hp;
        UpdateUI();
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
        if (IsDead())
        {
            this.gameObject.SetActive(false);
        }
    }
    public bool IsDead()
    {
        return currentHp <= 0;
    }

    private void OnTriggerEnter2D(Collider2D col) 
    {
        if (col.gameObject.CompareTag(OppositeSideTag))
        {
            TakeDamage(col.GetComponent<UnitManager>().status.atk);
            CheckDead();
        }
    }
}
