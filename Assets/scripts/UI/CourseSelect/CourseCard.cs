using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DuckRunning.Course;

[RequireComponent(typeof(Button))]
public class CourseCard : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text titleText;
    public TMP_Text[] labels;   // 0: Distance, 1: Time, 2: Base Speed
    public TMP_Text[] values;   // 각 라벨 대응 값
    public Image background;

    [Header("Course Info")]
    public CourseData course;

    private CourseSelectManager manager;
    private Button button;

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
        if (manager != null)
            manager.OnCourseSelected(this);
    }

    public void SetSelected(bool selected)
    {
        if (!background) return;
        background.color = selected ? new Color(1f, 0.9f, 0.6f) : Color.white;
    }

    private void UpdateCardUI()
    {
        if (!course)
        {
            Debug.LogWarning("[CourseCard] course 미지정");
            return;
        }

        if (titleText)
            titleText.text = string.IsNullOrEmpty(course.displayName) ? course.name : course.displayName;

        // 라벨 설정
        if (labels != null && labels.Length >= 3)
        {
            labels[0].text = "Distance";
            labels[1].text = "Time";
            labels[2].text = "Base Speed";
        }

        // ✅ 값 계산 (이제 세그먼트 X → 거리 & 속도로 계산)
        float distanceM = Mathf.Max(0f, course.totalLengthMeters);
        float baseSpeed = Mathf.Max(0.1f, course.baseSpeedKmh); // 0 방지
        float totalSec = (distanceM / (baseSpeed * 1000f / 3600f)); // 거리 ÷ (km/h → m/s 변환)

        // 시간 포맷
        string timeStr = "--:--";
        if (totalSec > 0f)
        {
            int mm = Mathf.FloorToInt(totalSec / 60f);
            int ss = Mathf.FloorToInt(totalSec % 60f);
            timeStr = $"{mm:00}:{ss:00}";
        }

        // UI 반영
        if (values != null && values.Length >= 3)
        {
            values[0].text = $"{distanceM:F0} m";
            values[1].text = timeStr;
            values[2].text = $"{baseSpeed:F1} km/h";
        }
    }
}
