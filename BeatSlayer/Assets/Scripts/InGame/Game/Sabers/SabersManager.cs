using InGame.Game.Spawn;
using InGame.Settings;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace InGame.Game.Sabers
{
    public class SabersManager : MonoBehaviour
    {
        public static SabersManager instance;


        [Header("Sabers")]
        public SaberController rightSaber;
        public SaberController leftSaber;





        private GameManager gm => GameManager.instance;
        private BeatManager bm => BeatManager.instance;


        private Vector3 prevMousePos;
        private Camera cam;

        private readonly Dictionary<int, Vector3> inputTouchesStartPoses = new Dictionary<int, Vector3>();
        private readonly Dictionary<int, Vector3> inputTouchesPrevPoses = new Dictionary<int, Vector3>();
        



        private void Awake()
        {
            instance = this;
            cam = Camera.main;
        }
        private void Start()
        {
            Color leftSaberColor = SSytem.instance.leftColor * (1 + SSytem.instance.GlowPowerSaberLeft / 25f);
            Color rightSaberColor = SSytem.instance.rightColor * (1 + SSytem.instance.GlowPowerSaberRight / 25f);

            float trailLifeTime = SSytem.instance.TrailLength / 100f * 0.4f;

            leftSaber.Init(leftSaberColor, gm.prefsManager.prefs.selectedLeftSaberId, gm.prefsManager.prefs.selectedSaberEffect, trailLifeTime);
            rightSaber.Init(rightSaberColor, gm.prefsManager.prefs.selectedRightSaberId, gm.prefsManager.prefs.selectedSaberEffect, trailLifeTime);
        }
        private void Update()
        {
            SabersUpdate();
        }


        public void SabersUpdate()
        {
            if (Application.isEditor)
            {
                SabersEditorUpdate();
            }
            else
            {
                SabersGameUpdate();
            }

            //r = hp.isAlive && r;
            //l = hp.isAlive && l;

            
        }

        private void SabersEditorUpdate()
        {
            bool r = false, l = false;

            if (Input.GetMouseButtonDown(0))
            {
                if (!inputTouchesStartPoses.ContainsKey(0))
                {
                    inputTouchesStartPoses.Add(0, Input.mousePosition);
                }
                else
                {
                    inputTouchesStartPoses[0] = Input.mousePosition;
                }

            }
            else if (Input.GetMouseButton(0))
            {
                if (inputTouchesStartPoses[0].x >= Screen.width / 2f)
                {
                    r = true;
                    rightSaber.SetSword(Input.mousePosition);
                }
                else
                {
                    l = true;
                    leftSaber.SetSword(Input.mousePosition);
                }
            }

            RaycastSaber(Input.mousePosition, prevMousePos, r ? 1 : l ? -1 : 0);
            prevMousePos = Input.mousePosition;

            rightSaber.SetEnabled(r);
            leftSaber.SetEnabled(l);
        }
        private void SabersGameUpdate()
        {
            bool r = false, l = false;

            for (int i = 0; i < Input.touches.Length; i++)
            {
                int saberSide = 0;
                int id = Input.touches[i].fingerId;

                if (Input.touches[i].phase == TouchPhase.Began)
                {
                    if (!inputTouchesStartPoses.ContainsKey(id))
                    {
                        inputTouchesStartPoses.Add(id, Input.touches[i].position);
                        inputTouchesPrevPoses.Add(id, Input.touches[i].position);
                    }
                    else
                    {
                        inputTouchesStartPoses[id] = Input.touches[i].position;
                    }

                }
                else if (Input.touches[i].phase != TouchPhase.Canceled && Input.touches[i].phase != TouchPhase.Ended)
                {
                    // If touch was started on right side
                    if (inputTouchesStartPoses[id].x >= Screen.width / 2f)
                    {
                        // If right saber already handled
                        if (r) continue;
                        // Set sword position
                        r = true;
                        rightSaber.SetSword(Input.touches[i].position);
                        saberSide = 1;
                    }
                    else
                    {
                        if (l) continue;

                        l = true;
                        leftSaber.SetSword(Input.touches[i].position);
                        saberSide = -1;
                    }
                }


                RaycastSaber(Input.touches[i].position, inputTouchesPrevPoses[id], saberSide);
                inputTouchesPrevPoses[id] = Input.touches[i].position;
            }

            rightSaber.SetEnabled(r);
            leftSaber.SetEnabled(l);
        }




        private void RaycastSaber(Vector3 screenPos, Vector3 prevScreenPos, int saberSide)
        {
            Vector3 dir = screenPos - prevScreenPos;

            int samples = SettingsManager.Settings.Gameplay.HideSabers ? 1 : 4;
            int offset = 100;
            int xOffset = saberSide == -1 ? 14 : saberSide == 1 ? -14 : 0;

            List<RaycastHit> hits = new List<RaycastHit>();
            List<IBeat> cubesToPing = new List<IBeat>();
            for (int i = 0; i < samples; i++)
            {
                int sampleOffset = offset * i;
                int sampleXOffset = xOffset * i;

                Ray ray = cam.ScreenPointToRay(screenPos + new Vector3(sampleXOffset, sampleOffset));

                List<RaycastHit> raycasted = new List<RaycastHit>();
                raycasted.AddRange(Physics.RaycastAll(ray, 100));

                if (i == samples - 1)
                {
                    raycasted.RemoveAll(c => GetComponent<IBeat>()?.GetClass().type == BeatCubeClass.Type.Bomb);
                }

                hits.AddRange(raycasted);
            }

            foreach (RaycastHit hit in hits.Distinct().OrderBy(c => c.point.z))
            {

                if (hit.point.z > bm.playAreaZ + 10) continue;
                IBeat beat = hit.transform.GetComponent<IBeat>();
                if (beat == null)
                {
                    beat = hit.transform.parent.GetComponent<IBeat>();
                }


                if (beat != null)
                {
                    if (beat.GetClass().type == BeatCubeClass.Type.Bomb)
                    {
                        if (hit.point.z <= bm.playAreaZ) cubesToPing.Add(beat);
                    }
                    else cubesToPing.Add(beat);
                }
            }


            foreach (IBeat beat in cubesToPing.Distinct())
            {
                int beatSaberType = beat.GetClass().saberType;
                if (beatSaberType == saberSide || beatSaberType == 0 || Application.isEditor)
                {
                    beat.OnPoint(dir);
                    break;
                }
            }
        }
    }
}
