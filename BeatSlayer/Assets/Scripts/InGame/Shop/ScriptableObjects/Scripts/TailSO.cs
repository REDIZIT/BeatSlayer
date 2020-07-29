using InGame.Shop;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Tail")]
public class TailSO : ShopItemSO
{
    public int id;

    [Tooltip("Image showed in shop")]
    public Sprite image;

    [Tooltip("Swoosh material with texture")]
    public Material swooshMaterial;
}