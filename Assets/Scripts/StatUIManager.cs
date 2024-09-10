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
}
