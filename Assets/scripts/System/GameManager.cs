using UnityEngine;
using UnityEngine.SceneManagement;
using GraduationProject.Course;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Current Game Data")]
    public CourseData currentCourse;
    public bool isInitialized = false;

    public int CurrentCourseId => currentCourse ? currentCourse.CourseId : -1;

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

    public void SetSelectedCourse(CourseData data)
    {
        currentCourse = data;
        isInitialized = true;

        Debug.Log($"[GameManager] 코스 선택됨: {data.displayName} (ID: {data.CourseId})");
    }

    public void StartGame()
    {
        if (!isInitialized)
        {
            Debug.LogWarning("[GameManager] 코스가 초기화되지 않았습니다!");
            return;
        }

        SceneManager.LoadScene("MainGameScene");
    }
}
