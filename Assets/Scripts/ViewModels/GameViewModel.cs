using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameViewModel : Model
{
	public enum Difficulty
	{
		Normal,
		Beginner
	}

	GameVars GameVars => Globals.GameVars;
	GameSession Session => Globals.Session;
	Notifications Notifications => Globals.Notifications;
	MainState MainState => Globals.MainState;

	// happens once crew is selected and we're putting the player in game
	public void GUI_startMainGame() {
		GameVars.camera_titleScreen.SetActive(false);

		//Turn on the environment fog
		RenderSettings.fog = true;

		//Now turn on the main player controls camera
		GameVars.FPVCamera.SetActive(true);

		//Turn on the player distance fog wall
		Session.playerShipVariables.fogWall.SetActive(true);

		//Now change titleScreen to false
		MainState.isTitleScreen = false;
		MainState.isStartScreen = false;

		//Now enable the controls
		Session.controlsLocked = false;

		//Initiate the main questline
		Globals.Quests.InitiateMainQuestLineForPlayer();

		//Reset Start Game Button
		GameVars.startGameButton_isPressed = false;

		// TODO: Crew select disabled for now
		//title_crew_select.SetActive(false);

		//Turn on the ship HUD
		Globals.UI.Show<Dashboard, DashboardViewModel>(new DashboardViewModel());
	}

	// TODO: This flow skips StartMainGame, so does some of the same stuff. Should merge if we can.
	public void GUI_loadGame(Difficulty difficulty) {
		if (GameVars.LoadSavedGame()) {
			MainState.isLoadedGame = true;
			MainState.isTitleScreen = false;
			MainState.isStartScreen = false;

			Globals.UI.Hide<TitleScreen>();

			GameVars.camera_titleScreen.SetActive(false);



			//Turn on the environment fog
			RenderSettings.fog = true;
			//Now turn on the main player controls camera
			GameVars.FPVCamera.SetActive(true);
			//Turn on the player distance fog wall
			Session.playerShipVariables.fogWall.SetActive(true);
			//Now enable the controls
			Session.controlsLocked = false;
			//Set the player's initial position to the new position
			Session.playerShipVariables.lastPlayerShipPosition = Session.playerShip.transform.position;
			//Update Ghost Route
			GameVars.LoadSavedGhostRoute();


			//Setup Difficulty Level
			if (difficulty == Difficulty.Normal) Session.gameDifficulty_Beginner = false;
			else Session.gameDifficulty_Beginner = true;
			GameVars.SetupBeginnerGameDifficulty();

			//Turn on the ship HUD
			Globals.UI.Show<Dashboard, DashboardViewModel>(new DashboardViewModel());

			Session.controlsLocked = false;
			//Flag the main GUI scripts to turn on
			MainState.runningMainGameGUI = true;
		}
	}

	// happens upon clicking the new game button on the title screen
	public void GUI_startNewGame(Difficulty difficulty) {
		MainState.isTitleScreen = false;
		MainState.isStartScreen = true;

		Globals.UI.Hide<TitleScreen>();

		GameVars.FillNewGameCrewRosterAvailability();

		if (difficulty == Difficulty.Normal) Session.gameDifficulty_Beginner = false;
		else Session.gameDifficulty_Beginner = true;
		GameVars.SetupBeginnerGameDifficulty();

		// since we're skipping crew select, force pick the first StartingCrewSize members
		for (var i = 0; i < Ship.StartingCrewSize; i++) {
			GameVars.newGameCrewSelectList[i] = true;
		}

		// TODO: For now, skip straight to starting the game since i turned off crew selection
		GUI_startMainGame();
		//play the intro for the game
		//time_Line_controll.play();//start the intro seen
		// TODO: Turned off crew selection because it's too overwhelming. Needs to be reworked.
		//title_crew_select.SetActive(true);
		//GUI_SetupStartScreenCrewSelection();

	}

	//-----------------------------------------------------
	//THIS IS THE SAVE DATA BUTTON
	public void GUI_saveGame() {
		Notifications.ShowANotificationMessage("Saved Data File 'player_save_game.txt' To: " + Application.persistentDataPath + "/");
		GameVars.SaveUserGameData();
	}

	//-----------------------------------------------------
	//THIS IS THE RESTART GAME BUTTON	
	public void GUI_restartGame() {
		GameVars.RestartGame();
	}
}
