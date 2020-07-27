using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI.Menu.Sidebar
{
    [RequireComponent(typeof(Toggle))]
    public class SidebarItem : MonoBehaviour
    {
        [SerializeField] private Animator animator;

        private Toggle toggle;

        private void Awake()
        {
            toggle = GetComponent<Toggle>();
        }

        private void Start()
        {
            OnToggleChanged(true);
        }

        public void OnToggleValueChanged()
        {
            OnToggleChanged(false);
        }
        private void OnToggleChanged(bool force = false)
        {
            if (force)
            {
                animator.SetBool("Enabled", toggle.isOn);
                animator.StopPlayback();
                animator.Play(toggle.isOn ? "ForceSelect" : "ForceDeselect");
            }
            else
            {
                animator.SetBool("Enabled", toggle.isOn);
            }
        }
    }
}
