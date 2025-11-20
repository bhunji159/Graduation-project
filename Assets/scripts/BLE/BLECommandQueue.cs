using UnityEngine;
using System.Collections;

/// <summary>
/// DuckRunning BLECommandQueue — 안정화 버전
/// - 속도/경사 ramp 명령을 각각 독립적으로 실행
/// - ack 기반 업데이트 + fallback 보정
/// - ramp 충돌 방지
/// </summary>
public class BLECommandQueue : MonoBehaviour
{
    [Header("Speed/Incline Ramp Settings")]
    public float speedStep = 0.5f;        // km/h
    public float inclineStep = 0.5f;      // %
    public float stepDelay = 0.4f;        // 한 단계마다 대기 시간
    public float ackTimeout = 1.0f;       // ESP32 응답 대기 시간

    private float lastAckSpeed = 0f;
    private float lastAckIncline = 0f;

    private bool waitingAck = false;

    private Coroutine speedRoutine;
    private Coroutine inclineRoutine;

    void OnEnable()
    {
        if (BLEManager.Instance)
            BLEManager.Instance.OnResponseReceived += HandleResponse;
    }

    void OnDisable()
    {
        if (BLEManager.Instance)
            BLEManager.Instance.OnResponseReceived -= HandleResponse;
    }

    // =========================================================
    // 외부에서 호출 (오리 이동/이벤트 시스템)
    // =========================================================
    public void SetTargetSpeed(float target)
    {
        if (speedRoutine != null) StopCoroutine(speedRoutine);
        speedRoutine = StartCoroutine(RunRamp("setSpeed", lastAckSpeed, target, speedStep));
    }

    public void SetTargetIncline(float target)
    {
        if (inclineRoutine != null) StopCoroutine(inclineRoutine);
        inclineRoutine = StartCoroutine(RunRamp("setIncline", lastAckIncline, target, inclineStep));
    }

    // =========================================================
    // 메인 램프 로직
    // =========================================================
    private IEnumerator RunRamp(string type, float currentValue, float targetValue, float step)
    {
        float dir = Mathf.Sign(targetValue - currentValue);

        Debug.Log($"[BLEQueue] {type} RAMP START {currentValue} → {targetValue}");

        int safety = 0;

        while (Mathf.Abs(targetValue - currentValue) > 0.05f)
        {
            if (safety++ > 40)
            {
                Debug.LogWarning($"[BLEQueue] {type} ramp safety stop");
                yield break;
            }

            float nextVal = currentValue + dir * step;

            // 목표보다 넘어가지 않도록 clamp
            nextVal = Mathf.Clamp(nextVal,
                Mathf.Min(currentValue, targetValue),
                Mathf.Max(currentValue, targetValue));

            // ===== BLE 명령 전송 =====
            BLEManager.Instance.SendCommand(new BLECommand(type, nextVal));

            waitingAck = true;

            // ACK 대기
            float t = 0f;
            while (waitingAck && t < ackTimeout)
            {
                t += Time.deltaTime;
                yield return null;
            }

            if (waitingAck)
            {
                // timeout — fallback → 방금 보낸 값 사용
                Debug.LogWarning($"[BLEQueue] {type} ack timeout → fallback");
                waitingAck = false;

                if (type == "setSpeed") lastAckSpeed = nextVal;
                if (type == "setIncline") lastAckIncline = nextVal;
            }

            // ack로 받은 실제 값 기반 업데이트
            currentValue = (type == "setSpeed") ? lastAckSpeed : lastAckIncline;

            yield return new WaitForSeconds(stepDelay);
        }

        Debug.Log($"[BLEQueue] {type} RAMP DONE → {targetValue}");
    }

    // =========================================================
    // ESP32 Notify 수신 → ack 처리
    // =========================================================
    private void HandleResponse(BLEResponse r)
    {
        lastAckSpeed = r.speed;
        lastAckIncline = r.incline;

        // ack 완료
        waitingAck = false;
    }
}
