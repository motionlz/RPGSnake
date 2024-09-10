using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(BoxCollider2D))]
public class HeroController : MonoBehaviour
{
    [SerializeField] Rigidbody2D rigidbody;
    [SerializeField] BoxCollider2D boxCollider;

    [Header("Stat")]
    [SerializeField] HeroStatus baseStat;
    //[SerializeField] HeroStatus upgradeStat;
    private float currentHp;

    [Header("UI")]
    [SerializeField] StatUIManager statUI;

    private void Awake() 
    {
        currentHp = baseStat.hp;
        UpdateUI();
    }
    private void Reset()
    {
        rigidbody = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
    }

    private void UpdateUI()
    {
        statUI.hpBar.maxValue = baseStat.hp;
        statUI.hpBar.value = currentHp;

        statUI.atkText.text = baseStat.atk.ToString();
        statUI.defText.text = baseStat.def.ToString();
    }

    public void ActiveUI(bool active)
    {
        statUI.SetActive(active);
        UpdateUI();
    }
}
