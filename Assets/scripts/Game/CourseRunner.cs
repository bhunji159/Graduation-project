using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DuckRunning.Core;
using DuckRunning.Course;
using DuckRunning.Events;

namespace DuckRunning.Gameplay
{
    public class CourseRunner : MonoBehaviour
    {
        public CourseData courseData;
        public List<EventDefinition> eventPool = new List<EventDefinition>();

        public float runDurationSec = 180f;
        public float eventDurationSec = 120f;

        public float proposalDuration = 7f;
        public float countdownDuration = 3f;
        public float resultDuration = 5f;

        private bool isRunning = false;
        private float totalCourseTimeSec;
        private Coroutine runnerCoroutine;
        private HUDController hud;

        // Cancel 체크용
        private bool cancelRequested = false;

        private void Awake()
        {
            hud = Object.FindFirstObjectByType<HUDController>();
        }

        private void Start()
        {
            courseData = GameManager.Instance?.currentCourse ?? courseData;
            if (courseData == null) return;

            totalCourseTimeSec =
                courseData.totalLengthMeters /
                (courseData.baseSpeedKmh * 1000f / 3600f);

            StartCourse();
        }

        public void StartCourse()
        {
            if (runnerCoroutine != null)
                StopCoroutine(runnerCoroutine);

            runnerCoroutine = StartCoroutine(RunCourseRoutine());
        }

        private IEnumerator RunCourseRoutine()
        {
            isRunning = true;
            float elapsed = 0f;

            while (elapsed < totalCourseTimeSec)
            {
                yield return RunSegment(runDurationSec);
                elapsed += runDurationSec;

                if (elapsed < totalCourseTimeSec)
                {
                    yield return EventFlow();
                    elapsed += eventDurationSec;
                }
            }

            isRunning = false;
        }

        private IEnumerator RunSegment(float dur)
        {
            float t = 0f;

            while (t < dur)
            {
                t += Time.deltaTime;
                hud.UpdateNextEventTimer(dur - t);
                yield return null;
            }

            hud.UpdateNextEventTimer(0);
        }

        private IEnumerator EventFlow()
        {
            int idx = Random.Range(0, eventPool.Count);
            EventDefinition e = eventPool[idx];

            float timer = 0f;
            int selectedOption = 2; // 기본 선택 = Decline

            hud.ShowEventProposal(e.displayName);

            // 버튼 선택 콜백
            hud.OnPlayerSelected = (bool accept) =>
            {
                selectedOption = accept ? 1 : 2;
            };

            // Proposal 7초 동안 대기
            while (timer < proposalDuration)
            {
                float remain = proposalDuration - timer;
                hud.UpdateProposalButtons(remain);

                timer += Time.deltaTime;
                yield return null;
            }

            hud.HideEventProposal();

            // 최종 결정
            if (selectedOption == 1)
                yield return StartEventSequence(e);
            else
                yield return SkipEventSequence();
        }

        private IEnumerator StartEventSequence(EventDefinition e)
        {
            float t = countdownDuration;
            hud.ShowCountdown(t);

            // Countdown
            while (t > 0)
            {
                t -= Time.deltaTime;
                hud.UpdateCountdown(t);
                yield return null;
            }

            hud.HideCountdown();

            // 이벤트 시작
            EventManager.Instance.RunEvent(e);

            // Cancel 이벤트 등록
            cancelRequested = false;
            hud.OnCancelPressed = () =>
                {
                    cancelRequested = true;
                    EventManager.Instance.CancelEvent();
                };

            float elapsed = 0f;

            // 이벤트 진행 루프
            while (elapsed < eventDurationSec)
            {
                // GIVE UP 누름 → 즉시 중단
                if (cancelRequested)
                {
                    cancelRequested = false;
                    yield return SkipEventSequence();
                    yield break;  // 이벤트 종료
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 이벤트 정상 종료
            hud.ShowEventResult(e.displayName, e.rewardCoins);
            yield return new WaitForSeconds(resultDuration);
            hud.HideEventResult();

            hud.UpdateNextEventTimer(runDurationSec);
        }

        private IEnumerator SkipEventSequence()
        {
            hud.HideEventProgress();

            hud.ShowEventGivenUp();
            yield return new WaitForSeconds(3f);
            hud.HideDeclined();
            // 이벤트 + 런의 전체 템포 유지
            hud.UpdateNextEventTimer(runDurationSec + eventDurationSec);
        }
    }
}
