using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuUIManager : MonoBehaviour
{    
    [SerializeField] Button restartButton;

    private void Awake()
    {
        restartButton.onClick.AddListener(() => {
            GameManager.Instance.StartGame();
            HideDialog();
        });
    }

    public void ShowDialog()
    {
        gameObject.SetActive(true);
    }

    public void HideDialog()
    {
        gameObject.SetActive(false);
    }
}
