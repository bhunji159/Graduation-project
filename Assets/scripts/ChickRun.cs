using UnityEngine;

public class ChickRun : MonoBehaviour
{
    [Header("Body Parts")]
    public Transform body;
    public Transform leftLeg;
    public Transform rightLeg;
    public Transform leftWing;
    public Transform rightWing;

    [Header("Animation Settings")]
    public float runSpeed = 5f;     
    public float legSwing = 25f;
    public float wingSwing = 20f;
    public float bounceHeight = 0.1f;
    public float idleThreshold = 0.2f;

    private Vector3 bodyBasePos;
    private Vector3 leftLegBaseRot;
    private Vector3 rightLegBaseRot;
    private Vector3 leftWingBaseRot;
    private Vector3 rightWingBaseRot;

    private float smoothAnimSpeed = 0f;
    private float animTime = 0f; // 🟡 누적 시간 (Time.time 대신)

    void Start()
    {
        bodyBasePos = body.localPosition;
        leftLegBaseRot = leftLeg.localEulerAngles;
        rightLegBaseRot = rightLeg.localEulerAngles;
        leftWingBaseRot = leftWing.localEulerAngles;
        rightWingBaseRot = rightWing.localEulerAngles;
    }

    void Update()
    {
        // 애니메이션 속도 보간
        smoothAnimSpeed = Mathf.Lerp(smoothAnimSpeed, runSpeed, Time.deltaTime * 5f);

        // 속도에 따라 누적 시간 증가
        animTime += smoothAnimSpeed * Time.deltaTime;

        if (smoothAnimSpeed < idleThreshold)
        {
            // Idle
            body.localPosition = bodyBasePos;
            leftLeg.localEulerAngles = leftLegBaseRot;
            rightLeg.localEulerAngles = rightLegBaseRot;
            leftWing.localEulerAngles = leftWingBaseRot;
            rightWing.localEulerAngles = rightWingBaseRot;
            return;
        }

        // 사인파 기반 애니메이션 (Time.time 대신 animTime 사용)
        float sin = Mathf.Sin(animTime);
        float absSin = Mathf.Abs(sin);

        leftLeg.localEulerAngles = leftLegBaseRot + new Vector3(legSwing * sin, 0, 0);
        rightLeg.localEulerAngles = rightLegBaseRot + new Vector3(-legSwing * sin, 0, 0);
        rightWing.localEulerAngles = rightWingBaseRot + new Vector3(wingSwing * sin, 0, 0);
        leftWing.localEulerAngles = leftWingBaseRot + new Vector3(-wingSwing * sin, 0, 0);
        body.localPosition = bodyBasePos + new Vector3(0, absSin * bounceHeight, 0);
    }
}
