using System;
using System.Collections.Generic;

/// <summary>
/// Global, type-safe event bus for decoupled communication between systems.
/// Usage:
///   EventBus.Subscribe&lt;MyEvent&gt;(OnMyEvent);
///   EventBus.Publish(new MyEvent());
///   EventBus.Unsubscribe&lt;MyEvent&gt;(OnMyEvent);
/// </summary>
public static class EventBus
{
    private static readonly Dictionary<Type, List<Delegate>> _handlers = new();

    public static void Subscribe<T>(Action<T> handler)
    {
        Type type = typeof(T);
        if (!_handlers.ContainsKey(type))
            _handlers[type] = new List<Delegate>();

        _handlers[type].Add(handler);
    }

    public static void Unsubscribe<T>(Action<T> handler)
    {
        Type type = typeof(T);
        if (_handlers.TryGetValue(type, out List<Delegate> list))
            list.Remove(handler);
    }

    public static void Publish<T>(T evt)
    {
        Type type = typeof(T);
        if (!_handlers.TryGetValue(type, out List<Delegate> list)) return;

        // Iterate over a copy to allow safe unsubscription during dispatch
        foreach (Delegate handler in list.ToArray())
            (handler as Action<T>)?.Invoke(evt);
    }

    public static void Clear<T>()
    {
        _handlers.Remove(typeof(T));
    }

    public static void ClearAll()
    {
        _handlers.Clear();
    }
}
