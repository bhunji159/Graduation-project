using UnityEngine;
using UnityEngine.UI;
using GraduationProject.Course;

public class CourseSelectManager : MonoBehaviour
{
    [Header("UI References")]
    public CourseCard[] cards;
    public Button startButton;

    private CourseCard selectedCard;

    private void Start()
    {
        foreach (var c in cards)
            c.Init(this);

        startButton.interactable = false;
        startButton.onClick.AddListener(OnStartClicked);
    }

    public void OnCourseSelected(CourseCard card)
    {
        selectedCard = card;

        foreach (var c in cards)
            c.SetSelected(c == card);

        startButton.interactable = true;
    }

    private void OnStartClicked()
    {
        if (selectedCard == null || selectedCard.course == null)
        {
            Debug.LogWarning("[CourseSelectManager] 선택된 코스가 없습니다.");
            return;
        }

        Debug.Log($"[CourseSelectManager] 선택된 코스: {selectedCard.course.displayName} (ID: {selectedCard.course.CourseId})");

        GameManager.Instance.SetSelectedCourse(selectedCard.course);
        GameManager.Instance.StartGame();
    }
}
