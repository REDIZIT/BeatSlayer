using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColumnGridLayout : MonoBehaviour
{
    public bool fixHeight;
    public float height;

    private void Start()
    {
        float width = this.gameObject.GetComponent<RectTransform>().rect.width;
        Vector2 newSize = new Vector2(width / 2, width / 2);
        Vector2 spacing = GetComponent<GridLayoutGroup>().spacing;

        //float defaultHeight = GetComponent<GridLayoutGroup>().cellSize.y;

        Vector2 size = new Vector2(newSize.x - spacing.x, fixHeight ? height : newSize.y - spacing.y);

        GetComponent<GridLayoutGroup>().cellSize = size;
    }
}
