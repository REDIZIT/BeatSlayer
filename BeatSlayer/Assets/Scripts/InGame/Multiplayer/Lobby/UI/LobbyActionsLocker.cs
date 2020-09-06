using BeatSlayerServer.Multiplayer.Accounts;
using GameNet;
using InGame.Animations;
using Profile;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using Web;

namespace InGame.Multiplayer.Lobby.UI
{
    public class LobbyActionsLocker : MonoBehaviour
    {
        public static LobbyActionsLocker instance;

        [Header("Components")]
        public LobbyUIManager lobbyUI;
        public ProfileUI profileUI;

        [Header("UI")]
        public GameObject locker;

        public RawImage avatarImage;
        public Text nickText, rankText;

        public GameObject profileBtn, friendInviteBtn, hostRightsBtn, kickBtn;

        public AccountData account;

        private void Awake()
        {
            if (instance == null) instance = this;
            else Destroy(this);
        }
        public void Show(ConnectedPlayer player, bool amIHost)
        {
            locker.SetActive(true);
            nickText.text = player.Nick;
            rankText.text = "";


            profileBtn.SetActive(true);

            friendInviteBtn.SetActive(player.Nick != Payload.Account.Nick && Payload.Account.Friends.All(c => c.Nick != player.Nick));
            friendInviteBtn.GetComponent<Button>().interactable = true;

            hostRightsBtn.SetActive(player.Nick != Payload.Account.Nick && amIHost);
            hostRightsBtn.GetComponent<Button>().interactable = true;

            kickBtn.SetActive(player.Nick != Payload.Account.Nick && amIHost);


            // Load avatar and account data
            WebAPI.GetAvatar(player.Nick, (Texture2D tex) => avatarImage.texture = tex);

            Task.Run(async () =>
            {
                account = await NetCore.ServerActions.Account.GetAccountByNick(player.Nick);

                UnityMainThreadDispatcher.Instance().Enqueue(() =>
                {
                    rankText.text = "#" + account.PlaceInRanking;
                });
            });
        }

        public void ShowProfile()
        {
            if (account == null) return;
            profileUI.ShowAccount(account);
            Close();
        }
        public void SendFriendInvite()
        {
            NetCore.ServerActions.Friends.InviteFriend(account.Nick, account.Nick);
            friendInviteBtn.GetComponent<Button>().interactable = false;
            Close();
        }
        public void GiveHostRights()
        {
            lobbyUI.GiveHostRights(account.Nick);
            Close();
        }
        public void Kick()
        {
            lobbyUI.Kick(account.Nick);
            Close();
        }

        public void Close()
        {
            account = null;
            locker.SetActive(false);
        }
    }
}
