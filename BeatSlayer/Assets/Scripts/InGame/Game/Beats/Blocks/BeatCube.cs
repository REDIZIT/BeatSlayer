using InGame.DI;
using UnityEngine;
using Zenject;

public class BeatCube : Beat
{
    public new MeshRenderer renderer;
    public MeshFilter filter;

    [SerializeField] private ParticleSystem cubeDissovleParticleSystem;

    public Transform markersPivot;
    public MeshRenderer arrowMarker;
    public MeshRenderer pointMarker;

    public Mesh pointMesh;

    private float materialThreshold;
    private float thresholdChange;

    private bool isDead;

    private new BoxCollider collider;

    private Pool pool;
    private SliceEffectSystem.Pool sliceEffectPool;

    [Inject]
    private void Construct(Pool pool, SliceEffectSystem.Pool sliceEffectPool)
    {
        this.pool = pool;
        this.sliceEffectPool = sliceEffectPool;
    }

    public override void Setup(BeatCubeClass cls, float cubesSpeed)
    {
        base.Setup(cls, cubesSpeed);

        if (cls.type == BeatCubeClass.Type.Point)
        {
            filter.mesh = pointMesh;
        }
        else if (cls.type == BeatCubeClass.Type.Dir)
        {
            if (cls.subType == BeatCubeClass.SubType.Random)
            {
                int rnd = Random.Range(0, 8);
                cls.subType = (BeatCubeClass.SubType)rnd;
            }

            float angle = (int)cls.subType * 45;
            transform.eulerAngles = new Vector3(0, 0, angle);
        }

        pointMarker.gameObject.SetActive(cls.type == BeatCubeClass.Type.Point);
        arrowMarker.gameObject.SetActive(cls.type == BeatCubeClass.Type.Dir);

        cubeDissovleParticleSystem.Stop();


        if (cls.saberType != 0)
        {
            Color saberColor = cls.saberType == 1 ? bm.rightSaberColor : bm.leftSaberColor;
            Color arrowColor = cls.saberType == 1 ? bm.rightArrowColor : bm.leftArrowColor;

            if (cls.saberType == 1)
            {
                float intensity = (saberColor.r + saberColor.g + saberColor.b) / 3f;
                float factor = (1 + SSytem.GlowPowerCubeRight / 25f) / intensity;
                saberColor *= new Color(saberColor.r * factor, saberColor.g * factor, saberColor.b * factor, saberColor.a);
            }
            else
            {
                float intensity = (saberColor.r + saberColor.g + saberColor.b) / 3f;
                float factor = (1 + SSytem.GlowPowerCubeRight / 25f) / intensity;
                saberColor *= new Color(saberColor.r * factor, saberColor.g * factor, saberColor.b * factor, saberColor.a);
            }

            foreach (Material material in renderer.materials)
            {
                material.SetColor("_Color", saberColor / 8f);
                material.SetColor("_EmissionColor", saberColor / 8f);
            }

            arrowMarker.material.SetColor("_EmissionColor", arrowColor * 4f);
            pointMarker.material.SetColor("_EmissionColor", arrowColor * 4f);
        }


        if (cls.road == -1) cls.road = Random.Range(0, 3);


        float y = cls.level == 0 ? 0.8f : bm.secondHeight;
        Vector3 pos = new Vector3(bm.GetPositionByRoad(cls.road), y, 100);
        transform.position = pos;
    }

    void Awake()
    {
        collider = GetComponent<BoxCollider>();
    }
    void Update()
    {
        if (isDead) SlicedUpdate();

        Movement();
        Animate();
    }


    public override void OnPoint(Vector2 direction, bool destroy = false)
    {
        if (destroy)
        {
            Slice(0, destroy);
            return;
        }

        if (direction.normalized == Vector2.zero) return;


        direction = new Vector3(-direction.x, direction.y);


        float angle = Mathf.Atan2(direction.x, direction.y);
        float degrees = Mathf.Rad2Deg * angle;
        if (degrees < 0) degrees = 360 + degrees;


        float i = Mathf.Repeat((int)Model.subType + 4, 8);
        float targetDeg = i * 45;

        float anglediff = (degrees - targetDeg + 180 + 360) % 360 - 180;


        if (Model.type == BeatCubeClass.Type.Point || Model.type == BeatCubeClass.Type.Bomb)
        {
            Slice(degrees);
        }
        else if (anglediff <= 45 && anglediff >= -45)
        {
            Slice(degrees);
        }
    }

    public override void Destroy()
    {
        Slice(Random.Range(0, 360));
    }

    void Slice(float angle, bool dissolve = false)
    {
        if (isDead) return;
        isDead = true;

        gm.BeatCubeSliced(this);

        OnSlice(angle, dissolve);
    }



    void Animate()
    {
        materialThreshold += Time.deltaTime * thresholdChange * CurrentSpeed;
        if (materialThreshold < -0.1f) { materialThreshold = -0.1f; thresholdChange = 0; }
        else if (materialThreshold > 1) materialThreshold = 1;

        foreach (Material material in renderer.materials)
        {
            material.SetFloat("_Threshold", materialThreshold);
        }

        arrowMarker.material.SetFloat("_Threshold", materialThreshold);
        pointMarker.material.SetFloat("_Threshold", materialThreshold);

        Quaternion rotation = Quaternion.LookRotation((transform.position - gm.transform.position).normalized);
        transform.rotation = rotation;

        float angle = (int)Model.subType * 45;
        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, angle);
    }

    void Movement()
    {
        //
        // Время за которое куб должен достичь игрока = bitCubeEndTime /replay.musicSpeed;
        // Расстояние от спавна до места разреза
        // Время за которое куб его проходит
        // 
        //
        transform.position += new Vector3(0, 0, -1) * CurrentSpeed/* * SpeedMultiplier*/;
        if (transform.position.z <= bm.maxDistance && !isDead)
        {
            gm.MissedBeatCube(this);

            pool.Despawn(this);
        }
    }

    void OnSlice(float angle, bool dissolve = false)
    {
        if (dissolve)
        {
            cubeDissovleParticleSystem.gameObject.SetActive(true);
            cubeDissovleParticleSystem.transform.parent = null;
        }
        else
        {
            sliceEffectPool.Spawn().Play(transform.position, angle, Model.type);
        }

        thresholdChange = 4;
        foreach (Material mat in renderer.materials)
        {
            mat.SetColor("_Color", new Color(
                mat.color.r / 10f,
                mat.color.g / 10f,
                mat.color.b / 10f));
        }

        collider.enabled = false;
    }

    void SlicedUpdate()
    {
        SpeedMultiplier -= (SpeedMultiplier - Time.deltaTime) / 8f;
        if (materialThreshold >= 1)
        {
            pool.Despawn(this);
        }
    }

    public override void Reset()
    {
        collider.enabled = true;
        isDead = false;

        SpeedMultiplier = 1;

        materialThreshold = 0.5f;
        thresholdChange = -1;
        foreach (Material material in renderer.materials)
        {
            material.SetFloat("_Threshold", materialThreshold);
        }
    }

    public class Pool : BeatPool
    {
    }
}
