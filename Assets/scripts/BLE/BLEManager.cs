using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// DuckRunning BLEManager — ESP32와 BLE 통신 총괄
/// (현재 버전은 BLE 시뮬레이션 가능 구조)
/// </summary>
public class BLEManager : MonoBehaviour
{
    public static BLEManager Instance { get; private set; }

    [Header("BLE Config")]
    public bool simulateBLE = true;         // 🔧 true면 실제 연결 없이 Unity 내부 시뮬레이션
    public float simulateUpdateRate = 0.2f; // 초당 BLE 응답 빈도
    public float fakeSpeed = 0f;            // 시뮬레이션용 현재 속도
    public float fakeDistance = 0f;

    public event Action<BLEResponse> OnResponseReceived;

    private Queue<BLECommand> commandQueue = new Queue<BLECommand>();
    private bool isConnected = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
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
            // TODO: 실제 BLE SDK 연결 코드
        }
    }

    /// <summary>
    /// BLE 명령 전송 (setSpeed, setIncline, stop 등)
    /// </summary>
    public void SendCommand(BLECommand cmd)
    {
        if (!isConnected)
        {
            Debug.LogWarning("[BLEManager] Not connected!");
            return;
        }

        commandQueue.Enqueue(cmd);
        Debug.Log($"[BLEManager] Queued command → {cmd.cmd}({cmd.value})");

        if (!simulateBLE)
        {
            // TODO: BLE SDK를 통해 ESP32로 전송 (writeCharacteristic)
        }
    }

    /// <summary>
    /// 실제 BLE 장치로부터 Notify 수신 처리
    /// </summary>
    public void HandleBLEMessage(string json)
    {
        BLEResponse response = BLEResponse.FromJson(json);
        OnResponseReceived?.Invoke(response);
    }

    /// <summary>
    /// 시뮬레이션용 BLE 루프 — 실제 BLE 응답처럼 동작
    /// </summary>
    IEnumerator SimulateBLELoop()
    {
        Debug.Log("[BLEManager] BLE Simulation mode active.");

        while (true)
        {
            if (commandQueue.Count > 0)
            {
                BLECommand cmd = commandQueue.Dequeue();

                switch (cmd.cmd)
                {
                    case "setSpeed":
                        // 천천히 목표속도로 보간
                        StartCoroutine(SimulateSpeedRamp(cmd.value));
                        break;
                    case "stop":
                        fakeSpeed = 0;
                        break;
                }
            }

            fakeDistance += fakeSpeed / 3.6f * simulateUpdateRate;

            BLEResponse res = new BLEResponse
            {
                speed = fakeSpeed,
                distance = fakeDistance,
                incline = 0f,
                emergencyStop = false
            };

            OnResponseReceived?.Invoke(res);
            yield return new WaitForSeconds(simulateUpdateRate);
        }
    }

    IEnumerator SimulateSpeedRamp(float targetSpeed)
    {
        float rampTime = 2f; // 2초 동안 서서히 변경
        float startSpeed = fakeSpeed;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / rampTime;
            fakeSpeed = Mathf.Lerp(startSpeed, targetSpeed, t);
            yield return null;
        }

        fakeSpeed = targetSpeed;
    }
}
