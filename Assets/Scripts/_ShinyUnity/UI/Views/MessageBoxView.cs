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

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBoxViewModel : Model
{
	private string _Title;
	public string Title { get => _Title; set { _Title = value; Notify(); } }

	private string _Message;
	public string Message { get => _Message; set { _Message = value; Notify(); } }

	private ButtonViewModel _OK;
	public ButtonViewModel OK { get => _OK; set { _OK = value; Notify(); } }

	private ButtonViewModel _Cancel;
	public ButtonViewModel Cancel { get => _Cancel; set { _Cancel = value; Notify(); } }
}

public class MessageBoxView : ViewBehaviour<MessageBoxViewModel>
{
	[SerializeField] StringView Title = null;
	[SerializeField] StringView Message = null;
	[SerializeField] ButtonView OK = null;
	[SerializeField] ButtonView Cancel = null;
	[SerializeField] ButtonView Close = null;

	public override void Bind(MessageBoxViewModel model) {
		base.Bind(model);

		if (model == null) {
			Debug.LogWarning("Tried to bind view to a null model on " + name);
			return;
		}

		Title?.Bind(new BoundModel<string>(Model, nameof(Model.Title)));
		Message?.Bind(new BoundModel<string>(Model, nameof(Model.Message)));

		OK?.Bind(new BoundModel<ButtonViewModel>(Model, nameof(Model.OK)));
		Cancel?.Bind(new BoundModel<ButtonViewModel>(Model, nameof(Model.Cancel)));
		Close?.Bind(new BoundModel<ButtonViewModel>(Model, nameof(Model.Cancel)));
	}
}
