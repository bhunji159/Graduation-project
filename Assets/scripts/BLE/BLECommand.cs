using System;

[Serializable]
public class BLECommand
{
    public string cmd;      // e.g. "setSpeed", "setIncline", "stop"
    public float value;     // target value (km/h, %, etc.)

    public BLECommand(string command, float val)
    {
        cmd = command;
        value = val;
    }

    public string ToJson() => UnityEngine.JsonUtility.ToJson(this);
    public override string ToString()
    {
        return $"{cmd}({value})";
    }

}
