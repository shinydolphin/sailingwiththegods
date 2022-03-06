using System.Collections;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

public class QuizScreenModel : Model
{
	private string _Title;
	public string Title { get => _Title; set { _Title = value; Notify(); } }

	private string _Message;
	public string Message { get => _Message; set { _Message = value; Notify(); } }

	private string _Caption;
	public string Caption { get => _Caption; set { _Caption = value; Notify(); } }

	private Sprite _Icon;
	public Sprite Icon { get => _Icon; set { _Icon = value; Notify(); } }

	public readonly ICollectionModel<ButtonViewModel> Choices;

	public QuizScreenModel(string title, string message, string caption, Sprite icon, ObservableCollection<ButtonViewModel> choices) {
		Title = title;
		Message = message;
		Caption = caption;
		Icon = icon;
		Choices = ValueModel.Wrap(choices);
	}
}

public class QuizScreen : PopupView<QuizScreenModel>
{
	[SerializeField] ImageView Icon = null;
	[SerializeField] StringView Title = null;
	[SerializeField] StringView Message = null;
	[SerializeField] StringView Caption = null;
	[SerializeField] ButtonView[] Choices = null;

	public override void Bind(QuizScreenModel model) {
		base.Bind(model);

		if (model == null) {
			Debug.LogWarning("Tried to bind view to a null model on " + name);
			return;
		}

		if (model.Icon != null) {
			Icon?.Bind(new BoundModel<Sprite>(model, nameof(model.Icon)));
		}

		Title?.Bind(new BoundModel<string>(Model, nameof(Model.Title)));
		Message?.Bind(new BoundModel<string>(Model, nameof(Model.Message)));
		Caption?.Bind(new BoundModel<string>(Model, nameof(Model.Caption)));

		// TODO: populate from the collection directly
		foreach (var choice in Choices) {
			choice.gameObject.SetActive(false);
		}
		for (var i = 0; i < model.Choices.Count; i++) {
			Choices[i]?.Bind(ValueModel.New(model.Choices.ElementAtOrDefault(i)));
			Choices[i].gameObject.SetActive(true);
		}
	}
}
