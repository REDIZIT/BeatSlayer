using InGame.Game.Spawn;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatCube : MonoBehaviour, IBeat
{
    public BeatCubeClass cls;
    public BeatCubeClass GetClass() { return cls; }

    public MeshRenderer renderer;
    public MeshFilter filter;
    public ParticleSystem psystem;

    public Mesh pointMesh;

    BeatManager bm;
    GameManager gm;

    public float speed;
    public float maxDistance;

    bool useSoundEffect;

    float materialThreshold = 0.5f;
    float thresholdChange = -1;

    bool isDead;

    public void Setup(GameManager gm, bool useSoundEffect, BeatCubeClass cls, float cubesSpeed, BeatManager bm)
    {
        this.gm = gm;
        this.useSoundEffect = useSoundEffect;
        this.cls = cls;
        speed *= cubesSpeed * cls.speed;
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
            renderer.materials[1].SetColor("_Color", saberColor * 2);
            renderer.materials[1].SetColor("_EmissionColor", saberColor * 2);
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


    public void OnPoint(Vector2 direction)
    {
        if (direction.normalized == Vector2.zero) return;

        if (cls.type == BeatCubeClass.Type.Point)
        {
            Slice();
        }
        else
        {
            float angle = Mathf.Atan2(direction.x, direction.y);
            float degrees = 180 - (angle * 180f / Mathf.PI);
            if (degrees < 0) degrees = 360 + degrees;

            float targetDeg = (int) cls.subType * 45;
            if (targetDeg < 0) targetDeg = 360 + targetDeg;

            if (Mathf.Abs(targetDeg - degrees) <= 45)
            {
                Slice();
            }
        }
    }

    void Slice()
    {
        if (isDead) return;
        isDead = true;

        gm.BeatCubeSliced();

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
        transform.position += new Vector3(0, 0, -1) * bm.CubeSpeed * cls.speed;
        if (transform.position.z <= maxDistance && !isDead)
        {
            gm.MissedBeatCube();
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
        speed -= (speed - Time.deltaTime) / 8f;
        if(materialThreshold >= 1)
        {
            Destroy(gameObject);
        }
    }
}

public interface IBeat
{
    void Setup(GameManager gm, bool useSliceSound, BeatCubeClass cls, float cubesSpeed, BeatManager bm);
    void OnPoint(Vector2 direction);
    BeatCubeClass GetClass();
}