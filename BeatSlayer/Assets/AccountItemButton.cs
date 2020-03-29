using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccountItemButton : MonoBehaviour
{
    public AccountManager manager;

    public string nick;

    [Header("Available actions")]
    public bool openProfile;



    public void Setup(AccountManager manager, string nick)
    {
        this.manager = manager;
        this.nick = nick;
    }


    public void OnClick()
    {
        manager.actionLocker.SetActive(true);
        manager.actionLocker.transform.GetChild(0).GetComponent<AccountActionList>().Open(this);
    }
}