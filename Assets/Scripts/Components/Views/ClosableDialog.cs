using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClosableDialog : OwnerBehaviour
{
	UISystem UI => Globals.UI;

	[SerializeField] Button Button = null;

	public Action Callback;

	private void Start() {
		Subscribe(Button.onClick, () => {
			Callback?.Invoke();
			UI.Hide(GetComponent<ViewBehaviour>());
		});
	}
}
