using UnityEngine;

namespace GraduationProject.Course
{
    public enum CourseType
    {
        Easy = 0,
        Normal = 1,
        Hard = 2
    }

    [CreateAssetMenu(fileName = "CourseData", menuName = "Graduation Project/Course Data")]
    public class CourseData : ScriptableObject
    {
        [Header("Identity")]
        public CourseType courseType;                // 인스펙터용 (드롭다운)
        public int CourseId => (int)courseType;      // 내부 ID
        public string displayName;                   // UI 표기용 이름

        [Header("Overview (for card UI)")]
        public float lengthMeters = 3000f;           // 총 거리 (m)
        public int targetMinutes = 20;               // 목표 시간 (분)
        public float avgSpeedKmh = 9.0f;             // 평균 속도 (km/h)
        public float avgInclinePercent = 2.0f;       // 평균 경사 (%)

        [Header("Track Tiles (main lane)")]
        public float tileLength = 10f;               // 타일 하나의 길이 (m)
        public WeightedPrefab[] tileSet;

        [Header("Side Decorations (left/right)")]
        public SpawnRule leftDecor;
        public SpawnRule rightDecor;

        [Header("Events (coins, boost, etc.)")]
        public EventRule[] events;

        [Header("Visual Slope")]
        public AnimationCurve slopeDegByProgress = AnimationCurve.Linear(0, 0, 1, 0);
    }

    [System.Serializable]
    public struct WeightedPrefab
    {
        public GameObject prefab;
        [Range(0.01f, 10f)] public float weight;
    }

    [System.Serializable]
    public struct SpawnRule
    {
        [Tooltip("100m 당 평균 몇 개 생성할지")]
        public float densityPer100m;
        public WeightedPrefab[] set;
        public Vector2 xRange;
        public Vector2 yOffsetRange;
        public Vector2 zJitterRange;
        public float minSpacingMeters;
    }

    [System.Serializable]
    public struct EventRule
    {
        public string eventId;
        [Range(0f, 1f)] public float chancePer100m;
        public float minIntervalMeters;
        public WeightedPrefab[] set;
    }
}
