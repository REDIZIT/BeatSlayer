using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace InGame.Multiplayer.Lobby.Chat
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextHyperlinkHandler : MonoBehaviour, IPointerClickHandler
    {
        public TextMeshProUGUI textMesh;


        public void OnPointerClick(PointerEventData eventData)
        {
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMesh, Input.mousePosition, null);
            if (linkIndex != -1)
            { 
                // was a link clicked?
                TMP_LinkInfo linkInfo = textMesh.textInfo.linkInfo[linkIndex];

                // open the link id as a url, which is the metadata we added in the text field
                Application.OpenURL(linkInfo.GetLinkID());
            }
        }
    }
}
