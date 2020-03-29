using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AccountActionList : MonoBehaviour
{
    AccountItemButton btn;

    public void Open(AccountItemButton btn)
    {
        GetComponent<RectTransform>().position = Input.mousePosition;
        this.btn = btn;
    }
    public void Close()
    {
        btn = null;
        transform.parent.gameObject.SetActive(false);
    }




    public void OnActionButtonClick(Button button)
    {
        if(button.name == "OpenProfile")
        {
            btn.manager.LoadAnotherAccountPage(btn.nick);
        }

        Close();
    }
}
