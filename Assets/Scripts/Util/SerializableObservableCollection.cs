using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// ObservableCollections are used to drive UI updates (for ex. adding a crew member adds to your crew roster and auto-dispatches a model changed event which the list view listens to)
// but they're not serializable, so this adds an internal serialized list that's written to on serialize and read from on deserialize
// this way our save data structures can stay simple. this seems simpler to me than making our serialized data structures ONLY be used for serialization, 
// but keeping in memory state and saved state separate is another option, just leads to some duplication of fields

[Serializable]
public class SerializableObservableCollection<T> : SerializableObservableCollection<T, T>
{
	public SerializableObservableCollection() : base(t => t, t => t) { }
}

[Serializable]
public class SerializableObservableCollection<T, TSerialized> : ObservableCollection<T>, ISerializationCallbackReceiver
{
	[SerializeField] List<TSerialized> _list = new List<TSerialized>();

	bool _silent = false;
	Func<T, TSerialized> _serializer;
	Func<TSerialized, T> _deserializer;

	public SerializableObservableCollection(Func<T, TSerialized> serialize, Func<TSerialized, T> deserialize) {
		_serializer = serialize;
		_deserializer = deserialize;
	}

	protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e) {
		if (!_silent) {
			base.OnCollectionChanged(e);
		}
	}

	public void OnBeforeSerialize() {
		_list = this.Select(e => _serializer(e)).ToList();
	}

	// silently modify the collection on deserialize. deserialize only happens in practice when saving and loading, so no UIs should be open.
	// this is needed because you may have listeners watching that run unity api functions in response to the event, and this happens in the constructor thread
	public void OnAfterDeserialize() {
		_silent = true;
		this.Clear();
		foreach (var e in _list) {
			this.Add(_deserializer(e));
		}
		_silent = false;
	}
}