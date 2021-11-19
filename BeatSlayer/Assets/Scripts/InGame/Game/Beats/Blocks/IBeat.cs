using InGame.Game.Spawn;
using UnityEngine;
public interface IBeat
{
    Transform Transform { get; }
    void Setup(GameManager gm, BeatCubeClass cls, float cubesSpeed, BeatManager bm);
    void OnPoint(Vector2 direction, bool destroy = false);
    void Destroy();
    BeatCubeClass GetClass();

    /// <summary>
    /// Range from 0 to 1 preferred
    /// </summary>
    float SpeedMultiplier { get; set; }
}