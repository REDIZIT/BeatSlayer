﻿using BeatSlayerServer.Dtos.Mapping;
using BeatSlayerServer.Multiplayer.Accounts;
using GameNet;
using ProjectManagement;
using UnityEngine;

namespace InGame.Game
{
    public class ScoringManager : MonoBehaviour
    {
        public GameManager gm;

        /// <summary>
        /// ReplayData is used for sending to server
        /// </summary>
        public ReplayData Replay { get; set; }


        public float comboValue = 0, comboValueMax = 16;
        public float comboMultiplier = 1;
        public float maxCombo = 1;
        /// <summary>
        /// Value from mods
        /// </summary>
        public float scoreMultiplier;

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
        public void OnGameStart(ProjectManagement.MapInfo map, DifficultyInfo difficulty, Difficulty projectDifficulty)
        {
            Replay = new ReplayData()
            {
                Map = new MapData()
                {
                    Group = new GroupData()
                    {
                        Author = map.author,
                        Name = map.name
                    },
                    Nick = map.nick
                },
                Difficulty = new DifficultyData()
                {
                    Id = difficulty.id,
                    Name = difficulty.name,
                    Stars = difficulty.stars,
                    CubesSpeed = projectDifficulty.speed
                },
                Nick = Payload.CurrentAccount?.Nick,
                DifficultyName = difficulty.name,
                Score = 0,
                CubesSliced = 0,
                Missed = 0,
                RP = 0
            };
        }

        public void OnCubeHit()
        {
            Replay.Score += comboMultiplier * scoreMultiplier;
            Replay.CubesSliced++;
            comboValue += 1;
        }
        public void OnCubeMiss()
        {
            comboValue -= 10;
            Replay.Score -= 5 * scoreMultiplier;
            if (Replay.Score < 0) Replay.Score = 0;
            Replay.Missed++;
        }

        public void OnLineHold()
        {
            //Replay.Score += comboMultiplier * scoreMultiplier * 0.1f;
            //Replay.Score += comboMultiplier * scoreMultiplier * 0.04f;
            Replay.Score += comboMultiplier * scoreMultiplier * 2.4f * Time.deltaTime;
        }
        public void OnLineHit()
        {
            Replay.CubesSliced++;
            comboValue += 1;
        }
    }
}