using InGame.Shop;
using UnityEngine;

[CreateAssetMenu(menuName = "Shop/Location")]
public class LocationSO : ShopItemSO
{
    [Tooltip("Id in SODB Location list")]
    public int id;

    [Tooltip("Image showed in map selector")]
    public Sprite image;

    [Tooltip("Location prefab")]
    public GameObject prefab;
}