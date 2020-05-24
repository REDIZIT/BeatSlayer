using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Ranking;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace InGame.Game.Spawn
{

    public class BeatManager : MonoBehaviour
    {
        public GameManager gm;
        public GameObject cubePrefab, linePrefab;
        public Camera cam;
        public AudioSource asource, spectrumAsource;

        public List<SpawnPointScript> spawnPoints;

        [HideInInspector] public Color32 leftSaberColor, rightSaberColor;
        [HideInInspector] public Color32 leftArrowColor, rightArrowColor;
        [HideInInspector] public int vibrationTime;

        // Beat field stuff
        public float fieldLength;
        public float fieldCrossTime;
        
        /*
#if UNITY_ANDROID
        bool isEditor = false;
#else
        bool isEditor = true;
#endif*/
        bool isEditor = Application.isEditor;

        public float CubeSpeed
        {
            get
            {
                return (fieldLength * Time.deltaTime * (gm.paused ? 0 : 1) * gm.difficulty.speed) / fieldCrossTime;
            }
        }
        public float playAreaZ;

        bool noArrows, noLines, useSliceSound;
        Replay replay;

        Dictionary<int, Vector3> inputTouchesStartPoses = new Dictionary<int, Vector3>();
        Dictionary<int, Vector3> inputTouchesPrevPoses = new Dictionary<int, Vector3>();


        public RectTransform[] saberKeys;
        Vector3 prevMousePos;



        public void Setup(GameManager gm, bool noArrows, bool noLines, Replay replay)
        {
            this.gm = gm;
            this.noArrows = noArrows;
            this.noLines = noLines;
            useSliceSound = SSytem.instance.GetBool("SliceSound");
            this.replay = replay;

            leftSaberColor = SSytem.instance.leftColor * gm.prefsManager.prefs.colorPower * new Color(2f, 0.5f, 0.5f);
            rightSaberColor = SSytem.instance.rightColor * gm.prefsManager.prefs.colorPower * new Color(2f, 0.5f, 0.5f);
            leftArrowColor = SSytem.instance.leftDirColor * gm.prefsManager.prefs.colorPower;
            rightArrowColor = SSytem.instance.rightDirColor * gm.prefsManager.prefs.colorPower;

            vibrationTime = SSytem.instance.GetInt("Vibration") * 50;
            
        }


        public IEnumerator IOnStart()
        {
            CalculateField();

            if (LoadingData.loadparams.Type == SceneloadParameters.LoadType.AudioFile) gm.audioManager.spectrumAsource.Play();
            
            yield return new WaitForSeconds(fieldCrossTime);

            asource.Play();
            
            gm.gameStarting = false;
            if (gm.paused)
            {
                gm.audioManager.asource.Pause();
                if (LoadingData.loadparams.Type == SceneloadParameters.LoadType.AudioFile) gm.audioManager.spectrumAsource.Pause();
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S))
            {
                Time.timeScale = 0.2f;
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                Time.timeScale = 1f;
            }
        }


        public void SabersUpdate()
        {
            bool r = false, l = false; // Включение и выключение мечей

            if (isEditor)
            {
                /*if (Input.GetMouseButtonDown(0))
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
                        gm.rightSaber.SetSword(Input.mousePosition);
                    }
                    else
                    {
                        l = true;
                        gm.leftSaber.SetSword(Input.mousePosition);
                    }
                }*/

                RaycastSaber(Input.mousePosition, prevMousePos, r ? 1 : l ? -1 : 0);
                prevMousePos = Input.mousePosition;
            }
            else
            {
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
                        if (inputTouchesStartPoses[id].x >= Screen.width / 2f)
                        {
                            r = true;
                            gm.rightSaber.SetSword(Input.touches[i].position);
                            saberSide = 1;
                        }
                        else
                        {
                            l = true;
                            gm.leftSaber.SetSword(Input.touches[i].position);
                            saberSide = -1;
                        }
                    }


                    RaycastSaber(Input.touches[i].position, inputTouchesPrevPoses[id], saberSide);
                    inputTouchesPrevPoses[id] = Input.touches[i].position;
                }
            }
            
            gm.rightSaber.SetEnabled(r);
            gm.leftSaber.SetEnabled(l);
        }

        void RaycastSaber(Vector3 screenPos, Vector3 prevScreenPos, int saberSide)
        {
            Vector3 dir = screenPos - prevScreenPos;

            int samples = 4;
            int offset = 100;
            int xOffset = saberSide == -1 ? 14 : saberSide == 1 ? -14 : 0;

            List<RaycastHit> hits = new List<RaycastHit>();
            List<IBeat> cubesToPing = new List<IBeat>();
            for (int i = 0; i < samples; i++)
            {
                int sampleOffset = offset * i;
                int sampleXOffset = xOffset * i;

                Ray ray = cam.ScreenPointToRay(screenPos + new Vector3(sampleXOffset, sampleOffset));
                hits.AddRange(Physics.RaycastAll(ray, 100));

                saberKeys[i].position = screenPos + new Vector3(sampleXOffset, sampleOffset);
            }

            foreach (RaycastHit hit in hits.Distinct().OrderBy(c => c.point.z))
            {
                
                if (hit.point.z > playAreaZ + 10) continue;
                IBeat beat = hit.transform.GetComponent<IBeat>();
                if (beat == null)
                {
                    beat = hit.transform.parent.GetComponent<IBeat>();
                }
                

                if (beat != null)
                {
                    cubesToPing.Add(beat);
                }
            }

            foreach (IBeat beat in cubesToPing.Distinct())
            {
                int beatSaberType = beat.GetClass().saberType;
                if (beatSaberType == saberSide || beatSaberType == 0 || isEditor)
                {
                    beat.OnPoint(dir);
                    break;
                }
            }
        }

        void CalculateField()
        {
            float spawnZ = spawnPoints[0].transform.position.z;
            playAreaZ = cam.transform.position.z + 26; // 26 это расстояние от камеры до точки где удобно резать кубы
            float distanceSpawnAndPlayArea = spawnZ - playAreaZ;

            fieldLength = distanceSpawnAndPlayArea; // Длина поля в юнитах (где летят кубы)
            fieldCrossTime = 1.5f; // Время за которое куб должен преодолеть поле (в секундах)

            //float mult = replay.cubesSpeed / replay.musicSpeed; 
            float mult = 1;
            // scale для поля
            // Чем больше игрок поставил скорость кубам, тем .... в жопу, потом разберусь с модами

            //cubeSpeed = fieldLength * Time.deltaTime * mult; // Скорость куба (логично)
        }

        public void SpawnBeatCube(BeatCubeClass beat)
        {
            if (beat.type == BeatCubeClass.Type.Dir && noArrows)
            {
                beat.type = BeatCubeClass.Type.Point;
            }
            else if (beat.type == BeatCubeClass.Type.Line && noLines)
            {
                return;
            }

            GameObject c = Instantiate(beat.type == BeatCubeClass.Type.Line ? linePrefab : cubePrefab);
            c.transform.name = "BeatCube";

            IBeat cube = c.GetComponent<IBeat>();

            cube.Setup(gm, useSliceSound, beat, replay.cubesSpeed, this);


            int road = beat.road == -1 ? GetBestSpawnPoint(beat) : beat.road;
            spawnPoints[road].Spawn(beat.type);

            GetComponent<CheatEngine>().AddCube(c.GetComponent<Bit>());
        }

        public int GetBestSpawnPoint(BeatCubeClass beat)
        {
            List<SpawnPointScript> sorted = spawnPoints.OrderBy(c => c.cooldown).ToList();
            IEnumerable<SpawnPointScript> noCooldown = sorted.Where(c => c.cooldown <= 0);

            SpawnPointScript point;
            if (beat.type == BeatCubeClass.Type.Dir)
            {
                if (noCooldown.Count() > 1)
                {
                    point = noCooldown.ElementAt(Random.Range(0, noCooldown.Count() - 1));
                    
                }
                else
                {
                    point = sorted.First();
                }
            }
            else
            {
                point = noCooldown.ElementAt(Random.Range(0, sorted.Count() - 1));
            }
            return point.road;

            //// Список тех точек которые полностью свободены
            //List<int> freePoints = new List<int>();

            //// Поиск наилучшего варианта (Если freepoints пустой)
            //float min = 999;
            //int bestIndex = Random.Range(0,4);

            //for (int i = 0; i < spawnPoints.Length; i++)
            //{
            //    if(spawnPoints[i].cooldown < min)
            //    {
            //        min = spawnPoints[i].cooldown;
            //        bestIndex = i;
            //    }


            //    // Если точка полностью свободна, то добавляем её в список
            //    if (spawnPoints[i].cooldown <= 0)
            //    {
            //        freePoints.Add(i);
            //    }
            //}

            //// Если послностью свободных точек нет, то возвращаем лучший вариант 
            //if(freePoints.ToArray().Length == 0)
            //{
            //    return bestIndex;
            //}
            //else
            //{
            //    // Если есть полностью свободные точки, то вернуть одну из них
            //    return freePoints[Random.Range(0, freePoints.ToArray().Length)];
            //}
        }
    }

    public class SpawnPointClass
    {
        public int index;
        public float cooldown;
    }
}