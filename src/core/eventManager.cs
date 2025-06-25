using System;
using System.Collections.Generic;
using System.Linq;
namespace TDgame;

public static class EventManager
{
    private static Dictionary<string, List<Action<object[]>>> _events =
        new Dictionary<string, List<Action<object[]>>>();

    public static void Subscribe(string eventName, Action<Object[]> handler)
    {
        if (_events[eventName] == null)
        {
            _events[eventName] = new List<Action<object[]>>();
        }

        // only add if it doesn't exist
        if (!_events[eventName].Contains(handler))
        {
            _events[eventName].Add(handler);
        }
    }

    public static void Call(string eventName, params object[] args)
    {
        List<Action<object[]>> handlersToCall = _events[eventName].ToList();
        if (_events.ContainsKey(eventName))
        {
            foreach (var handler in handlersToCall)
            {
                handler.Invoke(args);
            }
        }
    }
}