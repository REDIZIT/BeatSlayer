using InGame.Shop;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Saber")]
public class SaberSO : ShopItemSO
{
    public int id;

    [Tooltip("Image showed in shop")]
    public Sprite image;

    [Tooltip("Prefab of saber model")]
    public GameObject model;
}