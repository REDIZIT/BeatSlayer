using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InGame.Game.HP;
using InGame.Game.Scoring.Mods;
using InGame.Multiplayer.Lobby;
using InGame.Settings;
using UnityEngine;

namespace InGame.Game.Spawn
{

    public class BeatManager : MonoBehaviour
    {
        public static BeatManager instance;

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

        


        public float LineLengthMultiplier
        {
            get
            {
                float dspeed = gm.difficulty.speed;

                float speed = fieldLength / fieldCrossTime;
                //speed /= dspeed;

                return speed * asource.pitch * modsSpeedMultiplayer * dspeed;
            }
        }
        /// <summary>Cube speed per frame</summary>
        public float CubeSpeedPerFrame
        {
            get
            {
                return CubeSpeedPerSecond * Time.deltaTime;
            }
        }
        public float CubeSpeedPerSecond
        {
            get
            {
                float dspeed = gm.difficulty.speed;

                float distance = fieldLength;
                float time = fieldCrossTime;

                float speed = distance / time;
                speed *= dspeed;

                return speed * asource.pitch * modsSpeedMultiplayer;
            }
        }
        

        /// <summary>
        /// Modifier of <see cref="CubeSpeedPerFrame"/> based on selected Mods
        /// </summary>
        private float modsSpeedMultiplayer = 1;

        /// <summary>
        /// Audio time - offset
        /// </summary>
        private float virtualTime;


       
        private float firstCubeTime, lastCubeTime;

        public float playAreaZ;
        private float cubesSpeed;





        private void Awake()
        {
            instance = this;
        }
        private void Update()
        {
            if (gm.IsGameStartingMap)
            {
                virtualTime += asource.pitch * Time.deltaTime;
            }
            else
            {
                virtualTime = asource.time;
            }
        }



        public void Setup(GameManager gm, float cubesSpeed)
        {
            this.gm = gm;

            this.cubesSpeed = cubesSpeed;

            leftSaberColor = SSytem.leftColor * AdvancedSaveManager.prefs.colorPower * new Color(2f, 0.5f, 0.5f);
            rightSaberColor = SSytem.rightColor * AdvancedSaveManager.prefs.colorPower * new Color(2f, 0.5f, 0.5f);
            leftArrowColor = SSytem.leftDirColor * AdvancedSaveManager.prefs.colorPower;
            rightArrowColor = SSytem.rightDirColor * AdvancedSaveManager.prefs.colorPower;

            secondHeight = SettingsManager.Settings.Gameplay.SecondCubeHeight;

            if(Beats != null && Beats.Count > 0) firstCubeTime = Beats[0].time;
            if(Beats != null && Beats.Count > 0) lastCubeTime = Beats.Last().time;


            modsSpeedMultiplayer = ((gm.scoringManager.Replay.Mods & ModEnum.Easy) == ModEnum.Easy) ? 0.75f :
                            (gm.scoringManager.Replay.Mods.HasFlag(ModEnum.Hard)) ? 1.25f :
                            1;
        }

        public void SetBeats(IEnumerable<BeatCubeClass> ls)
        {
            Debug.Log("Set beats");
            Beats = new List<BeatCubeClass>();
            Beats.AddRange(ls.OrderBy(c => c.time));
        }


        public IEnumerator IStartGame(bool isTutorial)
        {
            CalculateField();

            if (LoadingData.loadparams.Type == SceneloadParameters.LoadType.AudioFile) gm.audioManager.spectrumAsource.Play();

            virtualTime = -fieldCrossTime;
            gm.IsGameStartingMap = true;

            yield return new WaitForSeconds(fieldCrossTime);

            if (!isTutorial)
                asource.Play();
            
            gm.IsGameStartingMap = false;
            gm.IsGameStarted = true;

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
                if(virtualTime >= GetNormalizedTime(cls))
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

        

        void CalculateField()
        {
            float spawnZ = spawnPoints[0].transform.position.z;
            playAreaZ = cam.transform.position.z + 26; // 26 это расстояние от камеры до точки где удобно резать кубы
            float distanceSpawnAndPlayArea = spawnZ - playAreaZ;

            fieldLength = distanceSpawnAndPlayArea + SettingsManager.Settings.Gameplay.CubesSuncOffset; // Длина поля в юнитах (где летят кубы)
            fieldCrossTime = 1.5f; // Время за которое куб должен преодолеть поле (в секундах)
            //fieldCrossTime = .5f; // Время за которое куб должен преодолеть поле (в секундах)
            //fieldCrossTime = 1f; // Время за которое куб должен преодолеть поле (в секундах)
            //fieldCrossTime = 2f; // Время за которое куб должен преодолеть поле (в секундах)
            //fieldCrossTime = 5f; // Время за которое куб должен преодолеть поле (в секундах)
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
            // Copensate error appeared due to limited fps
            // Make offset to proper time
            c.transform.position += new Vector3(0, 0, (asource.time - beat.time) * CubeSpeedPerSecond);

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

            if (LoadingData.loadparams.Type == SceneloadParameters.LoadType.Tutorial) return false;
            if (LoadingData.loadparams.Type == SceneloadParameters.LoadType.AudioFile) return false;

            if (asource.time == 0) return false;


            if (Beats == null) return true;
            



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