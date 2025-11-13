using UnityEngine;
using DuckRunning.Events;
using DuckRunning.Course; // CourseType 참조 (Easy, Normal, Hard)
using DuckRunning.Core;  // GameManager 참조

namespace DuckRunning.EventBehaviours
{
    public class KingRaceBehaviour : MonoBehaviour, IEventBehaviour
    {
        private EventDefinition def;
        private GameObject king;              // 🧱 왕 프리팹 인스턴스
        private Transform player;             // 오리 플레이어 Transform
        private Animator kingAnim;

        private float targetSpeed;
        private float targetIncline;
        private bool started;

        // 난이도 가져오기 (GameManager.currentCourse.courseType)
        private CourseType difficulty => GameManager.Instance.currentCourse.courseType;
        
        //  초기화
        public void Initialize(EventDefinition definition)
        {
            def = definition;
        }

        //  시작
        public void OnStart()
        {
            player = GameObject.FindWithTag("Player")?.transform;
            if (player == null)
            {
                Debug.LogWarning("[KingRace] Player not found!");
                return;
            }

            // 🧮 난이도 기반 목표 계산
            switch (difficulty)
            {
                case CourseType.Easy:
                    targetSpeed = def.targetSpeed;
                    targetIncline = def.targetIncline;
                    break;

                case CourseType.Normal:
                    targetSpeed = def.targetSpeed * 1.5f;
                    targetIncline = def.targetIncline + 3f;
                    break;

                case CourseType.Hard:
                    targetSpeed = def.targetSpeed * 2f;
                    targetIncline = def.targetIncline + 6f;
                    break;
            }

            // 👑 왕 프리팹 등장 (Resources/Prefabs/King.prefab)
            GameObject prefab = Resources.Load<GameObject>("Prefabs/King");
            king = Instantiate(prefab);
            king.transform.position = player.position + new Vector3(2.5f, 0f, 0f);

            kingAnim = king.GetComponent<Animator>();

            started = true;

            Debug.Log($"[KingRace] {difficulty} 난이도 시작! 목표 속도: {targetSpeed:F1}km/h, 경사 {targetIncline}%");
        }

        // =======================================================
        //  진행 중 (매 프레임)
        // =======================================================
        public void OnUpdate(float elapsed)
        {
            if (!started || king == null || player == null)
                return;

            // 🎯 플레이어 따라 달리기
            Vector3 targetPos = player.position + new Vector3(2.5f, 0, 1f);
            king.transform.position = Vector3.Lerp(
                king.transform.position,
                targetPos,
                Time.deltaTime * 2f
            );

            // 🕒 진행 상태 디버그
            if (elapsed % 10f < 0.02f)
                Debug.Log($"[KingRace] 진행 중... {elapsed:F1}s / 목표속도 {targetSpeed:F1}");
        }

        // =======================================================
        //  종료
        // =======================================================
        public void OnEnd(bool success)
        {
            if (success)
            {
                Debug.Log($"[KingRace] 성공! {difficulty} 클리어 보상: +{def.rewardCoins}G");
            }
            else
            {
                Debug.Log("[KingRace] 실패! 왕이 먼저 결승점에 도착했습니다...");
            }

            // 일정 시간 후 제거
            Destroy(king, 2f);
            started = false;
        }
    }
}
