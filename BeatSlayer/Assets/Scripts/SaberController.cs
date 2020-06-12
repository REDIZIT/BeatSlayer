using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaberController : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public MeleeWeaponTrail Trail => GetComponent<MeleeWeaponTrail>();

    public bool isUsing;

    public float camOffset, camScale;

    public Transform rightHand;
    public int saberSkinId;

    public Material[] swooshes;

    Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

    public void SetSword(Vector3 pos)
    {
        //transform.GetChild(0).eulerAngles = new Vector3(transform.GetChild(0).eulerAngles.x, transform.GetChild(0).eulerAngles.y, (float)GetSwordAngle(transform.localPosition, pos));
        //transform.GetChild(0).eulerAngles = new Vector3(transform.GetChild(0).eulerAngles.x, transform.GetChild(0).eulerAngles.y, curve.Evaluate(pos.x / Screen.width) * 45);

        if (!transform.GetChild(saberSkinId).gameObject.activeSelf)
        {
            Trail.DontUseThisFrame = true;
        }

        float xScale = (Screen.width > Screen.height ? camScale : camScale / 2f);
        float yScale = (Screen.width < Screen.height ? camScale : camScale / 2f);

        transform.localPosition = cam.ScreenToViewportPoint(new Vector3((pos.x - Screen.width / 2f) * xScale, (pos.y - Screen.height / 2f) * yScale) + new Vector3(0, 0, camOffset));
        transform.LookAt(rightHand);
    }

    double GetSwordAngle(Vector3 a, Vector3 b)
    {
        float xDiff = b.x - a.x;
        float yDiff = b.y - a.y;
        return Math.Atan2(yDiff, xDiff) * 180.0 / Math.PI;
    }

    public void Init(Color clr, int skinId, int swooshEffectId)
    {
        saberSkinId = skinId;
        GetComponent<MeleeWeaponTrail>()._colors[0] = clr;
        GetComponent<MeleeWeaponTrail>()._material = swooshes[swooshEffectId];


        foreach (Transform child in transform) child.gameObject.SetActive(false);

        MeshRenderer[] renderers = transform.GetChild(skinId).GetComponentsInChildren<MeshRenderer>();
        foreach(MeshRenderer rend in renderers)
        {
            foreach(Material mat in rend.materials)
            {
                if (mat.name.Contains("Saber_Laser"))
                {
                    mat.SetColor("_EmissionColor", clr);
                }
            }
        }

        foreach(Transform child in transform.GetChild(skinId).GetComponentInChildren<Transform>())
        {
            if(child.name == "Top")
            {
                GetComponent<MeleeWeaponTrail>()._base = child;
            }
            else if (child.name == "Bottom")
            {
                GetComponent<MeleeWeaponTrail>()._tip = child;
            }
        }

        //top.localPosition = new Vector3(top.localPosition.x, topY, topZ);
    }

    public void SetEnabled(bool enabled)
    {
        transform.GetChild(saberSkinId).gameObject.SetActive(enabled);
        isUsing = enabled;
    }

    void AnimateLineRenderer()
    {
        if (lineRenderer.positionCount != 60)
        {
            lineRenderer.positionCount = 60;
        }


        Vector3[] newPoses = new Vector3[lineRenderer.positionCount];
        Debug.Log(newPoses.Length);

        //Vector3 prevPos = Vector3.zero;
        //Vector3 prevDir = Vector3.zero;

        //Vector3 v = prevPos - transform.position;
        //bool b = true;
        //if (v.z < 0)
        //{
        //    b = false;
        //    for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        //    {
        //        newPoses[i + 1] = lineRenderer.GetPosition(i);
        //    }
        //    return;
        //}
        newPoses[0] = lineRenderer.transform.position;

        for (int i = 0; i < lineRenderer.positionCount - 1; i++)
        {
            //Vector3 curPos = lineRenderer.GetPosition(i);
            newPoses[i + 1] = lineRenderer.GetPosition(i);
            if (i != 0)
            {
                
                //if (curPos.z - prevPos.z < 0)
                //{
                //  newPoses[i + 1] = lineRenderer.GetPosition(i);
                //}
                //else
                //{
                //    newPoses[i + 1] = prevPos;
                //}
            }
            else
            {
                //newPoses[i + 1] = lineRenderer.GetPosition(i);
            }
            //newPoses[i + 1] = lineRenderer.GetPosition(i);

            //newPoses[i + 1] = new Vector3(newPoses[i + 1].x, newPoses[i + 1].y, newPoses[0].z);
        }
        lineRenderer.SetPositions(newPoses);
    }

    //private void OnTriggerStay(Collider other)
    //{
    //    transform.GetChild(transform.childCount - 1).gameObject.SetActive(true);
    //    VibratorWrapper.Vibrate((long)Time.deltaTime);
    //}
    //private void OnTriggerExit(Collider other)
    //{
    //    transform.GetChild(transform.childCount - 1).gameObject.SetActive(false);
    //}

    private void OnCollisionStay(Collision collision)
    {
        /*if (collision.transform.GetComponent<SaberController>() == null) return;
        if (!collision.transform.GetComponent<SaberController>().enabled) return;

        transform.GetChild(transform.childCount - 1).gameObject.SetActive(true);
        transform.GetChild(transform.childCount - 1).GetComponent<ParticleSystem>().Play();

        Vector3 mid = Vector3.zero;
        for (int i = 0; i < collision.contactCount; i++)
        {
            mid += collision.contacts[i].point;
        }
        mid = new Vector3(mid.x / collision.contactCount, mid.y / collision.contactCount, mid.z / collision.contactCount);
        transform.GetChild(transform.childCount - 1).transform.position = mid;

        if(!Application.isEditor) VibratorWrapper.Vibrate((long)Time.deltaTime);*/
    }
    private void OnCollisionExit(Collision collision)
    {
        //transform.GetChild(transform.childCount - 1).gameObject.SetActive(false);
        //transform.GetChild(transform.childCount - 1).GetComponent<ParticleSystem>().Stop();
    }
}