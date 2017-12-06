using System;
using System.Collections.Generic;
using System.Text;


public class EventManager : IDisposable
{
    private Dictionary<uint, Action<string, Response>> callBackMap;
    private Dictionary<ERequestTypes, List<Action<ICommand>>> eventMap;

	public EventManager()
	{
        this.callBackMap = new Dictionary<uint, Action<string, Response>>();
        this.eventMap = new Dictionary<ERequestTypes, List<Action<ICommand>>>();
	}

	//Adds callback to callBackMap by id.
    public void AddCallBack(uint id, Action<string, Response> callback)
	{
		if (id > 0 && callback != null) {
			this.callBackMap.Add(id, callback);
		}
	}

	/// <summary>
	/// Invoke the callback when the server return messge .
	/// </summary>
	/// <param name='pomeloMessage'>
	/// Pomelo message.
	/// </param>
    public void InvokeCallBack(uint id, Response data)
	{
        InvokeCallBack(id, data.error, data);
	}

    public void InvokeCallBack(uint id, string err, Response data)
    {
        if (!callBackMap.ContainsKey(id)) return;

        callBackMap[id].Invoke(err, data);
        callBackMap.Remove(id);
    }

    public void AddOnEvent<T>(Action<T> handler) where T : ICommand
    {
        ERequestTypes eventId = (ERequestTypes)Enum.Parse(typeof(ERequestTypes), "E" + typeof(T).Name);
        AddOnEvent(eventId, delegate(ICommand cmd) { handler((T)cmd); });
    }

	//Adds the event to eventMap by name.
    public void AddOnEvent(ERequestTypes eventName, Action<ICommand> callback)
	{
		List<Action<ICommand>> list = null;
		if (this.eventMap.TryGetValue(eventName, out list)) {
			list.Add(callback);
		} else {
			list = new List<Action<ICommand>>();
			list.Add(callback);
			this.eventMap.Add(eventName, list);
		}
	}

	/// <summary>
	/// If the event exists,invoke the event when server return messge.
	/// </summary>
	/// <param name="eventName"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	///
    public void InvokeOnEvent(ERequestTypes id, ICommand msg)
    {
        if (!this.eventMap.ContainsKey(id)) return;

        List<Action<ICommand>> list = eventMap[id];
		foreach(Action<ICommand> action in list) action.Invoke(msg);
	}

	// Dispose() calls Dispose(true)
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	// The bulk of the clean-up code is implemented in Dispose(bool)
	protected void Dispose(bool disposing)
	{
		this.callBackMap.Clear();
		this.eventMap.Clear();
	}
}
