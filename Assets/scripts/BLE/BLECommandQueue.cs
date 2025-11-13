using UnityEngine;
using System.Collections;

/// <summary>
/// DuckRunning BLECommandQueue (확장판)
/// 속도와 경사도를 단계적으로 변경.
/// </summary>
public class BLECommandQueue : MonoBehaviour
{
    [Header("Speed Control Settings")]
    public float speedStep = 0.5f;
    public float inclineStep = 0.1f;
    public float stepDelay = 0.5f;
    public float ackTimeout = 2f;

    private float lastAckSpeed = 0f;
    private float lastAckIncline = 0f;
    private bool awaitingAck = false;

    void OnEnable()
    {
        BLEManager.Instance.OnResponseReceived += HandleResponse;
    }

    void OnDisable()
    {
        if (BLEManager.Instance)
            BLEManager.Instance.OnResponseReceived -= HandleResponse;
    }

    // 🟡 속도 제어 시작
    public void SetTargetSpeed(float targetSpeed)
    {
        StopAllCoroutines();
        StartCoroutine(RunRamp("setSpeed", lastAckSpeed, targetSpeed, speedStep));
    }

    // 🟢 경사 제어 시작
    public void SetTargetIncline(float targetIncline)
    {
        StopAllCoroutines();
        StartCoroutine(RunRamp("setIncline", lastAckIncline, targetIncline, inclineStep));
    }

    IEnumerator RunRamp(string cmdType, float currentValue, float targetValue, float stepSize)
    {
        float dir = Mathf.Sign(targetValue - currentValue);
        int safety = 0;

        Debug.Log($"[BLECommandQueue] {cmdType} {currentValue} → {targetValue}");

        while (Mathf.Abs(targetValue - currentValue) > 0.05f)
        {
            if (safety++ > 50) yield break;

            float nextValue = Mathf.Clamp(currentValue + dir * stepSize,
                                          Mathf.Min(currentValue, targetValue),
                                          Mathf.Max(currentValue, targetValue));

            BLEManager.Instance.SendCommand(new BLECommand(cmdType, nextValue));
            awaitingAck = true;

            float timer = 0f;
            while (awaitingAck && timer < ackTimeout)
            {
                timer += Time.deltaTime;
                yield return null;
            }

            if (awaitingAck)
            {
                Debug.LogWarning($"[BLECommandQueue] {cmdType} ack timeout — continue");
                awaitingAck = false;
            }

            currentValue = (cmdType == "setSpeed") ? lastAckSpeed : lastAckIncline;
            yield return new WaitForSeconds(stepDelay);
        }

        Debug.Log($"[BLECommandQueue] {cmdType} target reached: {targetValue:0.0}");
    }

    private void HandleResponse(BLEResponse res)
    {
        lastAckSpeed = res.speed;
        lastAckIncline = res.incline;
        awaitingAck = false;
    }
}
