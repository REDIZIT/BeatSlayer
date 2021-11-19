using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InGame.DI;
using InGame.Game.Beats.Blocks;
using InGame.Game.HP;
using InGame.Game.Scoring.Mods;
using InGame.Multiplayer.Lobby;
using InGame.Settings;
using UnityEngine;
using Zenject;

namespace InGame.Game.Spawn
{

    public class BeatManager : MonoBehaviour
    {
        public static BeatManager instance;

        [Header("Components")]
        [SerializeField] private GameManager gm;
        [SerializeField] private HPManager hp;
        [SerializeField] private AudioManager audioManager;
        [SerializeField] private AudioSource asource, spectrumAsource;
        [SerializeField] private CheatEngine cheatEngine;

        public List<BeatCubeClass> Beats { get; private set; }
        public List<Beat> ActiveBeats { get; private set; } = new List<Beat>();


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

        private Camera cam;


        public float LineLengthMultiplier
        {
            get
            {
                float dspeed = gm.difficulty.speed;

                float speed = fieldLength / fieldCrossTime;

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

        private float musicOffset;

        private BeatCube.Pool beatPool;
        private BeatLine.Pool linePool;
        private BeatBomb.Pool bombPool;

        [Inject]
        private void Construct(BeatCube.Pool beatPool, BeatLine.Pool linePool, BeatBomb.Pool bombPool)
        {
            this.beatPool = beatPool;
            this.linePool = linePool;
            this.bombPool = bombPool;
        }


        private void Awake()
        {
            instance = this;
            musicOffset = SettingsManager.Settings.Gameplay.MusicOffset;
            cam = Camera.main;
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

            if (Beats != null && Beats.Count > 0) firstCubeTime = Beats[0].time;
            if (Beats != null && Beats.Count > 0) lastCubeTime = Beats.Last().time;


            modsSpeedMultiplayer = ((gm.scoringManager.Replay.Mods & ModEnum.Easy) == ModEnum.Easy) ? 0.75f :
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
            List<Beat> toDissolve = new List<Beat>();
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
                if (virtualTime >= GetNormalizedTime(cls))
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

            float dspeed = gm.difficulty.speed;

            float offset = fieldCrossTime / dspeed / cls.speed;

            return cls.time - offset - musicOffset;
        }



        void CalculateField()
        {
            float spawnZ = spawnPoints[0].transform.position.z;
            playAreaZ = cam.transform.position.z + 26; // 26 это расстояние от камеры до точки где удобно резать кубы
            float distanceSpawnAndPlayArea = spawnZ - playAreaZ;

            fieldLength = distanceSpawnAndPlayArea + SettingsManager.Settings.Gameplay.CubesSuncOffset; // Длина поля в юнитах (где летят кубы)
            fieldCrossTime = 1.5f; // Время за которое куб должен преодолеть поле (в секундах)
        }




        public void SpawnBeatCube(BeatCubeClass beat)
        {
            if (beat.type == BeatCubeClass.Type.Bomb && gm.scoringManager.Replay.Mods.HasFlag(ModEnum.NoBombs))
                return;

            if (beat.type == BeatCubeClass.Type.Dir && gm.scoringManager.Replay.Mods.HasFlag(ModEnum.NoArrows))
                beat.type = BeatCubeClass.Type.Point;


            BeatPool pool;

            switch (beat.type)
            {
                case BeatCubeClass.Type.Dir:
                case BeatCubeClass.Type.Point:
                    pool = beatPool; break;

                case BeatCubeClass.Type.Line:
                    pool = linePool; break;

                case BeatCubeClass.Type.Bomb:
                    pool = bombPool; break;

                default: throw new System.Exception("Can't define pool for BeatCubeClass.Type " + beat.type);
            }

            Beat b = pool.Spawn(beat, cubesSpeed);

            int road = beat.road == -1 ? GetBestSpawnPoint(beat) : beat.road;
            spawnPoints[road].Spawn(beat.type);


            cheatEngine.AddCube(b);
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
}