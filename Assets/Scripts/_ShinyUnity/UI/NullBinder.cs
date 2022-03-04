using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For when you need bind called, but want a null model. Allows binding to happen automatically with no need for a class that knows the generic type.
/// </summary>
public class NullBinder : MonoBehaviour
{
	private void Start() {
		var view = GetComponent<ViewBehaviour>();
		var viewType = view.GetType();
		viewType.GetMethod("Bind")?.Invoke(view, new[] { (object)null });
	}
}

