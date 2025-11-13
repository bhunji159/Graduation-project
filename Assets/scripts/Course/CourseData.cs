using UnityEngine;

namespace DuckRunning.Course
{
    public enum CourseType
    {
        Easy,
        Normal,
        Hard
    }

    [CreateAssetMenu(fileName = "CourseData", menuName = "DuckRunning/Course Data")]
    public class CourseData : ScriptableObject
    {
        [Header("Basic Info")]
        public CourseType courseType = CourseType.Easy;

        [Tooltip("UI에 표시될 이름")]
        public string displayName = "Standard Course";

        [Tooltip("총 거리 (미터)")]
        public float totalLengthMeters = 3000f;

        [Tooltip("기본 러닝 속도 (km/h)")]
        public float baseSpeedKmh = 8f;

    }
}
