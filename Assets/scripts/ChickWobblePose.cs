using UnityEngine;

public class ChickTiltFlapV2 : MonoBehaviour
{
    public Transform body;
    public Transform leftWing;
    public Transform rightWing; 
    public Transform rightFoot;
    [Header("Body Lift")]

    public float y = 5f;

    [Header("Body Movement")]
    public float forwardTilt = 45f;
    public float wobbleAmount = 5f;
    public float wobbleSpeed = 2f;

    [Header("Wing Flap")]
    public float flapSpeed = 15f;
    public float flapAmount = 60f;

    private Quaternion bodyBaseRot;
    private Quaternion rightFootBaseRot;
    private Quaternion leftWingBaseRot;
    private Quaternion rightWingBaseRot;

    void Start()
    {
        bodyBaseRot = body.localRotation;
        

        leftWing.position += new Vector3(-2/10f, 3/10f, +7/10f);
        rightWing.position += new Vector3(2/10f, 3/10f, 7/10f);
        body.localPosition += new Vector3(0f, y, 0);
        rightFoot.localPosition += new Vector3(0f, y, 5);
        // 초기 회전 정렬
        rightFoot.localRotation = Quaternion.Euler(-120f, 0f, 0f);
        leftWing.localRotation = Quaternion.Euler(140f, 0f, 90f);
        rightWing.localRotation = Quaternion.Euler(140f, 0f, -90f);
        rightFootBaseRot = rightFoot.localRotation;
        leftWingBaseRot = leftWing.localRotation;
        rightWingBaseRot = rightWing.localRotation;
    }

    void Update()
    {
        float wobble = Mathf.Sin(Time.time * wobbleSpeed) * wobbleAmount;
        body.localRotation = bodyBaseRot * Quaternion.Euler(-(forwardTilt + wobble), 0f, 0f);
        
        
        // 🪽 위아래로 같은 방향 펄럭
        float flap = Mathf.Sin(Time.time * flapSpeed) * flapAmount;

        leftWing.localRotation = leftWingBaseRot * Quaternion.Euler(flap * (9/26f), 0f, flap * (17/26f));
        rightWing.localRotation = rightWingBaseRot * Quaternion.Euler(flap * (9 / 26f), 0f, -flap * (17 / 26f));
        rightFoot.localRotation = rightFootBaseRot * Quaternion.Euler(flap/3, 0f, 0f);
    }
}
