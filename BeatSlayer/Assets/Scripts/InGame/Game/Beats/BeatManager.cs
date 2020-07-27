using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using InGame.Settings;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

namespace InGame.Game.Spawn
{

    public class BeatManager : MonoBehaviour
    {
        public GameManager gm;

        public List<BeatCubeClass> Beats { get; private set; }

        private float firstCubeTime;


        [Header("Prefabs")]
        [SerializeField] private GameObject cubePrefab;
        [SerializeField] private GameObject linePrefab, bombPrefab;
        
        public Camera cam;
        public AudioSource asource, spectrumAsource;

        public float secondHeight;


        public List<SpawnPointScript> spawnPoints;

        [HideInInspector] public Color32 leftSaberColor, rightSaberColor;
        public Color32 leftArrowColor, rightArrowColor;
        [HideInInspector] public int vibrationTime;

        // Beat field stuff
        public float fieldLength;
        public float fieldCrossTime;

        public RectTransform[] saberKeys;



        public UILineRenderer lineRenderer;
        public UILineRenderer algLineRenderer;
        private float maxDelay, maxAlgDelay;
        public Text mexDelayText, maxAlgDelayText;

        public float CubeSpeed
        {
            get
            {
                float dspeed = gm.difficulty.speed;

                float distance = fieldLength;
                float time = fieldCrossTime;

                float speed = distance / time;
                speed *= dspeed;

                return speed * Time.deltaTime * asource.pitch;
            }
        }
        public float maxDistance;



        private float playAreaZ;



        private bool noArrows, noLines;
        private float cubesSpeed;

        private Dictionary<int, Vector3> inputTouchesStartPoses = new Dictionary<int, Vector3>();
        private Dictionary<int, Vector3> inputTouchesPrevPoses = new Dictionary<int, Vector3>();

        private bool isEditor = Application.isEditor;

        private Vector3 prevMousePos;





        private void Start()
        {
            lineRenderer.Points = new Vector2[400];
            for (int i = 0; i < lineRenderer.Points.Length; i++)
            {
                lineRenderer.Points[i] = new Vector2(i * 2, 0);
            }

            algLineRenderer.Points = new Vector2[400];
            for (int i = 0; i < algLineRenderer.Points.Length; i++)
            {
                algLineRenderer.Points[i] = new Vector2(i * 2, 0);
            }

            for (int i = 0; i < spawnPoints.Count; i++)
            {
                spawnPoints[i].transform.position = new Vector3(GetPositionByRoad(i), spawnPoints[i].transform.position.y, spawnPoints[i].transform.position.z);
            }
        }

        public void Setup(GameManager gm, bool noArrows, bool noLines, float cubesSpeed)
        {
            this.gm = gm;
            this.noArrows = noArrows;
            this.noLines = noLines;

            this.cubesSpeed = cubesSpeed;

            leftSaberColor = SSytem.instance.leftColor * gm.prefsManager.prefs.colorPower * new Color(2f, 0.5f, 0.5f);
            rightSaberColor = SSytem.instance.rightColor * gm.prefsManager.prefs.colorPower * new Color(2f, 0.5f, 0.5f);
            leftArrowColor = SSytem.instance.leftDirColor * gm.prefsManager.prefs.colorPower;
            rightArrowColor = SSytem.instance.rightDirColor * gm.prefsManager.prefs.colorPower;

            vibrationTime = SSytem.instance.GetInt("Vibration") * 50;

            secondHeight = SettingsManager.Settings.Gameplay.SecondCubeHeight;

            if(Beats.Count > 0) firstCubeTime = Beats[0].time;
        }

        public void SetBeats(IEnumerable<BeatCubeClass> ls)
        {
            Beats = new List<BeatCubeClass>();
            Beats.AddRange(ls.OrderBy(c => c.time));
        }


        public IEnumerator IOnStart()
        {
            CalculateField();

            if (LoadingData.loadparams.Type == SceneloadParameters.LoadType.AudioFile) gm.audioManager.spectrumAsource.Play();
            
            yield return new WaitForSeconds(fieldCrossTime);

            asource.Play();
            
            gm.IsGameStartingMap = false;
            if (gm.paused)
            {
                gm.audioManager.asource.Pause();
                if (LoadingData.loadparams.Type == SceneloadParameters.LoadType.AudioFile) gm.audioManager.spectrumAsource.Pause();
            }
        }

