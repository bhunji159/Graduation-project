using UnityEngine;

/// <summary>
/// DuckRunning — 오리 이동 + 애니메이션 제어 + BLE 데이터 연동
/// (코스는 평평하게 두고 오리와 카메라만 기울임)
/// </summary>
public class DuckRunController : MonoBehaviour
{
    [Header("References")]
    public ChickRun chickAnimator;     // 오리 달리기 애니메이터
    public Transform cameraTransform;  // 메인 카메라 (기울이기용)

    [Header("Visual Settings")]
    public float animationScale = 0.25f;       // 속도(km/h) → 애니메이션 속도 변환 비율
    public float moveSmooth = 5f;              // 부드러운 속도 보간 정도
    public float worldSpeedScale = 2f;         // 전체 이동 속도 조절
    public float bodyTiltFactor = 1.5f;        // 경사도에 따른 몸체 기울기 배율
    public float cameraTiltFactor = 0.8f;      // 경사도에 따른 카메라 기울기 배율

    [Header("Runtime")]
    public float currentSpeed;    // km/h (BLE에서 받은 값)
    public float currentIncline;  // %
    public float distanceMeters;  // 누적 이동 거리

    private float smoothSpeed;    // 보간된 이동 속도
    private Quaternion cameraBaseRot;

    void Start()
    {
        if (cameraTransform)
            cameraBaseRot = cameraTransform.localRotation;
    }

    void OnEnable()
    {
        if (BLEManager.Instance)
            BLEManager.Instance.OnResponseReceived += OnBLEData;
    }

    void OnDisable()
    {
        if (BLEManager.Instance)
            BLEManager.Instance.OnResponseReceived -= OnBLEData;
    }

    void Update()
    {
        // BLE 응답 기반 속도 보간 (부드럽게 가속/감속)
        smoothSpeed = Mathf.Lerp(smoothSpeed, currentSpeed, Time.deltaTime * moveSmooth);

        // 실제 이동 (m/s)
        float dz = (smoothSpeed / 3.6f) * Time.deltaTime * worldSpeedScale;
        transform.Translate(0, 0, dz, Space.World);
        distanceMeters += dz/worldSpeedScale;

        // 🦆 몸 기울이기 (언덕 시각화용)
        if (chickAnimator && chickAnimator.body != null)
        {
            float bodyTilt = Mathf.Clamp(currentIncline * bodyTiltFactor, -20f, 20f);
            chickAnimator.body.localRotation = Quaternion.Euler(bodyTilt, 0f, 0f);
        }

        // 🎥 카메라 기울이기
        if (cameraTransform)
        {
            float camTilt = Mathf.Clamp(currentIncline * cameraTiltFactor, -10f, 10f);
            Quaternion targetRot = cameraBaseRot * Quaternion.Euler(-camTilt, 0f, 0f);
            cameraTransform.localRotation = Quaternion.Slerp(cameraTransform.localRotation, targetRot, Time.deltaTime * 3f);
        }

        // 🟡 애니메이션 속도 제어
        if (chickAnimator)
        {
            float animSpeed = Mathf.Max(0f, smoothSpeed * animationScale);
            chickAnimator.runSpeed = animSpeed;
        }
    }

    void OnBLEData(BLEResponse data)
    {
        currentSpeed = data.speed;
        currentIncline = data.incline;
    }
}
