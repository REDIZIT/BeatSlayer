﻿using InGame.Game.Spawn;
using UnityEngine;

public class BeatLine : MonoBehaviour, IBeat
{
    public Transform Transform { get { return transform; } }
    public BeatCubeClass cls;
    
    public BeatCubeClass GetClass() { return cls; }

    BeatManager bm;
    GameManager gm;

    public Transform firstCap, secondCap;
    public CapsuleCollider lineCollider;
    public Transform cylinder;
    public ParticleSystem psystem;

    float lineEndPosition;
    float lineTime;
    public float secondCapRoadPos;


    public float SpeedMultiplier { get; set; }
    public float CurrentSpeed { get { return bm.CubeSpeedPerFrame * cls.speed; } }


    public Transform firstAnimCap, secondAnimCap;
    Vector3 firstCapRotation, secondCapRotation;

    public void Setup(GameManager gm, BeatCubeClass cls, float cubesSpeed, BeatManager bm)
    {
        this.gm = gm;
        this.cls = cls;
        this.bm = bm;

        float y = cls.level == 0 ? 0.8f : bm.secondHeight;
        Vector3 pos = new Vector3(bm.GetPositionByRoad(cls.road), y, 100);
        transform.position = pos;

        secondCapRoadPos = bm.GetPositionByRoad(cls.road);
        if (cls.linePoints == null || cls.linePoints.Count == 0)
        {
            // Use new road positioning
            // Fixing another road bug (When you set start road 4 and in game end cap goes far away from 4 road to 8 road xD)
            secondCapRoadPos = bm.GetPositionByRoad(cls.lineEndRoad);
        }

        firstCapRotation = new Vector3(GetRandom(), GetRandom(), GetRandom());
        secondCapRotation = new Vector3(GetRandom(), GetRandom(), GetRandom());


        lineTime = cls.linePoints == null || cls.linePoints.Count > 0 ? cls.linePoints[1].z : cls.lineLenght; // Use new or legacy way

        if (lineTime == 0)
        {
            lineTime = 1;
            Debug.LogError("Line BUG! Line end time is zero! Value / 0 = Infinity = Unity BAG!!");
        }

        //capMax = lineEndTime * (CurrentSpeed / Time.deltaTime);

        lineEndPosition = lineTime * /*bm.CubeSpeedPerSecond*/ bm.LineLengthMultiplier;
        //lineEndPosition = lineTime * bm.fieldLength * bm.LineLengthMultiplier;
        //Debug.Log($"{capMax} = {lineEndTime} * ({bm.fieldLength} / {bm.fieldCrossTime}) / {CurrentSpeed}");
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
        transform.position += new Vector3(0, 0, -CurrentSpeed);
        totalDist += CurrentSpeed;
        if (firstCap.position.z <= bm.maxDistance)
        {
            gm.MissedBeatCube(this);
            Destroy(gameObject);
        }
    }
    void CapMovement()
    {
        // Cap speed in units/frame
        float capSpeed = CurrentSpeed;
        
        /// not field length because lines are more longer than should be due to
        /// cube can pass field faster than one second and we can just multiplay seconds on field len
        

        // Offset to selected road second cap at spawn
        float capRoadOffsetTime = lineEndPosition / capSpeed;
        float capRoadOffsetDistance = secondCapRoadPos - transform.position.x;
        float capRoadOffsetSpeed = capRoadOffsetDistance / capRoadOffsetTime;


        secondCap.position += new Vector3(capRoadOffsetSpeed, 0, capSpeed);
        if(secondCap.localPosition.z > lineEndPosition)
        {
            secondCap.localPosition = new Vector3(0, secondCap.localPosition.y, lineEndPosition);
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


    public void OnPoint(Vector2 direction, bool destroy = false)
    {
        if (destroy)
        {
            Destroy(gameObject);
            return;
        }

        float lineEndTime = cls.linePoints.Count > 0 ? cls.linePoints[1].z : cls.lineLenght; // Use new or legacy way
        //float capMax = lineEndTime * bm.fieldLength;

        // Offset to selected road second cap at spawn
        float capRoadOffsetTime = lineEndPosition / CurrentSpeed;
        float capRoadOffsetDistance = secondCapRoadPos - transform.position.x;
        float capRoadOffsetSpeed = capRoadOffsetDistance / capRoadOffsetTime;

        gm.BeatLineHold();
        psystem.Play();

        firstCap.transform.localPosition += new Vector3(capRoadOffsetSpeed, 0, CurrentSpeed);
        if(firstCap.transform.localPosition.z >= lineEndPosition)
        {
            OnLineSliced();
        }
    }

    void OnLineSliced()
    {
        psystem.transform.parent = null;
        psystem.Stop();
        gm.BeatLineSliced(this);
        Destroy(gameObject);
    }

    public void Destroy()
    {
        OnLineSliced();
    }
    
}
