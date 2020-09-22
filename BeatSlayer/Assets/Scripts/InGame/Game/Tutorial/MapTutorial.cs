using Assets.SimpleLocalization;
using BeatSlayerServer.Dtos.Mapping;
using GameNet;
using InGame.Animations;
using InGame.Game.Spawn;
using InGame.SceneManagement;
using Newtonsoft.Json;
using Ranking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace InGame.Game.Tutorial
{
    public class MapTutorial : MonoBehaviour
    {
        public AudioSource asource;
        public VideoPlayer videoPlayer;
        public Animator videoAnimator;
        public CanvasGroup videoCanvas;
        public BeatManager bm;
        public GameManager gm;
        public ScoringManager sm;


        public List<AudioTimeCode> timecodes;

        public AudioClip[] cubeHitClips, bombHitClips;

        public Text hintText;

        [Header("Lockers")]
        public GameObject videoLocker;
        public GameObject meshUI;

        [Header("Finish")]
        public GameObject finishLocker;
        public Animator finishAnimator;
        public FireworkSSytem fireworks;

        [Header("Buttons")]
        public GameObject skipBtnOverlay;
        public GameObject continueOverlay;
        public GameObject coinsText, coinsLocker;

        [Header("Videos")]
        public VideoClip arrowVideo;
        public VideoClip pointsVideo, colorVideo, linesVideo, bombsVideo;
        private bool isVideoSkipped, isVideoContinued, isVideoRepeated;

        private bool isEnabled;


        public void StartTutorial()
        {
            isEnabled = true;
            StartCoroutine(ITutorialSteps());
        }

        public void OnBombHit()
        {
            if (!isEnabled) return;

            //asource.PlayOneShot(bombHitClips[Random.Range(0, bombHitClips.Length)]);
        }

        public void OnSkipVideoBtnClick()
        {
            isVideoContinued = true;
            isVideoSkipped = true;
        }
        public void OnContinueBtnClick()
        {
            isVideoContinued = true;
        }
        public void OnAgainBtnClick()
        {
            isVideoContinued = true;
            isVideoRepeated = true;
        }
        public void OnFinishBtnClick()
        {
            gm.Exit();
        }
        public void OnReplayBtnClick()
        {
            SceneController.instance.LoadScene(LoadingData.loadparams);
        }



        private IEnumerator ITutorialSteps()
        {
            TutorialResult result = new TutorialResult();

            yield return Step1();
            yield return new WaitForSeconds(5);
            result.AddStep("Dir", sm.Replay);

            yield return Step2();
            yield return new WaitForSeconds(5);
            result.AddStep("Points", sm.Replay);

            yield return Step3();
            yield return new WaitForSeconds(5);
            result.AddStep("Colors", sm.Replay);

            yield return Step4();
            yield return new WaitForSeconds(5);
            result.AddStep("Lines", sm.Replay);

            yield return Step5();

            // Finish
            yield return new WaitForSeconds(5);
            result.AddStep("Bombs", sm.Replay);

            finishLocker.SetActive(true);
            finishAnimator.Play("Show-finish");
            fireworks.StartEmitting();

            Debug.Log(JsonConvert.SerializeObject(result));
            NetCore.ServerActions.Tutorial.TutorialPlayed(result);

            if (Payload.Account == null)
            {
                coinsText.SetActive(true);
                coinsLocker.SetActive(true);
            }
        }
        private IEnumerator Step1()
        {
            // Directions
            yield return ShowVideo(arrowVideo, "Tutorial-1");

            yield return new WaitForSeconds(4);
            SpawnArrow(0, 0, 0, BeatCubeClass.SubType.Down);

            yield return new WaitForSeconds(4);
            SpawnArrow(3, 0, 0, BeatCubeClass.SubType.Left);
        }
        private IEnumerator Step2()
        {
            // Points
            yield return ShowVideo(pointsVideo, "Tutorial-2");
            yield return new WaitForSeconds(3);
            SpawnPoint(1, 0, 0);
            yield return new WaitForSeconds(0.5f);
            SpawnPoint(3, 0, 0);
            yield return new WaitForSeconds(0.5f);
            SpawnPoint(1, 0, 0);
            yield return new WaitForSeconds(0.3f);
            SpawnPoint(0, 0, 0);
            yield return new WaitForSeconds(0.3f);
            SpawnPoint(3, 1, 0);
            yield return new WaitForSeconds(0.3f);
            SpawnPoint(2, 1, 0);
            SpawnPoint(0, 0, 0);
        }
        private IEnumerator Step3()
        {
            // Colors
            yield return ShowVideo(colorVideo, "Tutorial-3");
            yield return new WaitForSeconds(3);
            SpawnArrow(0, 0, -1, BeatCubeClass.SubType.Down);
            yield return new WaitForSeconds(0.5f);
            SpawnArrow(1, 0, -1, BeatCubeClass.SubType.Up);
            yield return new WaitForSeconds(1);
            SpawnArrow(2, 0, -1, BeatCubeClass.SubType.Down);

            yield return new WaitForSeconds(2);
            SpawnArrow(3, 0, 1, BeatCubeClass.SubType.Down);
            yield return new WaitForSeconds(0.5f);
            SpawnArrow(2, 0, 1, BeatCubeClass.SubType.Up);
            yield return new WaitForSeconds(0.3f);
            SpawnArrow(3, 1, 1, BeatCubeClass.SubType.Right);
            yield return new WaitForSeconds(0.6f);
            SpawnArrow(1, 0, 1, BeatCubeClass.SubType.Left);
        }
        private IEnumerator Step4()
        {
            // Lines
            yield return ShowVideo(linesVideo, "Tutorial-4");
            yield return new WaitForSeconds(3);
            SpawnLine(0, 1);
            yield return new WaitForSeconds(4);
            SpawnLine(2, 1);
            yield return new WaitForSeconds(3);
            SpawnLine(0, 1);
            SpawnLine(3, 1);
        }
        private IEnumerator Step5()
        {
            // Bombs
            yield return ShowVideo(bombsVideo, "Tutorial-5");
            yield return new WaitForSeconds(3);
            SpawnBomb(0);
            yield return new WaitForSeconds(2);
            SpawnBomb(3);
            yield return new WaitForSeconds(1);
            SpawnBomb(0);
            SpawnBomb(0, 1);
            yield return new WaitForSeconds(2);
            SpawnBomb(3);
            SpawnBomb(3, 1);
        }




        private void SpawnArrow(int road, int height, int saberType, BeatCubeClass.SubType direction)
        {
            bm.SpawnBeatCube(new BeatCubeClass()
            {
                road = road,
                level = height,
                saberType = saberType,
                type = BeatCubeClass.Type.Dir,
                subType = direction
            });
        }
        private void SpawnPoint(int road, int height, int saberType)
        {
            bm.SpawnBeatCube(new BeatCubeClass()
            {
                road = road,
                level = height,
                saberType = saberType,
                type = BeatCubeClass.Type.Point
            });
        }
        private void SpawnLine(int road, int lenght)
        {
            bm.SpawnBeatCube(new BeatCubeClass()
            {
                road = road,
                type = BeatCubeClass.Type.Line,
                lineLenght = lenght,
                lineEndRoad = road
            });
        }
        private void SpawnBomb(int road, int height = 0)
        {
            bm.SpawnBeatCube(new BeatCubeClass()
            {
                type = BeatCubeClass.Type.Bomb,
                road = road,
                level = height
            });
        }



        private IEnumerator ShowVideo(VideoClip clip, string hint)
        {
            isVideoSkipped = false;
            isVideoContinued = false;
            isVideoRepeated = false;

            // Stop audio and beats
            asource.pitch = 0;
            asource.Pause();


            // Start loading video
            videoCanvas.alpha = 0;
            videoLocker.SetActive(true);
            meshUI.SetActive(!videoLocker.activeSelf);
            videoPlayer.clip = clip;
            videoPlayer.Play();
            while (!videoPlayer.isPlaying) { yield return null; }

            // Show overlay and play video
            skipBtnOverlay.SetActive(true);
            continueOverlay.SetActive(false);
            hintText.text = LocalizationManager.Localize(hint);

            videoAnimator.Play("Show");
            videoPlayer.Play();





            // Waiting for ending video if skip button not clicked
            double elapsedSeconds = clip.length;
            while (!isVideoSkipped && elapsedSeconds > 0)
            {
                elapsedSeconds -= Time.deltaTime;
                yield return null;
            }
            videoPlayer.Stop();


            skipBtnOverlay.SetActive(false);
            continueOverlay.SetActive(true);

            while (!isVideoContinued) yield return null;


            // Is again button not clicked
            if (!isVideoRepeated)
            {
                videoCanvas.alpha = 0;
                videoLocker.SetActive(false);
                meshUI.SetActive(!videoLocker.activeSelf);

                asource.pitch = 1;
                asource.Play();
            }
            else
            {
                yield return ShowVideo(clip, hint);
            }
        }
    }

    [System.Serializable]
    public class AudioTimeCode
    {
        public AudioClip clip;

        /// <summary>
        /// Time in seconds
        /// </summary>
        public float time;
    }


    public class TutorialResult
    {
        public int AllSliced { get; set; }
        public int AllMissed { get; set; }
        /// <summary>
        /// Accuracy in range 0-1
        /// </summary>
        [JsonIgnore] public float Accuracy => AllSliced / (float)(AllSliced + AllMissed);

        public List<TutorialStep> Steps { get; set; } = new List<TutorialStep>();


        public void AddStep(string name, ReplayData replay)
        {
            Steps.Add(new TutorialStep(name, replay.CubesSliced - AllSliced, replay.Missed - AllMissed));

            AllSliced = replay.CubesSliced;
            AllMissed = replay.Missed;
        }
    }
    public class TutorialStep
    {
        public string Name { get; set; }
        public int Sliced { get; set; }
        public int Missed { get; set; }
        /// <summary>
        /// Accuracy in range 0-1
        /// </summary>
        public float Accuracy => Sliced / (float)(Sliced + Missed);

        public TutorialStep(string name, int sliced, int missed)
        {
            Name = name;
            Sliced = sliced;
            Missed = missed;
        }
    }
}
