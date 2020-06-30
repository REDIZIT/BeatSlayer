using InGame.Game.Spawn;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class BeatCube : MonoBehaviour, IBeat
{
    public Transform Transform { get { return transform == null ? null : transform; } }


    public BeatCubeClass cls;
    public BeatCubeClass GetClass() { return cls; }

    public MeshRenderer renderer;
    public MeshFilter filter;
    public ParticleSystem psystem;

    public Mesh pointMesh;

    BeatManager bm;
    GameManager gm;

    public float maxDistance;

    /// <summary>
    /// Multiplier of cube calculated speed from 0 to 1
    /// </summary>
    public float SpeedMultiplier { get; set; }
    public float CurrentSpeed { get { return bm.CubeSpeed * cls.speed; } }

    bool useSoundEffect;

    float materialThreshold = 0.5f;
    float thresholdChange = -1;

    Stopwatch w = new Stopwatch();

    bool isDead;

    public void Setup(GameManager gm, bool useSoundEffect, BeatCubeClass cls, float cubesSpeed, BeatManager bm)
    {
        w.Start();

        this.gm = gm;
        this.useSoundEffect = useSoundEffect;
        this.cls = cls;
        SpeedMultiplier = 1;
        this.bm = bm;

        if(cls.type == BeatCubeClass.Type.Point)
        {
            filter.mesh = pointMesh;
        }
        else
        {
            if (cls.subType == BeatCubeClass.SubType.Random)
            {
                int rnd = Random.Range(0, 8);
                cls.subType = (BeatCubeClass.SubType)rnd;
            }
            
            float angle = (int)cls.subType * 45;
            transform.eulerAngles = new Vector3(0, 0, angle);
        }

        psystem.Stop();

        renderer.materials[0].SetFloat("_Threshold", materialThreshold);
        renderer.materials[1].SetFloat("_Threshold", materialThreshold);
        renderer.materials[2].SetFloat("_Threshold", materialThreshold);


        if(cls.saberType != 0)
        {
            Color saberColor = cls.saberType == 1 ? bm.rightSaberColor : bm.leftSaberColor;
            Color arrowColor = cls.saberType == 1 ? bm.rightArrowColor : bm.leftArrowColor;

            if(cls.saberType == 1)
            {
                //saberColor *= (1 + SSytem.instance.GlowPowerCubeRight / 15f);

                //saberColor *= 1 + Mathf.Pow(2, SSytem.instance.GlowPowerCubeRight / 75f);

                float intensity = (saberColor.r + saberColor.g + saberColor.b) / 3f;
                float factor = (1 + SSytem.instance.GlowPowerCubeRight / 25f) / intensity;
                saberColor *= new Color(saberColor.r * factor, saberColor.g * factor, saberColor.b * factor, saberColor.a);
            }
            else
            {
                //saberColor *= (1 + SSytem.instance.GlowPowerCubeLeft / 15f);

                //saberColor *= 1 + Mathf.Pow(2, SSytem.instance.GlowPowerCubeLeft / 75f);

                float intensity = (saberColor.r + saberColor.g + saberColor.b) / 3f;
                //float factor = 1f / intensity;
                float factor = (1 + SSytem.instance.GlowPowerCubeRight / 25f) / intensity;
                saberColor *= new Color(saberColor.r * factor, saberColor.g * factor, saberColor.b * factor, saberColor.a);
            }

            renderer.materials[1].SetColor("_Color", saberColor);
            renderer.materials[1].SetColor("_EmissionColor", saberColor);
            renderer.materials[2].SetColor("_Color", arrowColor * 2);
            renderer.materials[2].SetColor("_EmissionColor", arrowColor * 2);
        }


        if (cls.road == -1) cls.road = Random.Range(0, 3);


        float y = cls.level == 0 ? 0.8f : 4.6f;
        Vector3 pos = new Vector3(-3.5f + cls.road * 2.25f, y, 100);
        transform.position = pos;
    }

    void Update()
    {
        if (isDead) SlicedUpdate();

        Movement();
        Animate();
    }


    public void OnPoint(Vector2 direction, bool destroy = false)
    {
        if (destroy)
        {
            Slice();
            return;
        }

        if (direction.normalized == Vector2.zero) return;


        if (cls.type == BeatCubeClass.Type.Point)
        {
            Slice();
        }
        else
        {
            direction = new Vector3(-direction.x, direction.y);
            
            
            float angle = Mathf.Atan2(direction.x, direction.y);
            float degrees = Mathf.Rad2Deg * angle;
            if (degrees < 0) degrees = 360 + degrees;

            
            float i = Mathf.Repeat((int) cls.subType + 4, 8);
            float targetDeg = i * 45;
            //float targetDeg = (int) cls.subType * 45;
            float anglediff = (degrees - targetDeg + 180 + 360) % 360 - 180;
            
            if (anglediff <= 45 && anglediff >= -45)
            {
                Slice();
            }
            /*float angle = Mathf.Atan2(direction.x, direction.y);
            float degrees = 180 - (angle * 180f / Mathf.PI);
            //if (degrees < 0) degrees = 360 + degrees;

            float targetDeg = (int) cls.subType * 45;
            //if (targetDeg < 0) targetDeg = 360 + targetDeg;

            Debug.Log(targetDeg - degrees);
            if (Mathf.Abs(targetDeg - degrees) <= 45)
            {
                Slice();
            }*/
        }
    }

    public void Destroy()
    {
        Slice();
    }

    void Slice()
    {
        if (isDead) return;
        isDead = true;

        gm.BeatCubeSliced(this);

        w.Stop();
        Debug.Log("Life time " + w.ElapsedMilliseconds);

        OnSlice();
    }



    void Animate()
    {
        materialThreshold += Time.deltaTime * thresholdChange; 
        if (materialThreshold< -0.1f) {materialThreshold = -0.1f; thresholdChange = 0; }
        else if (materialThreshold > 1) materialThreshold = 1;

        renderer.materials[0].SetFloat("_Threshold", materialThreshold);
        renderer.materials[1].SetFloat("_Threshold", materialThreshold);
        renderer.materials[2].SetFloat("_Threshold", materialThreshold);
    }

    void Movement()
    {
        //
        // Время за которое куб должен достичь игрока = bitCubeEndTime /replay.musicSpeed;
        // Расстояние от спавна до места разреза
        // Время за которое куб его проходит
        // 
        //
        //transform.position += new Vector3(0, 0, -1) * speed * Time.deltaTime;
        transform.position += new Vector3(0, 0, -1) * CurrentSpeed * SpeedMultiplier;
        if (transform.position.z <= maxDistance && !isDead)
        {
            gm.MissedBeatCube(this);

            w.Stop();
            Debug.Log("Life time " + w.ElapsedMilliseconds);

            Destroy(gameObject);
        }
    }

    void OnSlice()
    {
        psystem.gameObject.SetActive(true);
        psystem.transform.parent = null;

        thresholdChange = 4;
        foreach (Material mat in renderer.materials)
        {
            mat.SetColor("_Color", new Color(
                mat.color.r / 10f,
                mat.color.g / 10f,
                mat.color.b / 10f));
        }

        GetComponent<BoxCollider>().enabled = false;
    }

    void SlicedUpdate()
    {
        SpeedMultiplier -= (SpeedMultiplier - Time.deltaTime) / 8f;
        if(materialThreshold >= 1)
        {
            Destroy(gameObject);
        }
    }
}

public interface IBeat
{
    Transform Transform { get; }
    void Setup(GameManager gm, bool useSliceSound, BeatCubeClass cls, float cubesSpeed, BeatManager bm);
    void OnPoint(Vector2 direction, bool destroy = false);
    void Destroy();
    BeatCubeClass GetClass();

    /// <summary>
    /// Range from 0 to 1 preferred
    /// </summary>
    float SpeedMultiplier { get; set; }
}