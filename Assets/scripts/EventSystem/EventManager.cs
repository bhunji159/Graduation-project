using UnityEngine;
using System.Collections;

namespace DuckRunning.Events
{
    public class EventManager : MonoBehaviour
    {
        public static EventManager Instance { get; private set; }

        [Header("상태")]
        public bool isEventActive { get; private set; } = false;   // 현재 이벤트 활성화 여부

        private IEventBehaviour activeBehaviour;   // 현재 실행 중인 이벤트 로직
        private EventDefinition currentDefinition; // 현재 이벤트 데이터
        private GameObject spawnedEventObject;     // 인스턴스화된 이벤트 프리팹
        private HUDController hud;                 // HUD 참조 (이벤트 진행도 표시용)
        private bool cancelRequested = false;

        // ===========================================================
        // 초기화
        // ===========================================================
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // HUDController 자동 참조
            hud = Object.FindFirstObjectByType<HUDController>();
        }

        // ===========================================================
        // 이벤트 실행 진입점
        // ===========================================================
        public void RunEvent(EventDefinition def)
        {
            if (def == null)
            {
                Debug.LogWarning("[EventManager] EventDefinition이 null입니다.");
                return;
            }

            if (isEventActive)
            {
                Debug.LogWarning("[EventManager] 이미 이벤트가 실행 중입니다.");
                return;
            }

            currentDefinition = def;
            StartCoroutine(RunEventRoutine());
        }

        // ===========================================================
        // 이벤트 실행 루틴
        // ===========================================================
        private IEnumerator RunEventRoutine()
        {
            isEventActive = true;
            Debug.Log($"[EventManager] 이벤트 시작: {currentDefinition.displayName}");

            // HUDController 가져오기 (씬 변경 시 다시 찾을 수 있도록)
            if (!hud)
                hud = Object.FindFirstObjectByType<HUDController>();

            // HUD에 이벤트 UI 표시
            hud?.ShowEventProgress(currentDefinition.displayName);

            // 이벤트 프리팹 인스턴스화
            if (currentDefinition.eventPrefab != null)
            {
                spawnedEventObject = Instantiate(currentDefinition.eventPrefab);
                activeBehaviour = spawnedEventObject.GetComponent<IEventBehaviour>();
                activeBehaviour?.Initialize(currentDefinition);
            }

            // 이벤트 시작
            activeBehaviour?.OnStart();

            float elapsed = 0f;
            float duration = currentDefinition.durationSeconds;

            // 이벤트 진행 루프
            while (elapsed < duration)
            {
                // CANCEL CHECK
                if (cancelRequested)
                {
                    Debug.Log("[EventManager] 이벤트 강제 중단됨 (Give Up)");

                    activeBehaviour?.OnEnd(false); // 실패 처리
                    hud?.HideEventProgress();

                    if (spawnedEventObject)
                        Destroy(spawnedEventObject);

                    isEventActive = false;
                    cancelRequested = false;

                    yield break; // 이벤트 즉시 종료
                }

                elapsed += Time.deltaTime;

                float progress = Mathf.Clamp01(elapsed / duration);
                hud?.UpdateEventProgress(progress);

                activeBehaviour?.OnUpdate(elapsed);

                yield return null;
            }


            // 이벤트 종료 처리
            bool success = true; // 포기하지 않은 경우 기본 성공 처리
            activeBehaviour?.OnEnd(success);

            // HUD에서 이벤트 UI 숨기기
            hud?.HideEventProgress();

            // 보상 처리, BLE 통신 등은 이후 연동 예정
            Debug.Log($"[EventManager] 이벤트 종료: {currentDefinition.displayName}");

            // 상태 초기화
            isEventActive = false;

            // 이벤트 오브젝트 정리
            if (spawnedEventObject)
                Destroy(spawnedEventObject);
        }
        public void CancelEvent()
        {
            if (!isEventActive) return;

            cancelRequested = true;
        }
    }

}
