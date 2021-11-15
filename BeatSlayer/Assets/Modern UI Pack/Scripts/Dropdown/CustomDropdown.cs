using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using TMPro;
using System;

namespace Michsky.UI.ModernUIPack
{
    public class CustomDropdown : MonoBehaviour, IPointerExitHandler
    {
        [Header("OBJECTS")]
        public GameObject triggerObject;
        public TextMeshProUGUI selectedText;
        public Image selectedImage;
        public Transform itemParent;
        public GameObject itemObject;
        public GameObject scrollbar;
        private VerticalLayoutGroup itemList;
        private Transform currentListParent;
        public Transform listParent;
        public RectTransform itemListContent;

        [Header("SETTINGS")]
        public bool enableIcon = true;
        public bool enableTrigger = true;
        public bool enableScrollbar = true;
        public bool setHighPriorty = true;
        public bool outOnPointerExit = false;
        public bool isListItem = false;
        public bool invokeAtStart = false;
        public AnimationType animationType;
        public int selectedItemIndex = 0;

        [SerializeField]
        [Header("CONTENT")]
        public List<Item> dropdownItems = new List<Item>();


        public Action<int> OnValueChanged;



        private Animator dropdownAnimator;
        private CanvasGroup dropdownGroup;
        private TextMeshProUGUI setItemText;
        private Image setItemImage;

        Sprite imageHelper;
        string textHelper;
        bool isOn;
        [HideInInspector] public int index = 0;

        public enum AnimationType
        {
            FADING,
            SLIDING,
            STYLISH
        }

        [System.Serializable]
        public class Item
        {
            public string itemName = "Dropdown Item";
            public Sprite itemIcon;
            public UnityEvent OnItemSelection;
        }

        void Start()
        {
            Refresh();
        }
        public void Refresh()
        {
            dropdownAnimator = gameObject.GetComponent<Animator>();
            itemList = itemParent.GetComponent<VerticalLayoutGroup>();
            dropdownGroup = itemListContent.GetComponent<CanvasGroup>();

            foreach (Transform child in itemParent)
                GameObject.Destroy(child.gameObject);

            index = 0;
            for (int i = 0; i < dropdownItems.Count; ++i)
            {
                GameObject go = Instantiate(itemObject, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
                go.transform.SetParent(itemParent, false);

                setItemText = go.GetComponentInChildren<TextMeshProUGUI>();
                textHelper = dropdownItems[i].itemName;
                setItemText.text = textHelper;

                Transform goImage;
                goImage = go.gameObject.transform.Find("Icon");
                setItemImage = goImage.GetComponent<Image>();
                imageHelper = dropdownItems[i].itemIcon;
                setItemImage.sprite = imageHelper;
                setItemImage.gameObject.SetActive(imageHelper != null);

                Button itemButton;
                itemButton = go.GetComponent<Button>();

                if(dropdownItems[i].OnItemSelection != null) itemButton.onClick.AddListener(dropdownItems[i].OnItemSelection.Invoke);
                itemButton.onClick.AddListener(() => Animate(true));
                itemButton.onClick.AddListener(delegate
                {
                    ChangeDropdownInfo(index = go.transform.GetSiblingIndex());
                });

                if (invokeAtStart == true)
                    dropdownItems[i].OnItemSelection.Invoke();
            }

            if(dropdownItems.Count > 0)
            {
                selectedText.text = dropdownItems[selectedItemIndex].itemName;
                selectedImage.sprite = dropdownItems[selectedItemIndex].itemIcon;
                selectedImage.gameObject.SetActive(dropdownItems[selectedItemIndex].itemIcon != null);
            }

            //itemList.GetComponent<RectTransform>().sizeDelta = new Vector2(itemList.GetComponent<RectTransform>().sizeDelta.x, dropdownItems.Count * itemObject.GetComponent<RectTransform>().sizeDelta.y);

            if (enableScrollbar == true)
            {
                itemList.padding.right = 25;
                scrollbar.SetActive(true);
            }

            else
            {
                itemList.padding.right = 8;
                scrollbar.SetActive(false);
            }

            //if (enableIcon == false)
            //    selectedImage.gameObject.SetActive(false);
            //else
            //    selectedImage.gameObject.SetActive(true);

            currentListParent = transform.parent;
            dropdownGroup.alpha = 0;
        }

        public void ChangeDropdownInfo(int itemIndex)
        {
            selectedImage.sprite = dropdownItems[itemIndex].itemIcon;
            selectedImage.gameObject.SetActive(dropdownItems[itemIndex].itemIcon != null);

            OnValueChanged?.Invoke(itemIndex);

            selectedText.text = dropdownItems[itemIndex].itemName;
            selectedItemIndex = itemIndex;
        }

        public void Animate(bool skipAnimation)
        {
            if (skipAnimation)
            {
                dropdownGroup.alpha = isOn ? 0 : 1;

                if (isOn)
                {
                    if (isListItem == true)
                        gameObject.transform.SetParent(currentListParent, true);
                }
                else
                {
                    if (isListItem == true)
                        gameObject.transform.SetParent(listParent, true);
                }

                isOn = !isOn;
            }
            else
            {
                if (isOn == false && animationType == AnimationType.FADING)
                {
                    dropdownAnimator.Play("Fading In");
                    isOn = true;

                    if (isListItem == true)
                        gameObject.transform.SetParent(listParent, true);
                }

                else if (isOn == true && animationType == AnimationType.FADING)
                {
                    dropdownAnimator.Play("Fading Out");
                    isOn = false;

                    if (isListItem == true)
                        gameObject.transform.SetParent(currentListParent, true);
                }

                else if (isOn == false && animationType == AnimationType.SLIDING)
                {
                    dropdownAnimator.Play("Sliding In");
                    isOn = true;

                    if (isListItem == true)
                        gameObject.transform.SetParent(listParent, true);
                }

                else if (isOn == true && animationType == AnimationType.SLIDING)
                {
                    dropdownAnimator.Play("Sliding Out");
                    isOn = false;

                    if (isListItem == true)
                        gameObject.transform.SetParent(currentListParent, true);
                }

                else if (isOn == false && animationType == AnimationType.STYLISH)
                {
                    dropdownAnimator.Play("Stylish In");
                    isOn = true;

                    if (isListItem == true)
                        gameObject.transform.SetParent(listParent, true);
                }

                else if (isOn == true && animationType == AnimationType.STYLISH)
                {
                    dropdownAnimator.Play("Stylish Out");
                    isOn = false;

                    if (isListItem == true)
                        gameObject.transform.SetParent(currentListParent, true);
                }
            }

            

            if (enableTrigger == true && isOn == false)
                triggerObject.SetActive(false);

            else if (enableTrigger == true && isOn == true)
                triggerObject.SetActive(true);

            if (outOnPointerExit == true)
                triggerObject.SetActive(false);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (outOnPointerExit == true)
            {
                if (isOn == true)
                {
                    Animate(true);
                    isOn = false;
                }

                if (isListItem == true)
                    gameObject.transform.SetParent(currentListParent, true);
            }
        }

        public void UpdateValues()
        {
            if (enableScrollbar == true)
            {
                itemList.padding.right = 25;
                scrollbar.SetActive(true);
            }

            else
            {
                itemList.padding.right = 8;
                scrollbar.SetActive(false);
            }

            if (enableIcon == false)
                selectedImage.gameObject.SetActive(false);
            else
                selectedImage.gameObject.SetActive(true);
        }
    }
}