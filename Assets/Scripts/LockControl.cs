using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class CutsceneMode
{
	static GameSession Session => Globals.Game.Session;
	static UISystem UI => Globals.UI;

	static IEnumerable<ViewBehaviour> _restoreViews;

	public static void Enter() {
		Session.IsCutsceneMode = true;
		_restoreViews = UI.GetActiveViews();
		UI.HideAll();
	}

	public static void Exit() {
		Session.IsCutsceneMode = false;
		foreach(var view in _restoreViews) {
			UI.Show(view);
		}
	}
}
