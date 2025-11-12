using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GraduationProject.Course;

public class CourseCard : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text titleText;
    public TMP_Text[] labels;
    public TMP_Text[] values;
    public Image background;

    [Header("Course Info")]
    public CourseData course;

    private Button button;
    private CourseSelectManager manager;

    private void Awake()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnSelected);
    }

    public void Init(CourseSelectManager mgr)
    {
        manager = mgr;
        UpdateCardUI();
    }

    private void OnSelected()
    {
        manager.OnCourseSelected(this);
    }

    public void SetSelected(bool selected)
    {
        background.color = selected ? new Color(1f, 0.9f, 0.6f) : Color.white;
    }

    private void UpdateCardUI()
    {
        if (!course) return;

        titleText.text = course.displayName;
        labels[0].text = "Distance";
        labels[1].text = "time";
        labels[2].text = "speed";
        labels[3].text = "incline";

        values[0].text = $"{course.lengthMeters:F0} m";
        values[1].text = $"{course.targetMinutes} min";
        values[2].text = $"{course.avgSpeedKmh:F1} km/h";
        values[3].text = $"{course.avgInclinePercent:F1} %";
    }
}
