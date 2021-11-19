using UnityEngine;
using Zenject;

public class SliceEffectSystem : MonoBehaviour
{
    private ParticleSystem CurrentSystem =>
        type == BeatCubeClass.Type.Bomb ? bombSystem :
        type == BeatCubeClass.Type.Line ? lineSystem : cubeSystem;

    [SerializeField] private ParticleSystem cubeSystem, lineSystem, bombSystem;

    private bool isStarted;

    private BeatCubeClass.Type type;
    private Pool pool;

    [Inject]
    private void Construct(Pool pool)
    {
        this.pool = pool;
    }

    private void Update()
    {
        if (isStarted && CurrentSystem.isPlaying == false)
        {
            pool.Despawn(this);
            isStarted = false;
        }
    }

    public void Play(Vector3 position, float angle, BeatCubeClass.Type type)
    {
        isStarted = true;

        this.type = type;

        CurrentSystem.Play();
        transform.position = position;
        transform.eulerAngles = new Vector3(0, 0, angle);
    }
    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }
    public void Stop()
    {
        CurrentSystem.Stop();
    }

    private void Reset()
    {
        isStarted = false;
    }

    public class Pool : MonoMemoryPool<SliceEffectSystem>
    {
        protected override void Reinitialize(SliceEffectSystem item)
        {
            item.Reset();
        }
    }
}
