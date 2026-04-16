using System;
using System.Collections.Generic;

/// <summary>
/// 매개변수가 없는 표준 이벤트를 처리하는 정적 이벤트 버스입니다.
/// </summary>
public static class EventBus
{
    private static readonly Dictionary<GameEventType, Action> _events = new Dictionary<GameEventType, Action>();

    /// <summary>
    /// 이벤트 구독
    /// </summary>
    public static void Subscribe(GameEventType eventType, Action listener)
    {
        if (!_events.ContainsKey(eventType)) _events[eventType] = null;
        _events[eventType] += listener;
    }

    /// <summary>
    /// 이벤트 구독 해제
    /// </summary>
    public static void Unsubscribe(GameEventType eventType, Action listener)
    {
        if (_events.ContainsKey(eventType)) _events[eventType] -= listener;
    }

    /// <summary>
    /// 이벤트 발생 (구독된 모든 리스너 실행)
    /// </summary>
    public static void Publish(GameEventType eventType)
    {
        if (_events.TryGetValue(eventType, out Action action))
        {
            action?.Invoke();
        }
    }

    /// <summary>
    /// 모든 리스너를 제거합니다. 씬 전환 시 유용합니다.
    /// </summary>
    public static void Clear()
    {
        _events.Clear();
        // 모든 제네릭 이벤트 버스도 함께 클리어
        GenericEventBus.ClearAll();
    }
}

/// <summary>
/// 매개변수가 있는 이벤트를 위한 제네릭 이벤트 버스입니다. (타입 안정성 제공)
/// </summary>
public static class EventBus<T>
{
    private static readonly Dictionary<GameEventType, Action<T>> _events = new Dictionary<GameEventType, Action<T>>();

    /// <summary>
    /// 특정 타입의 매개변수를 가진 이벤트 구독
    /// </summary>
    public static void Subscribe(GameEventType eventType, Action<T> listener)
    {
        if (!_events.ContainsKey(eventType))
        {
            _events[eventType] = null;
            GenericEventBus.Register(eventType, Clear);
        }
        _events[eventType] += listener;
    }

    /// <summary>
    /// 이벤트 구독 해제
    /// </summary>
    public static void Unsubscribe(GameEventType eventType, Action<T> listener)
    {
        if (_events.ContainsKey(eventType)) _events[eventType] -= listener;
    }

    /// <summary>
    /// 특정 타입의 데이터를 포함하여 이벤트 발생
    /// </summary>
    public static void Publish(GameEventType eventType, T param)
    {
        if (_events.TryGetValue(eventType, out Action<T> action))
        {
            action?.Invoke(param);
        }
    }

    /// <summary>
    /// 해당 타입의 모든 리스너를 제거합니다.
    /// </summary>
    public static void Clear()
    {
        _events.Clear();
    }
}

/// <summary>
/// 모든 제네릭 이벤트 버스들을 관리하기 위한 내부 헬퍼 클래스입니다.
/// </summary>
internal static class GenericEventBus
{
    private static readonly Dictionary<GameEventType, List<Action>> _clearActions = new Dictionary<GameEventType, List<Action>>();

    public static void Register(GameEventType type, Action clearAction)
    {
        if (!_clearActions.ContainsKey(type)) _clearActions[type] = new List<Action>();
        _clearActions[type].Add(clearAction);
    }

    public static void ClearAll()
    {
        foreach (var list in _clearActions.Values)
        {
            foreach (var clear in list) clear?.Invoke();
        }
    }
}
