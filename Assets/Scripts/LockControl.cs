using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class CutsceneMode
{
	static IEnumerable<ViewBehaviour> _restoreViews;

	public static void Enter() {
		Globals.Game.Session.IsCutsceneMode = true;
		_restoreViews = Globals.UI.GetActiveViews();
		Globals.UI.HideAll();
	}

	public static void Exit() {
		Globals.Game.Session.IsCutsceneMode = false;
		foreach(var view in _restoreViews) {
			Globals.UI.Show(view);
		}
	}
}
