using System;

[Serializable]
public class BLEResponse
{
    public float speed;         // km/h
    public float incline;       // %
    public float distance;      // m
    public bool emergencyStop;  // true/false

    public static BLEResponse FromJson(string json)
        => UnityEngine.JsonUtility.FromJson<BLEResponse>(json);
}
