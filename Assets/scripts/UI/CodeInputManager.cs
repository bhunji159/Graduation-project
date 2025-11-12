using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CodeInputManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField inputField;
    public Button confirmButton;

    void Start()
    {
        confirmButton.onClick.AddListener(OnConfirm);
    }

    void OnConfirm()
    {
        string code = inputField.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            Debug.LogWarning("세션 코드를 입력하세요!");
            return;
        }

        PlayerPrefs.SetString("SessionCode", code); // 임시 저장
        Debug.Log($"입력된 코드: {code}");

        // TODO: 나중엔 서버 인증 API 호출
        SceneManager.LoadScene("CourseSelectScene"); // 다음 씬으로 전환
    }
}
