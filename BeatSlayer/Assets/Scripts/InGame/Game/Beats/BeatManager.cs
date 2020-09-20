﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InGame.Game.HP;
using InGame.Game.Scoring.Mods;
using InGame.Multiplayer.Lobby;
using InGame.Settings;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace InGame.Game.Spawn
{

    public class BeatManager : MonoBehaviour
    {
        [Header("Components")]
        public GameManager gm;
        public HPManager hp;
        public AudioManager audioManager;
        public Camera cam;
        public AudioSource asource, spectrumAsource;

        public List<BeatCubeClass> Beats { get; private set; }
        public List<IBeat> ActiveBeats { get; private set; } = new List<IBeat>();


        [Header("Valuables")]
        public float secondHeight;
        public float fieldLength;
        public float fieldCrossTime;
        public float maxDistance;
        public List<SpawnPointScript> spawnPoints;


        [Header("Prefabs")]
        [SerializeField] private GameObject cubePrefab;
        [SerializeField] private GameObject linePrefab, bombPrefab;


        // Colors
        [HideInInspector] public Color32 leftSaberColor, rightSaberColor;
        [HideInInspector] public Color32 leftArrowColor, rightArrowColor;

        


        public float CubeSpeed
        {
            get
            {
                float dspeed = gm.difficulty.speed;

                float distance = fieldLength;
                float time = fieldCrossTime;

                float speed = distance / time;
                speed *= dspeed;

                return speed * Time.deltaTime * asource.pitch * speedModifier;
            }
        }
        

        /// <summary>
        /// Modifier of <see cref="CubeSpeed"/> based on selected Mods
        /// </summary>
        private float speedModifier = 1;



        private float playAreaZ;
        private float firstCubeTime, lastCubeTime;

        private float cubesSpeed;

        private readonly Dictionary<int, Vector3> inputTouchesStartPoses = new Dictionary<int, Vector3>();
        private readonly Dictionary<int, Vector3> inputTouchesPrevPoses = new Dictionary<int, Vector3>();

        private readonly bool isEditor = Application.isEditor;

        private Vector3 prevMousePos;








        public void Setup(GameManager gm, float cubesSpeed)
        {
            this.gm = gm;

            this.cubesSpeed = cubesSpeed;

            leftSaberColor = SSytem.instance.leftColor * gm.prefsManager.prefs.colorPower * new Color(2f, 0.5f, 0.5f);
            rightSaberColor = SSytem.instance.rightColor * gm.prefsManager.prefs.colorPower * new Color(2f, 0.5f, 0.5f);
            leftArrowColor = SSytem.instance.leftDirColor * gm.prefsManager.prefs.colorPower;
            rightArrowColor = SSytem.instance.rightDirColor * gm.prefsManager.prefs.colorPower;

            secondHeight = SettingsManager.Settings.Gameplay.SecondCubeHeight;

            if(Beats != null && Beats.Count > 0) firstCubeTime = Beats[0].time;
            if(Beats != null && Beats.Count > 0) lastCubeTime = Beats.Last().time;


            speedModifier = ((gm.scoringManager.Replay.Mods & ModEnum.Easy) == ModEnum.Easy) ? 0.75f :
                            (gm.scoringManager.Replay.Mods.HasFlag(ModEnum.Hard)) ? 1.25f :
                            1;
        }

        public void SetBeats(IEnumerable<BeatCubeClass> ls)
        {
            Beats = new List<BeatCubeClass>();
            Beats.AddRange(ls.OrderBy(c => c.time));
        }


        public IEnumerator IStartGame(bool isTutorial)
        {
            CalculateField();

            if (LoadingData.loadparams.Type == SceneloadParameters.LoadType.AudioFile) gm.audioManager.spectrumAsource.Play();
            
            yield return new WaitForSeconds(fieldCrossTime);

            if (!isTutorial)
                asource.Play();
            
            gm.IsGameStartingMap = false;
            if (gm.paused)
            {
                gm.audioManager.asource.Pause();
                if (LoadingData.loadparams.Type == SceneloadParameters.LoadType.AudioFile) gm.audioManager.spectrumAsource.Pause();
            }
        }


        #region Dead and GameOver

        public void OnGameOver()
        {
            List<IBeat> toDissolve = new List<IBeat>();
            toDissolve.AddRange(ActiveBeats);
            toDissolve.ForEach(c => c.OnPoint(Vector2.zero, true));

            StartCoroutine(IEOnGameOver());
        }
        private IEnumerator IEOnGameOver()
        {
            float timespeed = 1;
            float decrease = 0.02f;

            while (timespeed > 0)
            {
                decrease /= 1.005f;
                timespeed -= decrease;

                audioManager.asource.pitch = timespeed * gm.Pitch;

                if (LoadingData.loadparams.Type == SceneloadParameters.LoadType.AudioFile) audioManager.spectrumAsource.pitch = timespeed * gm.Pitch;

                //Time.timeScale = timespeed;
                yield return new WaitForEndOfFrame();
            }

            audioManager.asource.Stop();
            audioManager.spectrumAsource.Stop();
        }

        #endregion


        /// <summary>
        /// Spawning cubes based on asource.time. Execute every frame
        /// </summary>
        public void ProcessSpawnCycle()
        {
            if (LobbyManager.lobby == null && !hp.isAlive)
                return;

            // Beat spawning cycle
            List<BeatCubeClass> toRemove = new List<BeatCubeClass>();

            foreach (BeatCubeClass cls in Beats)
            {
                if(asource.time >= GetNormalizedTime(cls))
                {
                    SpawnBeatCube(cls);

                    toRemove.Add(cls);
                }
            }

            Beats.RemoveAll(c => toRemove.Contains(c));
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

            r = hp.isAlive && r;
            l = hp.isAlive && l;

            gm.rightSaber.SetEnabled(r);
            gm.leftSaber.SetEnabled(l);
        }

        void RaycastSaber(Vector3 screenPos, Vector3 prevScreenPos, int saberSide)
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

            fieldLength = distanceSpawnAndPlayArea + SettingsManager.Settings.Gameplay.CubesSuncOffset; // Длина поля в юнитах (где летят кубы)
            fieldCrossTime = 1.5f; // Время за которое куб должен преодолеть поле (в секундах)

            //float mult = replay.cubesSpeed / replay.musicSpeed; 
            //float mult = 1;
            // scale для поля
            // Чем больше игрок поставил скорость кубам, тем .... в жопу, потом разберусь с модами

            //cubeSpeed = fieldLength * Time.deltaTime * mult; // Скорость куба (логично)
        }




        public void SpawnBeatCube(BeatCubeClass beat)
        {
            if (beat.type == BeatCubeClass.Type.Bomb && gm.scoringManager.Replay.Mods.HasFlag(ModEnum.NoBombs))
                return;

            if (beat.type == BeatCubeClass.Type.Dir && gm.scoringManager.Replay.Mods.HasFlag(ModEnum.NoArrows))
                beat.type = BeatCubeClass.Type.Point;

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

            ActiveBeats.Add(cube);
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
            if (!asource.isPlaying) return false;

            //UnityEngine.Debug.Log(ActiveBeats.Count);

            if (Beats == null) return true;
            //if (Beats.Count == 0) return true;

            if (asource.time == 0) return false;

            if (Beats.Count == 0) return asource.time >= lastCubeTime + 2;
            return GetSkipTime() > asource.time;
        }

        private float GetSkipTime()
        {
            if (Beats == null || Beats.Count == 0) return asource.clip.length;

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