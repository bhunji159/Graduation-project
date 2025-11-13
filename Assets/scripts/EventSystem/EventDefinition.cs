using UnityEngine;

namespace DuckRunning.Events
{
    public enum EventType
    {
        SpeedUp,
        InclineUp,
        Scenario // 복합 연출형 (예: 괴물 추격, 폭풍 언덕 등)
    }

    [CreateAssetMenu(fileName = "EventDefinition", menuName = "DuckRunning/Event Definition")]
    public class EventDefinition : ScriptableObject
    {
        [Header("기본 정보")]
        public string eventId = "default";
        public string displayName = "New Event";
        [TextArea(2, 4)] public string description;

        [Header("이벤트 타입 및 목표")]
        public EventType type = EventType.SpeedUp;
        public float targetSpeed = 6f;
        public float targetIncline = 0f;
        public float durationSeconds = 120f;

        [Header("연출 관련")]
        public GameObject eventPrefab;   // IEventBehaviour 붙은 프리팹
        public AudioClip eventBGM;
        public Sprite eventIcon;

        [Header("보상")]
        public int rewardCoins = 100;

        [Header("UI")]
        public bool allowForfeit = true;
    }
}
