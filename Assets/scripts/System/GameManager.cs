using UnityEngine;
using UnityEngine.SceneManagement;
using DuckRunning.Course;

namespace DuckRunning.Core
{
    /// <summary>
    /// DuckRunning 전역 게임 매니저
    /// - 코스 선택, 게임 시작, 씬 유지 관리
    /// - DontDestroyOnLoad로 씬 전환 시 유지
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Current Game Data")]
        public CourseData currentCourse;
        public bool isInitialized = false;

        /// <summary>
        /// 현재 선택된 코스의 타입 (Easy / Normal / Hard)
        /// </summary>
        public CourseType CurrentCourseType => currentCourse ? currentCourse.courseType : CourseType.Easy;

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

        /// <summary>
        /// 선택된 코스를 저장하고 초기화 상태로 전환
        /// </summary>
        public void SetSelectedCourse(CourseData data)
        {
            if (data == null)
            {
                Debug.LogWarning("[GameManager] 코스 데이터가 null 입니다!");
                return;
            }

            currentCourse = data;
            isInitialized = true;

            Debug.Log($"[GameManager] 코스 선택됨: {data.displayName} (Type: {data.courseType})");
        }

        /// <summary>
        /// 선택된 코스 기반으로 게임 시작
        /// </summary>
        public void StartGame()
        {
            if (!isInitialized || currentCourse == null)
            {
                Debug.LogWarning("[GameManager] 코스가 초기화되지 않았습니다!");
                return;
            }

            Debug.Log($"[GameManager] '{currentCourse.displayName}' 코스 시작 (Scene: MainGameScene)");
            SceneManager.LoadScene("MainGameScene");
        }
    }
}
