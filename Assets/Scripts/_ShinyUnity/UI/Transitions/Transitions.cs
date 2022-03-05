// The MIT License (MIT)
// 
// Copyright (c) 2018 Shiny Dolphin Games LLC
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public interface ITransitionIn
{
	void TransitionIn();
}
public interface ITransitionOut
{
	void TransitionOut(Action done);
}

public static class Transitions
{
	public static void SlideAndFadeIn(CanvasGroup group, Vector3 fromPos, Vector3 toPos, float duration) {
		SlideIn(group, fromPos, toPos, duration);
		FadeInCanvasGroup(group, duration);
	}

	public static void SlideAndFadeOut(CanvasGroup group, Action done, Vector3 fromPos, Vector3 toPos, float duration) {
		FadeOutCanvasGroup(group, done, duration);
		SlideOut(group, () => { }, fromPos, toPos, duration);
	}

	public static void FadeInCanvasGroup(CanvasGroup group, float duration) {
		if (group == null) {
			return;
		}

		group.interactable = false;
		group.alpha = 0;

		DOTween.To(() => group.alpha, val => group.alpha = val, 1, duration)
		  .SetUpdate(true)
		  .OnComplete(() => group.interactable = true);
	}

	public static void FadeOutCanvasGroup(CanvasGroup group, Action done, float duration) {
		if (group == null) {
			done();
			return;
		}

		group.interactable = false;

		DOTween.To(() => group.alpha, val => group.alpha = val, 0, duration)
		  .SetUpdate(true)
		  .OnComplete(() => done());
	}

	public static void SlideIn(CanvasGroup group, Vector3 fromPos, Vector3 toPos, float duration) {
		if (group == null) {
			return;
		}

		group.alpha = 1;
		group.interactable = false;
		group.transform.localPosition = fromPos;

		DOTween.To(() => group.transform.localPosition, val => group.transform.localPosition = val, toPos, duration)
		  .SetUpdate(true)
		  .OnComplete(() => group.interactable = true);
	}

	public static void SlideOut(CanvasGroup group, Action done, Vector3 fromPos, Vector3 toPos, float duration) {
		if (group == null) {
			done();
			return;
		}

		group.interactable = false;

		var groupTransform = group.GetComponent<RectTransform>();

		DOTween.To(() => groupTransform != null ? groupTransform.localPosition : Vector3.zero, val => {
			if (groupTransform != null) {
				groupTransform.localPosition = val;
			}
		}, toPos, duration)
		  .SetUpdate(true)
		  .OnComplete(() => {
			  if (group != null && groupTransform != null) {
				  group.alpha = 0;
				  groupTransform.localPosition = fromPos;
			  }

			  done();
		  });
	}
}