        /// <summary>
        /// Spawning cubes based on asource.time. Execute every frame
        /// </summary>
        public void ProcessSpawnCycle()
        {
            Stopwatch w = new Stopwatch();
            Stopwatch algw = new Stopwatch();
            algw.Start();
            w.Start();

            // Beat spawning cycle
            List<BeatCubeClass> toRemove = new List<BeatCubeClass>();

            foreach (BeatCubeClass cls in Beats)
            {
                if(asource.time >= GetNormalizedTime(cls))
                {
                    algw.Stop();
                    SpawnBeatCube(cls);
                    algw.Start();

                    toRemove.Add(cls);
                }
            }

            Beats.RemoveAll(c => toRemove.Contains(c));

            w.Stop();
            algw.Stop();

            AddPointToChart(w.ElapsedMilliseconds, algw.ElapsedMilliseconds);
        }
        private float GetNormalizedTime(BeatCubeClass cls)
        {
            // Offset to sync cubes which faster than 1x
            // If faster, make a delay before spawning
            // Delay = timeOffset

            //float timeOffset = fieldCrossTime * (cls.speed - 1);
            //timeOffset /= cls.speed * gm.difficulty.speed;

            float dspeed = gm.difficulty.speed;

            float offset = fieldCrossTime / dspeed / cls.speed;

            return cls.time - offset;
            // Return absolute time
            //return cls.time + timeOffset;
        }
        private void AddPointToChart(float value, float algTime)
        {
            AddPointToArray(lineRenderer, value * 30);
            lineRenderer.SetAllDirty();

            AddPointToArray(algLineRenderer, algTime * 30);
            algLineRenderer.SetAllDirty();

            if(value > maxDelay && asource.time > 5)
            {
                maxDelay = value;
                mexDelayText.text = "Max: " + maxDelay + "ms";
            }
            if (value > maxAlgDelay && asource.time > 5)
            {
                maxAlgDelay = value;
                maxAlgDelayText.text = "Only alg time. Max: " + maxAlgDelay + "ms";
            }
        }
        private void AddPointToArray(UILineRenderer renderer, float value)
        {
            for (int i = 1; i < renderer.Points.Length; i++)
            {
                renderer.Points[i - 1] = new Vector2(renderer.Points[i - 1].x, renderer.Points[i].y); 
            }

            Vector2 lastPoint = renderer.Points[renderer.Points.Length - 1];

            renderer.Points[renderer.Points.Length - 1] = new Vector2(lastPoint.x, value);
        }



        public void SabersUpdate()
        {
            bool r = false, l = false; // Включение и выключение мечей

            if (isEditor)
            {
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
                        gm.rightSaber.SetSword(Input.mousePosition);
                    }
                    else
                    {
                        l = true;
                        gm.leftSaber.SetSword(Input.mousePosition);
                    }
                }

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

                List<RaycastHit> raycasted = new List<RaycastHit>();
                raycasted.AddRange(Physics.RaycastAll(ray, 100));

                if (i == samples - 1)
                {
                    raycasted.RemoveAll(c => GetComponent<IBeat>()?.GetClass().type == BeatCubeClass.Type.Bomb);
                }

                hits.AddRange(raycasted);

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
                    if(beat.GetClass().type == BeatCubeClass.Type.Bomb)
                    {
                        if(hit.point.z <= playAreaZ) cubesToPing.Add(beat);
                    }
                    else cubesToPing.Add(beat);
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
            //float mult = 1;
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

            GameObject prefab;
            switch (beat.type)
            {
                case BeatCubeClass.Type.Dir:
                case BeatCubeClass.Type.Point:
                    prefab = cubePrefab; break;

                case BeatCubeClass.Type.Line:
                    prefab = linePrefab; break;

                case BeatCubeClass.Type.Bomb:
                    prefab = bombPrefab; break;

                default:
                    prefab = null; break;
            }
            GameObject c = Instantiate(prefab);
            c.transform.name = prefab.name;

            IBeat cube = c.GetComponent<IBeat>();

            cube.Setup(gm, beat, cubesSpeed, this);


            int road = beat.road == -1 ? GetBestSpawnPoint(beat) : beat.road;
            spawnPoints[road].Spawn(beat.type);


            GetComponent<CheatEngine>().AddCube(cube);

            gm.activeCubes.Add(cube);
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

        public float GetPositionByRoad(int road)
        {
            float dist = SettingsManager.Settings.Gameplay.RoadsDistance;

            float leftPos = -dist * 2 + dist / 2f;
            float roadPos = leftPos + dist * road;

            return roadPos;
        }

        public bool CanSkip()
        {
            if (Beats.Count == 0) return false;
            return GetSkipTime() > asource.time;
        }

        private float GetSkipTime()
        {
            return firstCubeTime - 3;
        }

        public void Skip()
        {
            if (!CanSkip()) return;

            asource.time = GetSkipTime();
        }
    }

    public class SpawnPointClass
    {
        public int index;
        public float cooldown;
    }
}