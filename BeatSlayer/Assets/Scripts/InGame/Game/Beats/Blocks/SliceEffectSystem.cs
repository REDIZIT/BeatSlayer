using System.Linq;
using UnityEngine;
using Zenject;

[RequireComponent(typeof(ParticleSystem))]
public class SliceEffectSystem : MonoBehaviour
{
    private ParticleSystem[] systems;
    private bool isStarted;
    private Pool pool;

    [Inject]
    private void Construct(Pool pool)
    {
        this.pool = pool;
    }
    private void Awake()
    {
        systems = GetComponentsInChildren<ParticleSystem>();
    }

    private void Update()
    {
        if (isStarted && systems.All(c => !c.isPlaying))
        {
            pool.Despawn(this);
        }
    }

    private void Play(Vector3 position, float angle)
    {
        isStarted = true;
        transform.position = position;
        transform.eulerAngles = new Vector3(0, 0, angle);
    }

    public class Pool : MonoMemoryPool<Vector3, float, SliceEffectSystem>
    {
        protected override void Reinitialize(Vector3 position, float angle, SliceEffectSystem item)
        {
            item.Play(position, angle);
        }
    }
}
