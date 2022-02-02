using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Quizzes
{
	abstract class Quiz
	{
		protected Game Game => Globals.Game;
		protected UISystem UI => Globals.UI;
		protected QuestSystem Quests => Globals.Quests;
		protected GameSession Session => Globals.Game.Session;

		private Action CompleteCallback;

		public abstract string Name { get; }

		protected abstract string Title { get; }
		protected abstract string Image { get; }
		protected abstract string Caption { get; }		// TODO: The caption should be pulled from the quest data instead, but since we're hardcoding the image too this will do for now.

		public virtual void Start(Action onComplete) {
			Debug.Log("Started Quiz: " + Name);

			// reset state
			CompleteCallback = onComplete;
		}

		protected void HideAnyScreens() {
			UI.Hide<QuizScreen>();
			UI.Hide<QuestScreen>();
		}

		protected void Question(string message, ButtonViewModel[] options) {
			HideAnyScreens();

			UI.Show<QuizScreen, QuizScreenModel>(new QuizScreenModel(
				title: Title,
				message: message,
				caption: Caption,
				icon: Resources.Load<Sprite>(Image),
				choices: new ObservableCollection<ButtonViewModel>(options)
			));
		}

		protected void Message(string message, Action callback) {
			HideAnyScreens();

			UI.Show<QuestScreen, QuizScreenModel>(new QuizScreenModel(
				title: Title,
				message: message,
				caption: Caption,
				icon: Resources.Load<Sprite>(Image),
				choices: new ObservableCollection<ButtonViewModel> {
					new ButtonViewModel { Label = "OK", OnClick = callback }
				}
			));
		}

		protected void GameOver() {
			Debug.Log("Failed Quiz: " + Name);

			HideAnyScreens();
			Game.isGameOver = true;
		}

		protected void Complete() {
			Debug.Log("Completed Quiz: " + Name);

			HideAnyScreens();
			CompleteCallback?.Invoke();
		}
	}

	public static class QuizSystem
	{
		static Dictionary<string, Quiz> _quizzes = new Dictionary<string, Quiz>();

		static void Add(Quiz quiz) {
			_quizzes.Add(quiz.Name, quiz);
		}

		static QuizSystem() {
			Add(new ClashingRocksQuiz());
			Add(new GoldenFleeceQuiz());
		}

		static Quiz GetQuiz(string name) => _quizzes.ContainsKey(name) ? _quizzes[name] : null;

		public static void StartQuiz(string name, Action onComplete) {
			var quiz = GetQuiz(name);
			if (quiz != null) {
				quiz.Start(onComplete);
			}
		}
	}
}
