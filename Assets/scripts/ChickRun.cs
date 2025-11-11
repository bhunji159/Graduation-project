using UnityEngine;

public class ChickRun : MonoBehaviour
{
    [Header("Body Parts")]
    public Transform body;          // 몸통 전체 (눈, 부리, 눈썹 포함된 부모)
    public Transform leftLeg;
    public Transform rightLeg;
    public Transform leftWing;
    public Transform rightWing;

    [Header("Animation Settings")]
    public float runSpeed = 5f;     // 애니메이션 속도 (초당 싸이클)
    public float legSwing = 25f;    // 다리 회전 각도
    public float wingSwing = 20f;   // 날개 회전 각도
    public float bounceHeight = 0.05f; // 몸통 들썩 높이

    private Vector3 bodyBasePos;
    private Vector3 leftLegBaseRot;
    private Vector3 rightLegBaseRot;
    private Vector3 leftWingBaseRot;
    private Vector3 rightWingBaseRot;

    void Start()
    {
        // 초기값 저장
        bodyBasePos = body.localPosition;
        leftLegBaseRot = leftLeg.localEulerAngles;
        rightLegBaseRot = rightLeg.localEulerAngles;
        leftWingBaseRot = leftWing.localEulerAngles;
        rightWingBaseRot = rightWing.localEulerAngles;
    }

    void Update()
    {
        float t = Time.time * runSpeed;
        float sin = Mathf.Sin(t);
        float cos = Mathf.Cos(t);

        // --- 다리 ---
        // 왼다리 앞으로, 오른다리 뒤로 (반대 위상)
        leftLeg.localEulerAngles = leftLegBaseRot + new Vector3(legSwing * sin, 0, 0);
        rightLeg.localEulerAngles = rightLegBaseRot + new Vector3(-legSwing * sin, 0, 0);

        // --- 날개 ---
        // 오른날개는 왼다리와 같은 위상, 왼날개는 반대 위상
        rightWing.localEulerAngles = rightWingBaseRot + new Vector3(wingSwing * sin, 0, 0);
        leftWing.localEulerAngles = leftWingBaseRot + new Vector3(-wingSwing * sin, 0, 0);

        // --- 몸통 (들썩) ---
        float bounce = Mathf.Abs(sin) * bounceHeight;
        body.localPosition = bodyBasePos + new Vector3(0, bounce, 0);
    }
}
