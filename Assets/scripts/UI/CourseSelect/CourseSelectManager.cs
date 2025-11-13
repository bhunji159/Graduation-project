using UnityEngine;
using UnityEngine.UI;
using DuckRunning.Course;
using DuckRunning.Core; // GameManager 위치에 맞게 조정

/// <summary>
/// 코스 선택 화면 관리:
/// - 모든 CourseCard를 초기화
/// - 클릭 시 선택 상태 반영
/// - Start 버튼으로 GameManager에 CourseData 전달
/// </summary>
public class CourseSelectManager : MonoBehaviour
{
    [Header("UI References")]
    public CourseCard[] cards;
    public Button startButton;

    private CourseCard selectedCard;

    private void Start()
    {
        // 모든 카드 초기화
        foreach (var c in cards)
            c.Init(this);

        // 시작 버튼 비활성화
        if (startButton != null)
        {
            startButton.interactable = false;
            startButton.onClick.AddListener(OnStartClicked);
        }
    }

    /// <summary>
    /// 카드 선택 시 호출 (CourseCard → OnSelected)
    /// </summary>
    public void OnCourseSelected(CourseCard card)
    {
        selectedCard = card;

        foreach (var c in cards)
            c.SetSelected(c == card);

        if (startButton)
            startButton.interactable = true;
    }

    /// <summary>
    /// Start 버튼 클릭 시 게임 시작
    /// </summary>
    private void OnStartClicked()
    {
        if (selectedCard == null || selectedCard.course == null)
        {
            Debug.LogWarning("[CourseSelectManager] 선택된 코스가 없습니다.");
            return;
        }

        var course = selectedCard.course;
        Debug.Log($"[CourseSelectManager] 선택된 코스: {course.displayName} (Type: {course.courseType})");

        // GameManager에 전달
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetSelectedCourse(course);
            GameManager.Instance.StartGame();
        }
        else
        {
            Debug.LogError("[CourseSelectManager] GameManager 인스턴스를 찾을 수 없습니다!");
        }
    }
}
