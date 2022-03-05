using System;
using System.ComponentModel;
using UnityEngine;

/// <summary>
/// Base class for UIs that should appear/disappear with transitions
/// </summary>
/// <typeparam name="TModel">The Model class that drives the UI</typeparam>
public class PopupView<TModel> : ViewBehaviour<TModel>, ITransitionOut, ITransitionIn 
	where TModel : INotifyPropertyChanged
{
	const float TransitionDuration = 0.1f;
	const float SlideOffset = -50;

	protected CanvasGroup Group { get; private set; }
	protected Vector3 ShowPos { get; private set; }

	virtual protected void Awake() {
		Group = GetComponentInChildren<CanvasGroup>();
		if (Group == null) {
			Debug.LogError("PopupView subclass UIs must have a CanvasGroup for transitions to work.");
			return;
		}

		ShowPos = Group.transform.localPosition;
	}

	virtual public void TransitionIn() => 
		Transitions.SlideAndFadeIn(Group, ShowPos.WithOffset(y: SlideOffset), ShowPos, TransitionDuration);

	virtual public void TransitionOut(Action done) => 
		Transitions.SlideAndFadeOut(Group, done, ShowPos, ShowPos.WithOffset(y: SlideOffset), TransitionDuration);

}
