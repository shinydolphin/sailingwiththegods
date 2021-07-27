using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameViewModel : Model
{
	World World => Globals.World;
	GameSession Session => Globals.Game.Session;
	Notifications Notifications => Globals.Notifications;
	Game Game => Globals.Game;

	// TODO: This flow skips StartMainGame, so does some of the same stuff. Should merge if we can.
	public void GUI_loadGame(Game.Difficulty difficulty) {
		Game.LoadGame(difficulty);
	}

	// happens upon clicking the new game button on the title screen
	public void GUI_startNewGame(Game.Difficulty difficulty) {
		Game.StartNewGame(difficulty);
	}

	//-----------------------------------------------------
	//THIS IS THE SAVE DATA BUTTON
	public void GUI_saveGame() {
		Notifications.ShowANotificationMessage("Saved Data File 'player_save_game.txt' To: " + Application.persistentDataPath + "/");
		Game.SaveUserGameData();
	}

	//-----------------------------------------------------
	//THIS IS THE RESTART GAME BUTTON	
	public void GUI_restartGame() {
		Game.RestartGame();
	}
}
