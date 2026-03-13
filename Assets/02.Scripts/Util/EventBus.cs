using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<GameEventType, Action> _events = new Dictionary<GameEventType, Action>();
    private static readonly Dictionary<GameEventType, Action<object>> _parameterizedEvents = new Dictionary<GameEventType, Action<object>>();

    #region Standard Events
    public static void Subscribe(GameEventType eventType, Action listener)
    {
        if (!_events.ContainsKey(eventType))
        {
            _events[eventType] = null;
        }
        _events[eventType] += listener;
    }

    public static void Unsubscribe(GameEventType eventType, Action listener)
    {
        if (_events.ContainsKey(eventType))
        {
            _events[eventType] -= listener;
        }
    }

    public static void Publish(GameEventType eventType)
    {
        if (_events.TryGetValue(eventType, out Action action))
        {
            action?.Invoke();
        }
    }
    #endregion

    #region Parameterized Events
    public static void Subscribe(GameEventType eventType, Action<object> listener)
    {
        if (!_parameterizedEvents.ContainsKey(eventType))
        {
            _parameterizedEvents[eventType] = null;
        }
        _parameterizedEvents[eventType] += listener;
    }

    public static void Unsubscribe(GameEventType eventType, Action<object> listener)
    {
        if (_parameterizedEvents.ContainsKey(eventType))
        {
            _parameterizedEvents[eventType] -= listener;
        }
    }

    public static void Publish(GameEventType eventType, object param)
    {
        if (_parameterizedEvents.TryGetValue(eventType, out Action<object> action))
        {
            action?.Invoke(param);
        }
    }
    #endregion

    /// <summary>
    /// Clears all listeners. Use this when switching scenes if necessary.
    /// </summary>
    public static void Clear()
    {
        _events.Clear();
        _parameterizedEvents.Clear();
    }
}
