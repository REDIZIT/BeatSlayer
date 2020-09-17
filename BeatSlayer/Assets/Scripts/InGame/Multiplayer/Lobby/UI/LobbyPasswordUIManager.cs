using System;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.Multiplayer.Lobby.UI
{
    public class LobbyPasswordUIManager : MonoBehaviour
    {
        public static LobbyPasswordUIManager instance;

        public GameObject locker;
        public InputField passwordField;

        private Lobby lobby;
        private Action callback;

        private void Awake()
        {
            instance = this;
        }

        public void ShowPasswordLocker(Lobby lobby, Action correctCallback)
        {
            this.lobby = lobby;
            callback = correctCallback;

            locker.SetActive(true);
            passwordField.text = "";
        }

        public void OnCloseBtnClick()
        {
            locker.SetActive(false);
            passwordField.text = "";
        }
        
        public void OnPasswordFieldChanged()
        {
            if (passwordField.text == lobby.Password)
            {
                OnCloseBtnClick();
                callback?.Invoke();
            }
        }
    }
}
