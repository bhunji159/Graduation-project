namespace DuckRunning.Events
{
    public interface IEventBehaviour
    {
        /// <summary>이벤트 정의 데이터를 전달받아 초기화합니다.</summary>
        void Initialize(EventDefinition definition);

        /// <summary>이벤트 시작 시 호출됩니다.</summary>
        void OnStart();

        /// <summary>매 프레임 업데이트됩니다. (진행 시간 elapsed 전달)</summary>
        void OnUpdate(float elapsed);

        /// <summary>이벤트가 종료될 때 호출됩니다. success = 성공 여부</summary>
        void OnEnd(bool success);
    }
}
