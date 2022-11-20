using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager instance { get; set; }

    [SerializeField]
    GameObject LoginPanel,
        RegisterPanel;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        LoginShow();
    }

    public void LoginShow()
    {
        LoginPanel.SetActive(true);
        RegisterPanel.SetActive(false);
    }

    public void RegisterShow()
    {
        LoginPanel.SetActive(false);
        RegisterPanel.SetActive(true);
    }
}
