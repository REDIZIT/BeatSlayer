using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Bit : MonoBehaviour
{
    public Transform model;
    MeshRenderer renderer
    {
        get
        {
            return model.GetComponent<MeshRenderer>();
        }
    }
    MeshFilter filter
    {
        get
        {
            return model.GetComponent<MeshFilter>();
        }
    }

    public float speed, _speed;
    public float maxDistance;
    public bool useSoundEffect;

    [HideInInspector] public Color rightSaberColor, leftSaberColor;
    [HideInInspector] public Color rightArrowColor, leftArrowColor;

    //public GameScript gs;
    public GameManager gs;
    [HideInInspector] public int touch;
    [HideInInspector] public Vector3 startPos, endPos, dir;
    [HideInInspector] public float slicePow;
    [HideInInspector] public int saberType; // -1 Left; 0 Any; 1 Right
    [HideInInspector] public int vibrationTime;
    public Mesh dirMesh;
    public Mesh pointMesh;

    public BeatCubeClass.Type type;
    public BeatCubeClass.SubType subType;
    public float spawnEffectValue;

    public BeatCubeClass.SubType bitSubType;

    public GameObject cubeSlicePs;

    public ParticleSystem linePsLoop, linePsDestroy;

    [HideInInspector] public float prevDistPerFrame = 0;

    public void Start()
    {
        _speed = speed;
        if (type == BeatCubeClass.Type.Line)
        {
            StartForLine();
        }
        else
        {
            StartForCube();
        }
    }

    void StartForCube()
    {
        //if (cubeSlicePs == null)
        //{
        //    //cubeSlicePs = transform.GetChild(0).gameObject;
        //    cubeSlicePs = gs.lean.ShowParticle();
        //}


        cubeSlicePs.GetComponent<ParticleSystem>().Stop();
        //cubeSlicePs.transform.SetParent(transform);
        cubeSlicePs.transform.position = transform.position;

        spawnEffect = 0.5f;
        //cubeSlicePs.SetActive(true);


        //for (int i = 0; i < 3; i++)
        //{
        //    renderer.materials[i].SetColor("_Color", gs.lean.bitCubeColors[i]);
        //}

        GetComponent<BoxCollider>().enabled = true;

        //renderer.materials[0].SetFloat("_Threshold", -0.1f);
        //renderer.materials[1].SetFloat("_Threshold", -0.1f);
        //renderer.materials[2].SetFloat("_Threshold", -0.1f);

        if (saberType != 0)
        {
            if(saberType == 1)
            {
                renderer.materials[1].SetColor("_Color", rightSaberColor * 2);
                renderer.materials[1].SetColor("_EmissionColor", rightSaberColor * 2);

                renderer.materials[2].SetColor("_Color", rightArrowColor * 2);
                renderer.materials[2].SetColor("_EmissionColor", rightArrowColor * 2);
            }
            else
            {
                renderer.materials[1].SetColor("_Color", leftSaberColor * 2);
                renderer.materials[1].SetColor("_EmissionColor", leftSaberColor * 2);

                renderer.materials[2].SetColor("_Color", leftArrowColor * 2);
                renderer.materials[2].SetColor("_EmissionColor", leftArrowColor * 2);
            }
        }

        if (type == BeatCubeClass.Type.Point)
        {
            filter.mesh = pointMesh;
        }
        else
        {
            filter.mesh = dirMesh;
        }

        float r = Random.Range(-100f, 100f);
        renderer.materials[0].SetFloat("_Offset", r);
        renderer.materials[1].SetFloat("_Offset", r);
        renderer.materials[2].SetFloat("_Offset", r);



        if (subType == BeatCubeClass.SubType.Random)
        {
            int rnd = Random.Range(0, 4);
            if (rnd == 0) { bitSubType = BeatCubeClass.SubType.Down; }
            else if (rnd == 1) { bitSubType = BeatCubeClass.SubType.Up; }
            else if (rnd == 2) { bitSubType = BeatCubeClass.SubType.Left; }
            else { bitSubType = BeatCubeClass.SubType.Right; }
        }
        else
        {
            bitSubType = subType;
        }


        //int zRot = bitSubType == BeatCubeClass.SubType.Up ? 180 :
        //   bitSubType == BeatCubeClass.SubType.Down ? 0 :
        //   bitSubType == BeatCubeClass.SubType.Left ? 270 :
        //   90;
        int zRot = (int)bitSubType * 45;

        transform.eulerAngles = new Vector3(0, 0, zRot);
    }
    void StartForLine()
    {
        line = GetComponent<LineRenderer>();
        
        //

        //line.transform.GetChild(0).gameObject.SetActive(false);
        sphere.SetActive(false);
        sphereCap.SetActive(true);
        sphereEndCap.SetActive(false);

        ResetLine();
        //for (int i = 0; i < spawnLinePoints.Count - 1; i++)
        //{
        //    AddColliderToLine(line, spawnLinePoints[i], spawnLinePoints[i + 1]);
        //}
    }

    void ResetLine()
    {
        if (linePsLoop == null)
        {
            sphere = transform.GetChild(0).gameObject;
            sphereCap = transform.GetChild(1).gameObject;
            sphereEndCap = transform.GetChild(2).gameObject;
            linePsLoop = transform.GetChild(0).GetChild(1).GetComponent<ParticleSystem>();
            linePsDestroy = transform.GetChild(0).GetChild(2).GetComponent<ParticleSystem>();
        }
        linePsLoop.transform.parent = sphere.transform;
        linePsLoop.transform.localPosition = Vector3.zero;
        linePsDestroy.transform.parent = sphere.transform;
        linePsDestroy.transform.localPosition = Vector3.zero;

        //linePsLoop.Play();
        //Debug.Log("Reset");
        //linePsDestroy.Stop();
    }

    public GameObject sphere, sphereCap, sphereEndCap;
    public List<Vector3> linePoints = new List<Vector3>();
    public List<Vector3> spawnLinePoints = new List<Vector3>();

    public void SpecSpawn(Vector3[] points)
    {
        linePoints = new List<Vector3>();
        spawnLinePoints = new List<Vector3>();
        foreach (Vector3 point in points)
        {
            spawnLinePoints.Add(new Vector3(point.x, point.y, point.z * 100 * (1f / gs.pitch)));
        }
        //Debug.Log(points.Length);
        linePoints.Add(spawnLinePoints[0]);
        linePoints.Add(spawnLinePoints[0]);

        ResetLine();

        //linePsLoop.Play();
        //linePsDestroy.Stop();
    }
    public float spawnEffect = 0;
    public float distToNext;

    public int spawningId = 1;
    public void SpecSpawnUpdater()
    {
        if (spawningId == -1) return;

        // Получаем последний элемент в списке
        if (linePoints.Count < 2) return;
        int i = linePoints.ToArray().Length - 1;

        // Получаем нормаль движения
        Vector3 dir = (spawnLinePoints[spawningId] - spawnLinePoints[spawningId - 1]).normalized;
        // Рачтитываем точку соприкосновения LineRenderer и спавн поинта
        spawnEffect = 43 - transform.position.z + dir.sqrMagnitude;

        // Находим расстояние между запларинуемыми точками
        distToNext = spawnLinePoints[spawningId].z - spawnLinePoints[spawningId - 1].z;
        // Находим расстояние от отрисованной части до спавн поинта
        float dist = spawnEffect - linePoints[i - 1].z;
        // Увеличиваем отрисованую часть
        Vector3 prevKey = linePoints[i - 1];
        prevKey += new Vector3(dist * dir.x, dist * dir.y, dist * dir.z);
        linePoints[i] = prevKey;

        foreach(Transform child in transform)
        {
            if(child.name == "LineCollider")
            {
                Destroy(child.gameObject);
                //child.gameObject.SetActive(false);
            }
        }
        for (int s = 0; s < linePoints.Count - 1; s++)
        {
            AddColliderToLine(line, linePoints[s], linePoints[s + 1]);
        }

        // Если расстояние от отрисовнной части до запланированной < 0
        if (spawnLinePoints[spawningId].z - linePoints[i].z < 0)
        {
            // Выравниваем
            linePoints[i] = spawnLinePoints[spawningId];

            // Если конец
            if(spawningId >= spawnLinePoints.ToArray().Length - 1)
            {
                spawningId = -1;
                sphereEndCap.transform.localPosition = spawnLinePoints[spawnLinePoints.Count - 1];
                //Debug.Log("123");
                sphereEndCap.SetActive(true);
            }
            else
            {
                spawningId++;
                linePoints.Add(spawnLinePoints[spawningId - 1]);
            }
        }
    }
    public void Spec(Vector3 hit)
    {
        //Debug.LogError("Spec: " + isDead);

        if (isDead) return;

        float localHit = hit.z - transform.position.z;
        if (!sphere.activeSelf) { sphere.SetActive(true); linePsLoop.Play(); sphereCap.SetActive(false); }
        //sphere.transform.position = new Vector3(hit.x, transform.position.y, hit.z);
        float distToNext = linePoints[1].z - linePoints[0].z;
        float dist = localHit - linePoints[0].z;
        Vector3 prevKey = linePoints[0];
        Vector3 dir = (linePoints[1] - linePoints[0]).normalized;
        prevKey += new Vector3(dir.x * dist, dir.y * dist, dist);
        linePoints[0] = prevKey;
        sphere.transform.localPosition = linePoints[0];
        if(dist >= distToNext)
        {
            linePoints.RemoveAt(0);
        }

        if(spawningId == -1)
        {
            if (spawnLinePoints[spawnLinePoints.Count - 1].z - linePoints[0].z <= 1.25f)
            {
                linePsLoop.transform.parent = null;
                linePsLoop.Stop();
                linePsDestroy.Play();
                linePsDestroy.transform.parent = null;

                gs.BeatLineSliced();
                gs.activeCubes.Remove(gameObject);

                //isDead = true;
                Destroy(gameObject);
                //gs.lean.Hide(gameObject);
            }
        }

        // Начисление очков 
        float distPerFrame = linePoints[0].z - prevDistPerFrame;
        prevDistPerFrame = distPerFrame;
        gs.earnedScore += prevDistPerFrame * 2 * Time.deltaTime * gs.pitch; // 2 - множитель очков за кадр
    }
    public void SpecDieAnim()
    {
        Color emission = line.material.GetColor("_EmissionColor");
        emission = new Color(emission.r / 1.1f, emission.g / 1.2f, emission.b / 1.2f, emission.a / 1.2f);
        line.material.SetColor("_EmissionColor", emission);
        line.material.color = new Color(emission.a, emission.a, emission.a, emission.a);

        if(emission.a <= 0.1f)
        {


            gs.activeCubes.Remove(gameObject);
            gs.MissedBeatCube();

            linePsLoop.transform.parent = null;
            linePsLoop.Stop();

            linePsDestroy.transform.parent = null;
            //linePsDestroy.Play();

            foreach (Transform child in transform)
            {
                if (child.name == "LineCollider")
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    child.gameObject.SetActive(false);
                }
            }

            //gs.lean.Hide(gameObject);
            Destroy(gameObject);
        }
    }

    LineRenderer line;
    private void AddColliderToLine(LineRenderer line, Vector3 startPoint, Vector3 endPoint)
    {
        //create the collider for the line
        BoxCollider lineCollider = new GameObject("LineCollider").AddComponent<BoxCollider>();
        //set the collider as a child of your line
        lineCollider.transform.parent = line.transform;
        // get width of collider from line 
        float lineWidth = line.endWidth * 2.5f;
        // get the length of the line using the Distance method
        float lineLength = Vector3.Distance(startPoint, endPoint);
        // size of collider is set where X is length of line, Y is width of line
        //z will be how far the collider reaches to the sky
        lineCollider.size = new Vector3(lineLength, lineWidth, lineWidth);
        // get the midPoint
        Vector3 midPoint = (startPoint + endPoint) / 2;
        // move the created collider to the midPoint
        lineCollider.transform.localPosition = midPoint;


        //heres the beef of the function, Mathf.Atan2 wants the slope, be careful however because it wants it in a weird form
        //it will divide for you so just plug in your (y2-y1),(x2,x1)
        float angle = Mathf.Atan2((endPoint.z - startPoint.z), (endPoint.x - startPoint.x));

        // angle now holds our answer but it's in radians, we want degrees
        // Mathf.Rad2Deg is just a constant equal to 57.2958 that we multiply by to change radians to degrees
        angle *= Mathf.Rad2Deg;

        //were interested in the inverse so multiply by -1
        angle *= -1;
        // now apply the rotation to the collider's transform, carful where you put the angle variable
        // in 3d space you don't wan't to rotate on your y axis
        lineCollider.transform.Rotate(0, angle, 0);
    }



    public float deadThresgold = 0;
    bool vibrated = false;
    public bool Update_IsDead()
    {
        if (isDead)
        {
            try
            {
                if (!vibrated && vibrationTime != 0 && !Application.isEditor)
                {
                    vibrated = true;
                    VibratorWrapper.Vibrate(vibrationTime);
                }

                renderer.materials[0].SetFloat("_Threshold", deadThresgold);
                renderer.materials[1].SetFloat("_Threshold", deadThresgold);
                renderer.materials[2].SetFloat("_Threshold", deadThresgold);
                deadThresgold += Time.deltaTime * 4f;
                speed -= (speed - Time.deltaTime) / 8f;
                if (deadThresgold >= 1 /*&& !cubeSlicePs.GetComponent<ParticleSystem>().isPlaying && gameObject.activeSelf*/)
                {
                    //Destroy(gameObject);
                    //cubeSlicePs.transform.parent = transform; $$$
                    //cubeSlicePs.gameObject.SetActive(true); // $$$
                    //System.TimeSpan t = System.DateTime.Now.TimeOfDay;
                    //gs.lean.Hide(gameObject);
                    gs.activeCubes.Remove(gameObject);
                    Destroy(gameObject);
                    //Debug.Log("Bit.Hide: " + (System.DateTime.Now.TimeOfDay - t).TotalMilliseconds + "ms");
                }

                transform.position += new Vector3(0, 0, 1) * -speed * Time.deltaTime;
            }
            catch (System.Exception err)
            {
                Debug.LogError("IsDead error: Materials count: " + renderer.materials.Length + " Name: " + transform.name + " msg:" + err.Message);
            }

            return true;
        }
        return false;
    }

    private void Update()
    {
        System.TimeSpan t = System.DateTime.Now.TimeOfDay;
        if (gs.paused) return;

        if (type == BeatCubeClass.Type.Line)
        {
            UpdateForLine();
        }
        else
        {
            if (Update_IsDead()) { return; }

            UpdateForCube();
        }

        //Debug.Log("Bit.Update: " + (System.DateTime.Now.TimeOfDay - t).TotalMilliseconds + "ms");
    }
    void UpdateForCube()
    {
        if(spawnEffect > 0)
        {
            renderer.materials[0].SetFloat("_Threshold", spawnEffect);
            renderer.materials[1].SetFloat("_Threshold", spawnEffect);
            renderer.materials[2].SetFloat("_Threshold", spawnEffect);
            spawnEffect -= Time.deltaTime * 1f;
        }
        else
        {
            renderer.materials[0].SetFloat("_Threshold", -0.1f);
            renderer.materials[1].SetFloat("_Threshold", -0.1f);
            renderer.materials[2].SetFloat("_Threshold", -0.1f);
        }
        

        transform.position += new Vector3(0, 0, 1) * -speed * Time.deltaTime;
        if (transform.position.z <= maxDistance)
        {
            System.TimeSpan t = System.DateTime.Now.TimeOfDay;
            gs.activeCubes.Remove(gameObject);
            gs.MissedBeatCube();

            gs.activeCubes.Remove(gameObject);
            Destroy(gameObject);
            //gs.lean.Hide(gameObject);

        }

        if (sliced)
        {
            if (Application.isEditor)
            {
                endPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
                slicePow = (endPos.x - startPos.x) + (endPos.y - startPos.y);
                slicePow *= 5f;
            }
            else
            {
                try
                {
                    if (Input.GetTouch(touch).fingerId != currentTouchId)
                    {
                        endPos = startPos;
                        return;
                    }
                }
                catch
                {
                    return;
                }
                endPos = new Vector3(Input.GetTouch(touch).position.x, Input.GetTouch(touch).position.y, 0);
                slicePow = (endPos.x - startPos.x) + (endPos.y - startPos.y);
            }
            dir = (endPos - startPos);

            if (type == BeatCubeClass.Type.Point && (dir.normalized != Vector3.zero))
            {
                SendBitSliced(dir);
            }
            //else if (bitSubType == BeatCubeClass.SubType.Down) // DOWN
            //{
            //    if (dir.normalized.y <= -0.6f) SendBitSliced(dir);
            //}
            //else if (bitSubType == BeatCubeClass.SubType.Up) // UP
            //{
            //    if (dir.normalized.y >= 0.6f) SendBitSliced(dir);
            //}
            //else if (bitSubType == BeatCubeClass.SubType.Left) // LEFT
            //{
            //    if (dir.normalized.x <= -0.6f) SendBitSliced(dir);
            //}
            //else if (bitSubType == BeatCubeClass.SubType.Right) // RIGHT
            //{
            //    if (dir.normalized.x >= 0.6f) SendBitSliced(dir);
            //}
            //else
            //{
            //    sliced = false;
            //}

            float targetDeg = (int)bitSubType * 45;


            Vector2 vec = endPos - startPos;
            float angle = Mathf.Atan2(vec.y, vec.x);
            float degrees = angle * 180f / Mathf.PI + 90;
            if (targetDeg > 180) targetDeg = 360 - targetDeg;

            if(Mathf.Abs(targetDeg - degrees) <= 45)
            {
                SendBitSliced(dir);
            }
        }
    }
    void UpdateForLine()
    {
        if (!isDead)
        {
            //line.materials[0].SetColor("_Color", new Color32(0, 145, 255, 255));
            //line.materials[0].SetColor("_EmissionColor", new Color32(0, 145, 255, 255));

            //line.materials[0].SetColor("_Color", gs.lean.beatLineColor);
            //line.materials[0].SetColor("_EmissionColor", gs.lean.beatLineColor);
        }
        

        transform.position += new Vector3(0, 0, 1) * -speed * Time.deltaTime;

        if (linePoints[0].z + transform.position.z <= maxDistance) isDead = true;
        if (isDead) SpecDieAnim();

        if (isDead) return;

        SpecSpawnUpdater();

        line.positionCount = linePoints.ToArray().Length;
        for (int i = 0; i < linePoints.ToArray().Length; i++)
        {
            line.SetPosition(i, linePoints[i]);
        }
    }


    public bool isDead;
    [HideInInspector] public int currentTouchId;
    public void SendBitSliced(Vector2 direction)
    {
        if (useSoundEffect)
        {
            cubeSlicePs.SetActive(true);
            cubeSlicePs.transform.parent = null;
        }

        gs.BeatCubeSliced();

        isDead = true;
        GetComponent<BoxCollider>().enabled = false;


        foreach (Material mat in renderer.materials)
        {
            mat.SetColor("_Color", new Color(
                mat.color.r / 10f,
                mat.color.g / 10f,
                mat.color.b / 10f));
        }
    }
    public void Sliced(int t)
    {
        if (sliced) return;
        if (transform.position.z <= 14)
        {
            touch = t;
            currentTouchId = Input.touches[t].fingerId;


            //if (Input.GetTouch(t).phase != TouchPhase.Began && type == BeatCubeClass.Type.Point)
            //{
            //    return;
            //}
            startPos = new Vector3(Input.GetTouch(t).position.x, Input.GetTouch(t).position.y, 0);
            sliced = true;

        }
    }

    public bool sliced = false;
    public void Sliced()
    {
        if (sliced)
        {
            return;
        }
        if (transform.position.z <= 20)
        {
            startPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            sliced = true;
        }
    }
}



public static class VibratorWrapper
{

    static AndroidJavaObject vibrator = null;

    static VibratorWrapper()
    {

        var unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        var unityPlayerActivity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        vibrator = unityPlayerActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");

    }
    public static void Vibrate(long time)
    {
        if (!Application.isEditor)
        {
            vibrator.Call("vibrate", time);
        }
        else
        {
            Handheld.Vibrate();
        }
    }
}