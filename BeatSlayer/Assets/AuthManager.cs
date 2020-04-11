using Pixelplacement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{
    [HideInInspector] public AccountManager manager;

    public State mainScreen;
    public GameObject locker;
    public Button accountBtn;
    [Header("Log in")]
    public InputField login_nickname;
    public InputField login_password;
    public Button login_btn, login_switchBtn;
    [Header("Sign up")]
    public InputField sign_nickname;
    public InputField sign_password, sign_confirm;
    public Button sign_btn, sign_switchBtn;


    public string request;
    public string response;


    private void Start()
    {
        manager = Camera.main.GetComponent<MenuScript_v2>().accountManager;
        string filepath = Application.persistentDataPath + "/session.txt";
        if (File.Exists(filepath))
        {
            string content = File.ReadAllText(filepath);
            string nick = content.Split(':')[0];
            string password = content.Split(':')[1];
            request = "LogIn";
            manager.LogIn(nick, password, this);
        }
        else
        {
            //locker.SetActive(true);
        }
    }
    private void Update()
    {
        if (response == "") return;

        string result = response;
        string req = request;
        response = "";
        request = "";


        if (req == "LogIn")
        {
            login_btn.interactable = true;
            login_switchBtn.interactable = true;
            if (result.Contains("[ERR]"))
            {
                if (result.ToLower().Contains("nick"))
                {
                    login_nickname.transform.GetChild(login_nickname.transform.childCount - 1).gameObject.SetActive(true);
                    login_nickname.transform.GetChild(login_nickname.transform.childCount - 1).GetComponentInChildren<Text>().text = result.Replace("[ERR] ", "");
                    return;
                }
                else
                {
                    login_password.transform.GetChild(login_password.transform.childCount - 1).gameObject.SetActive(true);
                    login_password.transform.GetChild(login_password.transform.childCount - 1).GetComponentInChildren<Text>().text = result.Replace("[ERR] ", "");
                    return;
                }
            }
            else
            {
                accountBtn.interactable = true;
                locker.SetActive(false);

                string filepath = Application.persistentDataPath + "/session.txt";
                File.WriteAllText(filepath, AccountManager.account.nick + ":" + AccountManager.account.password);
            }
        }
        else if(req == "SignUp")
        {
            sign_btn.interactable = true;
            sign_switchBtn.interactable = true;
            sign_nickname.interactable = true;
            sign_password.interactable = true;
            sign_confirm.interactable = true;

            if (result.Contains("[ERR]"))
            {
                if (result.ToLower().Contains("nick"))
                {
                    sign_nickname.transform.GetChild(sign_nickname.transform.childCount - 1).gameObject.SetActive(true);
                    sign_nickname.transform.GetChild(sign_nickname.transform.childCount - 1).GetComponentInChildren<Text>().text = result.Replace("[ERR] ", "");
                }
            }
            else
            {
                request = "LogIn";
                manager.LogIn(sign_nickname.text, sign_password.text, this);
            }
        }
    }


    


    public void OnAccountBtnClicked()
    {
        if (!(Application.internetReachability != NetworkReachability.NotReachable)) return;

        if (AccountManager.account == null)
        {
            locker.SetActive(true);
            locker.transform.GetChild(1).gameObject.SetActive(true);
            locker.transform.GetChild(2).gameObject.SetActive(false);
            locker.transform.GetChild(3).gameObject.SetActive(false);
        }
        else
        {
            manager.LoadAccountPage();
        }
    }

    public void OnLogInBtnClick()
    {
        login_nickname.transform.GetChild(login_nickname.transform.childCount - 1).gameObject.SetActive(false);
        login_password.transform.GetChild(login_password.transform.childCount - 1).gameObject.SetActive(false);

        if (login_nickname.text.Trim() == "") {
            login_nickname.transform.GetChild(login_nickname.transform.childCount - 1).gameObject.SetActive(true);
            login_nickname.transform.GetChild(login_nickname.transform.childCount - 1).GetComponentInChildren<Text>().text = "Empty nickname";
            return;
        }

        if(login_password.text.Trim() == "")
        {
            login_password.transform.GetChild(login_password.transform.childCount - 1).gameObject.SetActive(true);
            login_password.transform.GetChild(login_password.transform.childCount - 1).GetComponentInChildren<Text>().text = "Empty password";
            return;
        }

        request = "LogIn";
        login_btn.interactable = false;
        login_switchBtn.interactable = false;

        manager.LogIn(login_nickname.text, login_password.text, this);
    }

    public void OnSignUpBtnClick()
    {
        sign_nickname.transform.GetChild(sign_nickname.transform.childCount - 1).gameObject.SetActive(false);
        sign_password.transform.GetChild(sign_password.transform.childCount - 1).gameObject.SetActive(false);

        if(sign_nickname.text.Trim() == "")
        {
            sign_nickname.transform.GetChild(sign_nickname.transform.childCount - 1).gameObject.SetActive(true);
            sign_nickname.transform.GetChild(sign_nickname.transform.childCount - 1).GetComponentInChildren<Text>().text = "Empty nickname";
            return;
        }
        else if(ContainsSpecial(sign_nickname.text))
        {
            sign_nickname.transform.GetChild(sign_nickname.transform.childCount - 1).gameObject.SetActive(true);
            sign_nickname.transform.GetChild(sign_nickname.transform.childCount - 1).GetComponentInChildren<Text>().text = "Contains spec. symbols";
            return;
        }

        if(sign_password.text.Trim() == "")
        {
            sign_password.transform.GetChild(sign_password.transform.childCount - 1).gameObject.SetActive(true);
            sign_password.transform.GetChild(sign_password.transform.childCount - 1).GetComponentInChildren<Text>().text = "Empty nickname";
            return;
        }
        else if (ContainsSpecial(sign_password.text))
        {
            sign_password.transform.GetChild(sign_password.transform.childCount - 1).gameObject.SetActive(true);
            sign_password.transform.GetChild(sign_password.transform.childCount - 1).GetComponentInChildren<Text>().text = "Contains spec. symbols";
            return;
        }

        if(sign_password.text != sign_confirm.text)
        {
            sign_confirm.transform.GetChild(sign_confirm.transform.childCount - 1).gameObject.SetActive(true);
            sign_confirm.transform.GetChild(sign_confirm.transform.childCount - 1).GetComponentInChildren<Text>().text = "Incorrect password";
            return;
        }

        request = "SignUp";
        sign_btn.interactable = false;
        sign_switchBtn.interactable = false;
        sign_nickname.interactable = false;
        sign_password.interactable = false;
        sign_confirm.interactable = false;

        manager.SignUp(sign_nickname.text, sign_password.text, this);
    }

    public void OnLogOut()
    {
        mainScreen.ChangeState(mainScreen.gameObject);

        File.Delete(Application.persistentDataPath + "/session.txt");
        File.Delete(Application.persistentDataPath + "/data/account/avatar.pic");

        AccountManager.account = null;
    }




    public bool ContainsSpecial(string str)
    {
        var regexItem = new Regex("^[a-zA-Z0-9 ]*$");
        return !regexItem.IsMatch(str);
    }
}
