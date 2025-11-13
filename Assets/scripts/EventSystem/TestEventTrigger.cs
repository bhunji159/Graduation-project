using UnityEngine;
using DuckRunning.Events;
using UnityEngine.InputSystem; // ✅ 새 Input System 사용

public class TestEventTrigger : MonoBehaviour
{
    public EventDefinition testEvent;

    void Update()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            EventManager.Instance.RunEvent(testEvent);
        }
    }
}
