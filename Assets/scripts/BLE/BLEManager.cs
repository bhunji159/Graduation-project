using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// DuckRunning BLEManager — 안정판
/// - Write 간격 보장
/// - setSpeed / setIncline 둘 다 처리
/// - 시뮬레이션 & 실제 BLE 호환
/// - Notify → BLEResponse 전달
/// </summary>
public class BLEManager : MonoBehaviour
{
    public static BLEManager Instance { get; private set; }

    [Header("Mode")]
    public bool simulateBLE = true;               // 시뮬레이션 모드
    public float simulateUpdateRate = 0.2f;       // BLE Notify 빈도

    [Header("Simulated State")]
    public float fakeSpeed = 0f;
    public float fakeIncline = 0f;
    public float fakeDistance = 0f;

    [Header("BLE Write Settings")]
    public float writeInterval = 0.12f;           // 실제 BLE에서 80~150ms 권장
    private float lastWriteTime = 0f;

    private bool isConnected = false;

    public event Action<BLEResponse> OnResponseReceived;

    // BLE 명령 큐
    private Queue<BLECommand> commandQueue = new Queue<BLECommand>();

    // 시뮬레이션용 코루틴
    private Coroutine speedRampRoutine;
    private Coroutine inclineRampRoutine;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (simulateBLE)
        {
            isConnected = true;
            StartCoroutine(SimulateBLELoop());
        }
        else
        {
            // TODO: 실제 BLE SDK 초기화/스캔/연결
        }

        // 커맨드 처리 루프 시작
        StartCoroutine(CommandWriteLoop());
    }

    // =========================================================
    // BLE 명령 큐에 넣기
    // =========================================================
    public void SendCommand(BLECommand cmd)
    {
        if (!isConnected)
        {
            Debug.LogWarning("[BLEManager] Not connected!");
            return;
        }

        commandQueue.Enqueue(cmd);
        Debug.Log($"[BLEManager] QUEUE → {cmd.cmd}({cmd.value})");
    }

    // =========================================================
    // Write 루프 — BLE Write는 한 번에 한 개씩만 전송
    // =========================================================
    private IEnumerator CommandWriteLoop()
    {
        while (true)
        {
            if (commandQueue.Count > 0)
            {
                var cmd = commandQueue.Dequeue();

                // Write rate limit
                float delta = Time.time - lastWriteTime;
                if (delta < writeInterval)
                    yield return new WaitForSeconds(writeInterval - delta);

                lastWriteTime = Time.time;

                if (!simulateBLE)
                {
                    // TODO: BLE SDK로 실제 writeCharacteristic 전송
                }

                // 시뮬레이션 모드라면 내부 처리
                if (simulateBLE)
                    HandleSimulatedCommand(cmd);

                Debug.Log($"[BLEManager] WRITE → {cmd.cmd}({cmd.value})");
            }

            yield return null;
        }
    }

    // =========================================================
    // 실제 BLE Notify 들어올 때 호출될 함수
    // =========================================================
    public void HandleBLEMessage(string json)
    {
        BLEResponse res = BLEResponse.FromJson(json);
        OnResponseReceived?.Invoke(res);
    }

    // =========================================================
    // 시뮬레이션 루프 — BLE Notify 효과
    // =========================================================
    private IEnumerator SimulateBLELoop()
    {
        Debug.Log("[BLEManager] Simulation mode enabled.");

        while (true)
        {
            fakeDistance += fakeSpeed / 3.6f * simulateUpdateRate;

            var res = new BLEResponse
            {
                speed = fakeSpeed,
                incline = fakeIncline,
                distance = fakeDistance,
                emergencyStop = false
            };

            OnResponseReceived?.Invoke(res);
            yield return new WaitForSeconds(simulateUpdateRate);
        }
    }

    // =========================================================
    // 시뮬레이션 명령 처리
    // =========================================================
    private void HandleSimulatedCommand(BLECommand cmd)
    {
        switch (cmd.cmd)
        {
            case "setSpeed":
                if (speedRampRoutine != null) StopCoroutine(speedRampRoutine);
                speedRampRoutine = StartCoroutine(SimulateSpeedRamp(cmd.value));
                break;

            case "setIncline":
                if (inclineRampRoutine != null) StopCoroutine(inclineRampRoutine);
                inclineRampRoutine = StartCoroutine(SimulateInclineRamp(cmd.value));
                break;

            case "stop":
                fakeSpeed = 0f;
                break;
        }
    }

    // =========================================================
    // Speed Ramp (Simulation)
    // =========================================================
    private IEnumerator SimulateSpeedRamp(float target)
    {
        float start = fakeSpeed;
        float t = 0f;
        float time = 1.2f; // 속도 반응 시간

        while (t < 1f)
        {
            t += Time.deltaTime / time;
            fakeSpeed = Mathf.Lerp(start, target, t);
            yield return null;
        }

        fakeSpeed = target;
    }

    // =========================================================
    // Incline Ramp (Simulation)
    // =========================================================
    private IEnumerator SimulateInclineRamp(float target)
    {
        float start = fakeIncline;
        float t = 0f;
        float time = 1.8f; // 경사 반응 시간

        while (t < 1f)
        {
            t += Time.deltaTime / time;
            fakeIncline = Mathf.Lerp(start, target, t);
            yield return null;
        }

        fakeIncline = target;
    }
}
