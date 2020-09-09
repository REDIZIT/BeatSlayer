using BeatSlayerServer.Dtos.Mapping;
using BeatSlayerServer.Multiplayer.Accounts;
using GameNet;
using InGame.Game.Scoring.Mods;
using InGame.Menu.Mods;
using InGame.Models;
using InGame.ScriptableObjects;
using ProjectManagement;
using System.Collections.Generic;
using UnityEngine;

namespace InGame.Game
{
    public class ScoringManager : MonoBehaviour
    {
        public GameManager gm;
        public SODB sodb;

        /// <summary>
        /// ReplayData is used for sending to server
        /// </summary>
        public ReplayData Replay { get; set; }
        public SceneloadParameters.LoadType Loadtype { get; private set; }


        public float comboValue = 0, comboValueMax = 16;
        public float comboMultiplier = 1;
        public float maxCombo = 1;
        /// <summary>
        /// Value from mods
        /// </summary>
        public float scoreMultiplier = 1;

        /// <summary>
        /// Value for smooth earning score on line slices
        /// </summary>
        public float earnedScore;




        private void Update()
        {
            if (gm.paused) return;

            // == // == == Combo == == // == //

            if (comboValue >= comboValueMax && comboMultiplier < 16)
            {
                comboValue = 2;
                comboMultiplier *= 2;
                comboValueMax = 8 * comboMultiplier;
                StartCoroutine(gm.comboMultiplierAnim());
            }
            else if (comboValue <= 0)
            {
                if (comboMultiplier != 1)
                {
                    comboMultiplier /= 2;
                    comboValue = comboValueMax - 5;
                }
                else
                {
                    comboValue = 0;
                }
            }
            if (comboValue > 0)
            {
                //comboValue -= Time.deltaTime * comboMultiplier * 0.4f;
            }

            if (comboMultiplier > maxCombo) maxCombo = comboMultiplier;



            // == // == == Lines == == // == //

            if (earnedScore >= 1)
            {
                float rounded = Mathf.FloorToInt(earnedScore) * scoreMultiplier;
                earnedScore -= rounded;
                Replay.Score += rounded;
            }
        }


        // Ну вот блять, куча разных классов, которые делают одно и тоже, но ска разными буквами
        public void OnGameStart(
            BasicMapData map, DifficultyInfo difficulty, Difficulty projectDifficulty, 
            SceneloadParameters.LoadType loadtype, List<ModSO> mods)
        {
            Loadtype = loadtype;

            ModEnum modsEnum = ModEnum.None;
            //mods.ForEach(c => c.ApplyEnum(modsEnum));
            scoreMultiplier = 1;
            foreach (ModSO mod in mods)
            {
                scoreMultiplier *= mod.scoreMultiplier;
                modsEnum = mod.ApplyEnum(modsEnum);
            }
            Debug.Log($"Active mods ({mods.Count}) are " + modsEnum.ToString());

            Replay = new ReplayData()
            {
                Map = new MapData()
                {
                    Group = new GroupData()
                    {
                        Author = map.Author,
                        Name = map.Name
                    },
                    Nick = map.MapperNick
                },
                Difficulty = new DifficultyData()
                {
                    Id = difficulty.id,
                    Name = difficulty.name,
                    Stars = difficulty.stars,
                    CubesSpeed = projectDifficulty.speed
                },
                Nick = Payload.Account?.Nick,
                DifficultyName = difficulty.name,
                Score = 0,
                CubesSliced = 0,
                Missed = 0,
                RP = 0,
                Mods = modsEnum
            };
        }

        public void OnCubeHit(IBeat beat)
        {
            if(beat.GetClass().type == BeatCubeClass.Type.Bomb)
            {
                CubeMissRemoveScore();
                gm.tutorial.OnBombHit();
            }
            else
            {
                CubeHitAddScore();
                //gm.tutorial.OnCubeHit();
            }
        }
        private void CubeHitAddScore()
        {
            Replay.Score += comboMultiplier * scoreMultiplier;
            Replay.CubesSliced++;
            comboValue += 1;
        }
        public void OnCubeMiss(IBeat beat)
        {
            if (beat.GetClass().type == BeatCubeClass.Type.Bomb)
            {
                CubeHitAddScore();
                //gm.tutorial.OnBombMiss();
            }
            else
            {
                CubeMissRemoveScore();
                //gm.tutorial.OnCubeMiss();
            }
        }
        private void CubeMissRemoveScore()
        {
            comboValue -= 10;
            Replay.Score -= 5 * scoreMultiplier;
            if (Replay.Score < 0) Replay.Score = 0;
            Replay.Missed++;
        }

        public void OnLineHold()
        {
            Replay.Score += comboMultiplier * scoreMultiplier * 2.4f * Time.deltaTime;
        }
        public void OnLineHit()
        {
            Replay.CubesSliced++;
            comboValue += 1;
        }
    }
}