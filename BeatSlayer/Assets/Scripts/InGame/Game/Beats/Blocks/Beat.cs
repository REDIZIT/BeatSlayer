using InGame.Game.Spawn;
using UnityEngine;
using Zenject;

public abstract class Beat : MonoBehaviour
{
    public Transform Transform => transform;
    public BeatCubeClass Model { get; protected set; }

    /// <summary>
    /// Range from 0 to 1 preferred
    /// </summary>
    public float SpeedMultiplier { get; set; } = 1;

    public float CurrentSpeed { get { return bm.CubeSpeedPerFrame * Model.speed; } }


    protected GameManager gm;
    protected BeatManager bm;

    [Inject]
    private void Construct(GameManager gm, BeatManager bm)
    {
        this.gm = gm;
        this.bm = bm;
    }

    public virtual void Setup(BeatCubeClass cls, float cubesSpeed)
    {
        Model = cls;
    }


    public abstract void Reset();
    
    public abstract void OnPoint(Vector2 direction, bool destroy = false);
    
    public abstract void Destroy();
}
