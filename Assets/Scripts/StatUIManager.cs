using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatUIManager : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI atkText;
    public TextMeshProUGUI defText;
    public Slider hpBar;

    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    public void SetValue(float currentHp, float maxHp, float atk, float def)
    {
        hpBar.maxValue = maxHp;
        hpBar.value = currentHp;

        atkText.text = atk.ToString();
        defText.text = def.ToString();
    }
}
