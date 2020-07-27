using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace InGame.UI.Menu
{
    [Serializable]
    public class SmartGridLayout : GridLayoutGroup
    {
        [Header("Smart")]
        public Vector2 minCellSize;
        public bool fitContent;

        private RectTransform rect;


        protected override void Awake()
        {
            base.Awake();
            rect = GetComponent<RectTransform>();
        }

        public override void CalculateLayoutInputHorizontal()
        {
            float baseWidth = rectTransform.rect.width;
            float baseHeight = rectTransform.rect.height;
            float cellWidth = minCellSize.x;
            float cellHeight = minCellSize.y;


            // Count of cells which can be placed in row and column with set min size
            int cellsPerRow = Mathf.FloorToInt(baseWidth / cellWidth);
            int cellsPerColumn = Mathf.FloorToInt(baseHeight / cellHeight);

            Vector2 freespace = Vector2.zero;

            if(cellsPerRow != 0)
            {
                float freeSpace = baseWidth - cellsPerRow * cellWidth;

                float freeSpaceForCell = freeSpace / cellsPerRow;

                freespace += new Vector2(freeSpaceForCell, 0);
            }
            if (cellsPerColumn != 0)
            {
                float freeSpace = baseHeight - cellsPerColumn * cellHeight;

                float freeSpaceForCell = freeSpace / cellsPerColumn;

                freespace += new Vector2(0, freeSpaceForCell);
            }

            cellSize = minCellSize + freespace;


            if (fitContent)
            {
                // Is vertical mode
                if (constraint == Constraint.FixedRowCount)
                {
                    rect.sizeDelta = new Vector2(cellWidth * rectChildren.Count, rect.parent.GetComponent<RectTransform>().rect.height);
                }
                else
                {
                    rect.sizeDelta = new Vector2(rect.parent.GetComponent<RectTransform>().rect.width, cellWidth * rectChildren.Count);
                }
                rect.anchoredPosition = Vector2.zero;
            }

            base.CalculateLayoutInputHorizontal();
        }

        public override float preferredHeight
        {
            get
            {
                int cellsPerRow = GetCellsPerRow();
                if (cellsPerRow == 0) return 0;

                int allCells = rectChildren.Count;

                //float heightPerCell = preferredHeight / allCells;
                float heightPerCell = cellSize.y;

                return Mathf.CeilToInt(allCells / (float)cellsPerRow) * heightPerCell;
            }
        }
        public override float preferredWidth
        {
            get
            {
                int cellsPerRow = GetCellsPerRow();
                if (cellsPerRow == 0) return 0;

                int allCells = rectChildren.Count;

                float widthPerCell = cellSize.x;

                int horizontalCellsCount = Mathf.CeilToInt(allCells / (float)cellsPerRow);

                return horizontalCellsCount * widthPerCell;
            }
        }


        private int GetCellsPerRow()
        {
            float baseWidth = rectTransform.rect.width;
            float cellWidth = minCellSize.x;

            // Count of cells which can be placed in row with set min size
            return Mathf.FloorToInt(baseWidth / cellWidth);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Override <see cref="GridLayoutGroup"/> Inspector drawing for appending <see cref="SmartGridLayout"/> fields
    /// </summary>
    [CustomEditor(typeof(SmartGridLayout))]
    public class UIButtonEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            SmartGridLayout t = (SmartGridLayout)target;
        }
    }
#endif
}
