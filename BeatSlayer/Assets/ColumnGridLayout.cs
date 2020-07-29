using InGame.Settings;
using UnityEngine;
using UnityEngine.UI;

public class ColumnGridLayout : MonoBehaviour
{
    public bool allowTwoColumns;

    public bool fixHeight;
    public float height;

    [Tooltip("Min cell width, when grid can be splitted for 2 columns")]
    public float widthForDivide;

    private RectTransform rect;
    private GridLayoutGroup group;

    private float layoutedGridWidth = -1;


    private void Awake()
    {
        allowTwoColumns = SettingsManager.Settings.Menu.TwoColumnList;
    }
    private void Start()
    {
        Build();
    }

    private void Update()
    {
        if(layoutedGridWidth != rect.rect.width)
        {
            Build();
        }
    }

    public void Build()
    {
        rect = GetComponent<RectTransform>();
        group = GetComponent<GridLayoutGroup>();

        float gridWidth = rect.rect.width;
        Vector2 cellSize = new Vector2(gridWidth, gridWidth);

        if (allowTwoColumns && gridWidth / 2f >= widthForDivide)
        {
            cellSize = new Vector2(gridWidth / 2, gridWidth / 2);
        }


        Vector2 spacing = group.spacing;

        Vector2 size = new Vector2(cellSize.x - spacing.x, fixHeight ? height : cellSize.y - spacing.y);

        group.cellSize = size;

        layoutedGridWidth = gridWidth;
    }
}
