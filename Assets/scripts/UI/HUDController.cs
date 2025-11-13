using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class HUDController : MonoBehaviour
{
    [Header("Main HUD")]
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI distanceText;
    public TextMeshProUGUI inclineText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI avgSpeedText;
    public TextMeshProUGUI nextEventText;

    [Header("Event Progress UI")]
    public GameObject eventProgressGroup;
    public TextMeshProUGUI eventTitleText;
    public Slider eventProgressBar;
    public Button cancelButton;   // <- GIVE UP 버튼

    [Header("Unified Event UI")]
    public GameObject eventProposalGroup;
    public TextMeshProUGUI proposalMessageText;

    [Header("Buttons (Accept / Decline)")]
    public GameObject buttonGroup;
    public Button acceptButton;
    public Button declineButton;
    public TextMeshProUGUI acceptButtonText;
    public TextMeshProUGUI declineButtonText;

    [Header("References")]
    public DuckRunController duckController;

    // CourseRunner에서 사용할 콜백
    public Action<bool> OnPlayerSelected;
    public Action OnCancelPressed;   // <- 이벤트 중단 콜백

    private int currentSelection = 2; // 1 = Accept, 2 = Decline (기본값: Decline)
    private float elapsedTime = 0f;
    private float totalDistance = 0f;
    private float totalTime = 0f;
    private bool isEventActive = false;

    private void Update()
    {
        if (!duckController) return;

        elapsedTime += Time.deltaTime;
        totalTime += Time.deltaTime;

        float speed = duckController.currentSpeed;
        float distance = duckController.distanceMeters;
        float incline = duckController.currentIncline;

        if (totalTime > 0f)
            totalDistance = distance;

        float avgSpeed = (totalDistance / totalTime) * 3.6f;

        speedText.text = $"Speed: {speed:F1} km/h";
        inclineText.text = $"Incline: {incline:F1}%";
        distanceText.text = $"Distance: {distance:F1} m";
        avgSpeedText.text = $"Avg Speed: {avgSpeed:F1} km/h";

        int mm = Mathf.FloorToInt(elapsedTime / 60f);
        int ss = Mathf.FloorToInt(elapsedTime % 60f);
        timeText.text = $"Time: {mm:00}:{ss:00}";

        if (isEventActive)
            nextEventText.text = "Event in progress!";
    }

    private void Start()
    {
        // 기본적으로 UI 꺼두기
        eventProposalGroup.SetActive(false);
        buttonGroup.SetActive(false);
        eventProgressGroup.SetActive(false);

        // Cancel(Give Up) 버튼 클릭
        cancelButton.onClick.AddListener(() => OnCancelPressed?.Invoke());

        // 제안 버튼 클릭
        acceptButton.onClick.AddListener(() => SelectOption(true));
        declineButton.onClick.AddListener(() => SelectOption(false));

        // Cancel 버튼 텍스트 명시적으로 설정
        cancelButton.GetComponentInChildren<TextMeshProUGUI>().text = "Give Up";
    }


    // Accept / Decline 클릭 시 선택 변경
    private void SelectOption(bool accepted)
    {
        currentSelection = accepted ? 1 : 2;
        HighlightSelection(currentSelection);
        OnPlayerSelected?.Invoke(accepted);
    }

    // 버튼 강조(선택된 버튼 색상 변경)
    public void HighlightSelection(int sel)
    {
        // Accept 버튼
        var ac = acceptButton.colors;
        ac.normalColor = (sel == 1) ? new Color(0.8f, 0.9f, 1f) : Color.white;
        acceptButton.colors = ac;

        // Decline 버튼
        var dc = declineButton.colors;
        dc.normalColor = (sel == 2) ? new Color(0.8f, 0.9f, 1f) : Color.white;
        declineButton.colors = dc;
    }

    // =====================================================================
    // Proposal UI
    // =====================================================================
    public void ShowEventProposal(string title)
    {
        eventProposalGroup.SetActive(true);
        proposalMessageText.text = $"Incoming event:\n{title}";

        buttonGroup.SetActive(true);

        // 기본 선택 = Decline
        currentSelection = 2;
        HighlightSelection(2);
    }

    public void UpdateProposalButtons(float remain)
    {
        if (currentSelection == 1)     // Accept 선택됨
        {
            acceptButtonText.text = $"Go! ({remain:F0})";
            declineButtonText.text = "Skip";
        }
        else                           // Decline 선택됨 (기본)
        {
            acceptButtonText.text = "Go!";
            declineButtonText.text = $"Skip ({remain:F0})";
        }
    }

    public void HideEventProposal()
    {
        eventProposalGroup.SetActive(false);
        buttonGroup.SetActive(false);
    }

    // =====================================================================
    // Countdown
    // =====================================================================
    public void ShowCountdown(float sec)
    {
        eventProposalGroup.SetActive(true);
        buttonGroup.SetActive(false);
        proposalMessageText.text = $"{sec:F0}";
    }

    public void UpdateCountdown(float sec)
    {
        proposalMessageText.text = $"{sec:F0}";
    }

    public void HideCountdown()
    {
        eventProposalGroup.SetActive(false);
    }

    // =====================================================================
    // Event Progress
    // =====================================================================
    public void ShowEventProgress(string eventName)
    {
        eventProgressGroup.SetActive(true);

        // Cancel button text = Give Up
        cancelButton.GetComponentInChildren<TextMeshProUGUI>().text = "Give Up";

        eventTitleText.text = $"Event: {eventName}";
        eventProgressBar.value = 0f;
    }

    public void UpdateEventProgress(float p)
    {
        eventProgressBar.value = Mathf.Clamp01(p);
    }

    public void HideEventProgress()
    {
        eventProgressGroup.SetActive(false);
    }

    // =====================================================================
    // Result
    // =====================================================================
    public void ShowEventResult(string name, int reward)
    {
        eventProposalGroup.SetActive(true);
        buttonGroup.SetActive(false);

        proposalMessageText.text =
            $"{name} Completed!\n<color=#FFD700>Reward: {reward} Gold</color>";
    }


    public void HideEventResult()
    {
        eventProposalGroup.SetActive(false);
    }

    // =====================================================================
    // Declined
    // =====================================================================
    public void ShowEventDeclined()
    {
        eventProposalGroup.SetActive(true);
        buttonGroup.SetActive(false);
        proposalMessageText.text = "Event declined";
    }

    public void ShowEventGivenUp()
    {
        eventProposalGroup.SetActive(true);
        buttonGroup.SetActive(false);
        proposalMessageText.text = "Event given up";
    }

    public void HideDeclined()
    {
        eventProposalGroup.SetActive(false);
    }

    // =====================================================================
    // Next Event Timer
    // =====================================================================
    public void UpdateNextEventTimer(float sec)
    {
        if (sec < 0)
        {
            nextEventText.text = "";
            return;
        }

        if (sec == 0)
        {
            nextEventText.text = "Event starting!";
            return;
        }

        int mm = Mathf.FloorToInt(sec / 60f);
        int ss = Mathf.FloorToInt(sec % 60f);

        nextEventText.text = $"Next event in: {mm:00}:{ss:00}";
    }
}
