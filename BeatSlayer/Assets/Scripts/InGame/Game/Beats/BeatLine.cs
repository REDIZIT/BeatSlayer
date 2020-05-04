using InGame.Game.Spawn;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class BeatLine : MonoBehaviour, IBeat
{
    public BeatCubeClass cls;
    public BeatCubeClass GetClass() { return cls; }

    BeatManager bm;
    GameManager gm;

    public Transform firstCap, secondCap;
    public CapsuleCollider lineCollider;
    public Transform cylinder;
    public ParticleSystem psystem;

    public float maxDistance = -20;
    public float secondCapRoadPos;

    bool useSoundEffect;

    public Transform firstAnimCap, secondAnimCap;
    Vector3 firstCapRotation, secondCapRotation;

    public void Setup(GameManager gm, bool useSoundEffect, BeatCubeClass cls, float cubesSpeed, BeatManager bm)
    {
        this.gm = gm;
        this.useSoundEffect = useSoundEffect;
        this.cls = cls;
        this.bm = bm;

        float y = cls.level == 0 ? 0.8f : 4.6f;
        Vector3 pos = new Vector3(-3.5f + cls.road * 2.25f, y, 100);
        transform.position = pos;

        secondCapRoadPos = -3.5f + cls.road * 2.25f;
        if (cls.linePoints.Count == 0)
        {
            // Use new road positioning
            secondCapRoadPos = -3.5f + cls.lineEndRoad * 2.25f;
        }

        firstCapRotation = new Vector3(GetRandom(), GetRandom(), GetRandom());
        secondCapRotation = new Vector3(GetRandom(), GetRandom(), GetRandom());
    }

    void Update()
    {
        Movement();

        CapMovement();

        AnimateCaps();
    }

    public float totalDist;
    void Movement()
    {
        transform.position += new Vector3(0, 0, -bm.CubeSpeed * cls.speed);
        totalDist += bm.CubeSpeed * cls.speed;
        if (firstCap.position.z <= maxDistance)
        {
            gm.MissedBeatCube();
            Destroy(gameObject);
        }
    }
    void CapMovement()
    {
        // Cap speed in units/frame
        float capSpeed = bm.CubeSpeed;
        float lineEndTime = cls.linePoints.Count > 0 ? cls.linePoints[1].z : cls.lineLenght; // Use new or legacy way
        float capMax = lineEndTime * bm.fieldLength;

        // Offset to selected road second cap at spawn
        float capRoadOffsetTime = capMax / capSpeed;
        float capRoadOffsetDistance = secondCapRoadPos - transform.position.x;
        float capRoadOffsetSpeed = capRoadOffsetDistance / capRoadOffsetTime;

        
        secondCap.position += new Vector3(capRoadOffsetSpeed, 0, capSpeed);
        if(secondCap.localPosition.z > capMax)
        {
            secondCap.localPosition = new Vector3(0, secondCap.localPosition.y, capMax);
            secondCap.position = new Vector3(secondCapRoadPos, secondCap.position.y, secondCap.position.z);
        }



        float colliderZ = (firstCap.position.z + secondCap.position.z) / 2f;
        lineCollider.transform.position = new Vector3(lineCollider.transform.position.x, lineCollider.transform.position.y, colliderZ);

        float capsDistance = secondCap.position.z - firstCap.position.z;
        lineCollider.height = capsDistance + lineCollider.radius * 2;




        // Update cylinder between caps
        UpdateCylinder();
    }
    void UpdateCylinder()
    {
        float averageZ = (firstCap.position.z + secondCap.position.z) / 2f;
        float averageY = (firstCap.position.y + secondCap.position.y) / 2f;
        float averageX = (firstCap.position.x + secondCap.position.x) / 2f;

        float distance = Vector3.Distance(firstCap.position, secondCap.position);

        Vector3 cylinderPos = new Vector3(averageX, averageY, averageZ);

        //cylinder.position = new Vector3(cylinder.position.x, cylinder.position.y, colliderZ);
        cylinder.position = cylinderPos;
        cylinder.localScale = new Vector3(cylinder.localScale.x, distance / 2f - lineCollider.radius, cylinder.localScale.z);

        cylinder.LookAt(secondCap.position);
        cylinder.localEulerAngles = new Vector3(90, cylinder.localEulerAngles.y, 0);

        lineCollider.transform.localEulerAngles = cylinder.localEulerAngles;
        lineCollider.transform.position = cylinder.position;
    }
    void AnimateCaps()
    {
        firstAnimCap.eulerAngles += firstCapRotation * 200 * Time.deltaTime;
        secondAnimCap.eulerAngles += secondCapRotation * 200 * Time.deltaTime;
    }
    public float GetRandom()
    {
        float value = Random.Range(-1, 1);
        if (value > 0) value += 0.5f;
        else if (value < 0) value -= 0.5f;
        else value = GetRandom();

        return value;
    }


    public void OnPoint(Vector2 direction)
    {
        float lineEndTime = cls.linePoints.Count > 0 ? cls.linePoints[1].z : cls.lineLenght; // Use new or legacy way
        float capMax = lineEndTime * bm.fieldLength;

        // Offset to selected road second cap at spawn
        float capRoadOffsetTime = capMax / bm.CubeSpeed;
        float capRoadOffsetDistance = secondCapRoadPos - transform.position.x;
        float capRoadOffsetSpeed = capRoadOffsetDistance / capRoadOffsetTime;

        gm.BeatLineSliced();
        psystem.Play();

        firstCap.transform.localPosition += new Vector3(capRoadOffsetSpeed, 0, bm.CubeSpeed);
        if(firstCap.transform.localPosition.z >= capMax)
        {
            psystem.transform.parent = null;
            psystem.Stop();
            Destroy(gameObject);
        }
    }
}
