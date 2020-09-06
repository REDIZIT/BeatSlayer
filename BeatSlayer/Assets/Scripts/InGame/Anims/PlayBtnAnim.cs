using InGame.UI.Overlays;
using Pixelplacement;
using UnityEngine;

namespace InGame.Animations
{
    public class PlayBtnAnim : MonoBehaviour
    {
        public Animator anim;
        public GameObject mainUI;
        public MusicFilesUI musicFilesUI;
        public OwnMusicUI ownMusicUI;
        public PageController pager;
        
        
        public GameObject authorPage, ownPage;
        public GameObject overlay;

        [Header("Pages")]
        public StateMachine stateMachine;
        public GameObject mainState;

        [Header("Lobby")]
        public State lobbyState;
        public GameObject roomsPage, lobbyPage;

        private bool isExtended;

        public void OnBtnClick()
        {
            anim.Play(isExtended ? "Hide" : "Show");

            isExtended = !isExtended;
        }

        public void OpenMainPage()
        {
            stateMachine.ChangeState(mainState);
        }

        public void OnAuthorBtnClick()
        {
            overlay.SetActive(true);
            mainUI.SetActive(false);
            
            authorPage.SetActive(true);
            ownPage.SetActive(false);

            pager.ShowAuthosViews();
        }

        public void OnOwnBtnClick()
        {
            /*state.ChangeState(1);
            snap.StartingScreen = 1;
            snap.ChangePage(0);*/
            //snap.GoToScreen(0);
            
            overlay.SetActive(true);
            mainUI.SetActive(false);
            
            authorPage.SetActive(false);
            ownPage.SetActive(true);

            //musicFilesUI.Refresh();

            //ownMusicUI.OnOwnBtnClicked(() => { });
            pager.ShowScrollViewOwn();
        }

        public void OpenMultiplayerPage()
        {
            lobbyState.ChangeState(lobbyState.gameObject);
            roomsPage.SetActive(true);
            lobbyPage.SetActive(false);
        }
        public void OpenLobbyPage()
        {
            roomsPage.SetActive(false);
            lobbyPage.SetActive(true);
        }


        public void OnBackBtnClick()
        {
            overlay.SetActive(false);
            mainUI.SetActive(true);
        }
    }
}