/*
The MIT License(MIT)
Copyright(c) 2016 Salty Dog Digital, LLC

Permission is hereby granted, free of charge, to any person obtaining
a copy of this software and associated documentation files (the
"Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish,
distribute, sublicense, and/or sell copies of the Software, and to
permit persons to whom the Software is furnished to do so, subject to
the following conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Collections.Generic;
using UnityEngine;

public class GameEvent { }
public interface IGenericGameEvent { string EventName { get; } }
public class GenericGameEvent : GameEvent, IGenericGameEvent
{
	public string EventName { get; set; }
}

// automatic static instances on events that happen often to reduce allocations
public static class EventCache<T> where T : GameEvent, new()
{
	public static T Instance { get; private set; } = new T();
	public static void Raise() => Events.Instance.Raise(Instance);
}

/// <summary>
/// Event Manager manages publishing raised events to subscribing/listening classes.
///
/// @example subscribe
///     EventManager.Instance.AddListener<SomethingHappenedEvent>(OnSomethingHappened);
///
/// @example unsubscribe
///     EventManager.Instance.RemoveListener<SomethingHappenedEvent>(OnSomethingHappened);
///
/// @example publish an event
///     EventManager.Instance.Raise(new SomethingHappenedEvent());
///
/// This class is a minor variation on <http://www.willrmiller.com/?p=87>
/// </summary>
public class Events
{

	public static Events Instance {
		get {
			if (instance == null) {
				instance = new Events();
			}

			return instance;
		}
	}
	private static Events instance = null;

	public delegate void EventDelegate<T>(T e) where T : GameEvent;
	public delegate void EventDelegate(GameEvent e);

	/// <summary>
	/// The actual delegate, there is one delegate per unique event. Each
	/// delegate has multiple invocation list items.
	/// </summary>
	internal Dictionary<System.Type, EventDelegate> delegates = new Dictionary<System.Type, EventDelegate>();

	/// <summary>
	/// Lookups only, there is one delegate lookup per listener
	/// </summary>
	internal Dictionary<System.Delegate, EventDelegate> delegateLookup = new Dictionary<System.Delegate, EventDelegate>();

#if SHINY_EVENT_DEBUG
    internal class EventDebugInfo
    {
      public System.Type EventType;
      public System.Delegate Delegate;
      public System.Diagnostics.StackTrace StackTrace;
    }

    internal Dictionary<System.Delegate, EventDebugInfo> debugLookup = new Dictionary<System.Delegate, EventDebugInfo>();

    internal event System.Action<EventDebugInfo, GameEvent> EventRaised;
    internal void ResetEventRaisedListeners() => EventRaised = null;
#endif

	/// <summary>
	/// Add the delegate.
	/// </summary>
	public void AddListener<T>(EventDelegate<T> del) where T : GameEvent {
		if (delegateLookup.ContainsKey(del)) {
#if SHINY_EVENT_DEBUG
        var debug = debugLookup.ContainsKey(del) ? debugLookup[del] : null;
        Debug.Log("Skipping AddListener because the delegate has already been added: " + del + " Stack: " + debug?.StackTrace);
#else
			Debug.Log("Skipping AddListener because the delegate has already been added: " + del);
#endif
			return;
		}

		// Create a new non-generic delegate which calls our generic one.  This
		// is the delegate we actually invoke.
		EventDelegate internalDelegate = (e) => del((T)e);
		delegateLookup[del] = internalDelegate;

		AddListener(typeof(T), internalDelegate);
	}

	/// <summary>
	/// Remove the delegate. Can be called multiple times on same delegate.
	/// </summary>
	public void RemoveListener<T>(EventDelegate<T> del) where T : GameEvent {
		EventDelegate internalDelegate;
		if (delegateLookup.TryGetValue(del, out internalDelegate)) {
			RemoveListener(typeof(T), internalDelegate);

			// must be removed here, not in RemoveListener, because this dict contains the external delegate, notthe internalDelegate
			delegateLookup.Remove(del);
		}
	}

	public void AddListener(System.Type t, EventDelegate del) {
		EventDelegate tempDel;
		if (delegates.TryGetValue(t, out tempDel)) {
			delegates[t] = tempDel += del;
		}
		else {
			delegates[t] = del;
		}

#if SHINY_EVENT_DEBUG
      debugLookup[del] = new EventDebugInfo { StackTrace = new System.Diagnostics.StackTrace(2, true), EventType = t, Delegate = del };
      Debug.Log("Added listener: " + debugLookup[del].StackTrace.ToString());
#endif
	}

	public void RemoveListener(System.Type t, EventDelegate del) {
		EventDelegate tempDel;
		if (delegates.TryGetValue(t, out tempDel)) {
			tempDel -= del;
			if (tempDel == null) {
				delegates.Remove(t);
			}
			else {
				delegates[t] = tempDel;
			}
		}

#if SHINY_EVENT_DEBUG
      if(delegateLookup.ContainsKey(del))
      {
        Debug.Log("Removed listener: " + debugLookup[delegateLookup[del]].StackTrace.ToString());
        debugLookup.Remove(delegateLookup[del]);
      }
      else if(debugLookup.ContainsKey(del))
      {
        debugLookup.Remove(del);
      }
#endif

		if (delegateLookup.ContainsKey(del)) {
			delegateLookup.Remove(del);
		}
	}

	/// <summary>
	/// The count of delegate lookups. The delegate lookups will increase by
	/// one for each unique AddListener. Useful for debugging and not much else.
	/// </summary>
	public int DelegateLookupCount { get { return delegateLookup.Count; } }

	/// <summary>
	/// Raise the event to all the listeners
	/// </summary>
	public void Raise(GameEvent e) {
		EventDelegate del;
		if (delegates.TryGetValue(e.GetType(), out del)) {
			del.Invoke(e);

#if SHINY_EVENT_DEBUG && SHINY_EVENT_HIT_DEBUG
        foreach(var entry in del.GetInvocationList())
        {
          var debugInfo = debugLookup[entry];
          EventRaised?.Invoke(debugInfo, e);
        }
#endif
		}
	}

}
