using GameNet;
using InGame.Game.Scoring.Mods;
using InGame.Game.Spawn;
using UnityEngine;

namespace InGame.Game.HP
{
    public class HPManager : MonoBehaviour
    {
        public GameManager gm;
        public BeatManager bm;
        public ScoringManager sm;
        public HPBar bar;
        public HPLocker locker;

        public bool isAlive = true;
        public float HP
        {
            get { return hp; }
            set { hp = Mathf.Clamp(value, 0, 100); }
        }
        [SerializeField] private float hp;

        public string playerNick;



        [Header("HP balance values")]
        [Tooltip("Count of HP to give player when block sliced")]
        public float rewardHP;
        [Tooltip("Count of HP to reduce when block missed or bomb exploded")]
        public float punishHP;

        public bool IsNoFail { get; private set; }
        public bool IsEasy { get; private set; }
        public bool IsHard { get; private set; }
        public bool IsInstantDeath { get; private set; }



        private void Awake()
        {
            isAlive = true;
            HP = 100;
            playerNick = Payload.Account == null ? "Player" : Payload.Account.Nick;

            bar.manager = this;
            locker.manager = this;

            gm.OnCubeMiss += OnMiss;
            gm.OnCubeSlice += OnSlice;
            gm.OnLineSlice += OnSlice;
            gm.OnBombSlice += OnMiss;
            gm.OnBombMiss += OnSlice;
        }
        private void Start()
        {
            Debug.Log("Mods are " + sm.Replay.Mods.ToString());

            IsNoFail = (sm.Replay.Mods & ModEnum.NoFail) == ModEnum.NoFail;
            IsEasy = sm.Replay.Mods.HasFlag(ModEnum.Easy);
            IsHard = sm.Replay.Mods.HasFlag(ModEnum.Hard);
            IsInstantDeath = sm.Replay.Mods.HasFlag(ModEnum.OneTry);


            rewardHP *= IsEasy ? 1.25f : IsHard ? 0.75f : 1;
            punishHP *= IsEasy ? 0.75f : IsHard ? 1.25f : 1;
        }

        public void OnSlice()
        {
            if (!isAlive) return;

            HP += rewardHP;
        }
        public void OnMiss()
        {
            if (!isAlive) return;

            if (IsInstantDeath) HP = 0;
            else HP -= punishHP;

            if (HP <= 0)
            {
                KillPlayer();
            }
        }
        public void KillPlayer()
        {
            if (IsNoFail) return;

            if (!isAlive) return;
            isAlive = false;

            bm.OnGameOver();
        }
    }
}
