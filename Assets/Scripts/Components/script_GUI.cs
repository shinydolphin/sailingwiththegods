using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

public class script_GUI : MonoBehaviour
{
	Notifications Notifications => Globals.Notifications;
	Database Database => Globals.Database;
	QuestSystem Quests => Globals.Quests;
	MiniGames MiniGames => Globals.MiniGames;
	Game Game => Globals.Game;
	UISystem UI => Globals.UI;

	//======================================================================================================================================================================
	//======================================================================================================================================================================
	//  SETUP ALL VARIABLES FOR THE GUI
	//======================================================================================================================================================================
	//======================================================================================================================================================================
	public enum Intention {Water, Trading, Tavern, All };

	//public bool useDialog = true;
	public bool useDebugDialog = false;
	public string debugDialogNode = "Start_Debug";
	public string dialogNode = "Start_Tax";

	//-----------------------------------------------------------
	// Game Over Notification Variables
	//-----------------------------------------------------------
	[Header("Game Over Notification")]
	public GameObject gameover_main;
	public GameObject gameover_message;
	public GameObject gameover_restart;

	public GameObject port_dialog;

	//===================================
	// OTHER VARS
	Game MainState => Globals.Game;
	World World => Globals.World;
	GameSession Session => Globals.Game.Session;

	private PortViewModel port;
	private TradeViewModel trade;

	//I tried using the default just get; private set; but it refused to work? So it needs to go like this
	public PortViewModel Port 
	{
		get {
			return port;
		}
		private set {
			port = value;
		}
	}

	public TradeViewModel Trade 
	{
		get {
			return trade;
		}
		private set {
			trade = value;
		}
	}

	public void ClearViewModels() {
		port = null;
		trade = null;
	}


	//======================================================================================================================================================================
	//======================================================================================================================================================================
	//  START OF THE MAIN UPDATE LOOP
	//======================================================================================================================================================================
	//vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

	void OnGUI() {
		
		//=====================================================================================================================================	
		//  IF WE ARE AT THE TITLE SCREEN OR START SCREEN
		//=====================================================================================================================================	

		if (!MainState.runningMainGameGUI) {

			//=====================================================================================================================================
			// IF WE ARE AT THE TITLE SCREEN
			if (MainState.isTitleScreen) {

				UI.Show<TitleScreen, GameViewModel>(new GameViewModel());
				MainState.isTitleScreen = false;			// TODO: Make this based on an event rather than this hacky one-time execution style.
			}

			Notifications.Pump();

			//=====================================================================================================================================	
			//  IF WE AREN'T AT THE TITLE SCREEN OR START SCREEN
			//=====================================================================================================================================	
		}
		else if (MainState.runningMainGameGUI) {

			//`````````````````````````````````````````````````````````````````
			//WE ARE SHOWING A YES / NO  PORT TAX NOTIFICATION POP UP	?
			if (Session.showPortDockingNotification) {
				Session.showPortDockingNotification = false;
				MainState.menuControlsLock = true;
				GUI_ShowPortDockingNotification();
			}
			
			Notifications.Pump();

			//`````````````````````````````````````````````````````````````````
			// GAME OVER GUI (prevent blocking a more important UI if one is up using menuControlIsLock to check)
			if (MainState.isGameOver && !MainState.menuControlsLock) {
				MainState.menuControlsLock = true;
				GUI_ShowGameOverNotification();
				MainState.isGameOver = false;
			}


			//`````````````````````````````````````````````````````````````````
			// WIN THE GAME GUI
			if (MainState.gameIsFinished) {
				MainState.menuControlsLock = true;
				GUI_ShowGameIsFinishedNotification();
				MainState.gameIsFinished = false;
			}

		}


	}//End of On.GUI()
	 //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^	
	 //======================================================================================================================================================================
	 //  END OF MAIN UPDATE LOOP
	 //======================================================================================================================================================================
	 //======================================================================================================================================================================


	//======================================================================================================================================================================
	//======================================================================================================================================================================
	//  ALL FUNCTIONS
	//======================================================================================================================================================================
	//======================================================================================================================================================================

	//This is here because if we put it on DialogScreen it breaks
	//Either the taverna loads - which automatically shuts off DialogScreen and stops it from turning itself off
	//Or DialogScreen shuts itself off and can't load back the taverna
	//And we *need* DialogScreen to be off when we switch to taverna, because unloading the minigames
	//restores all the screens that were open when it was loaded
	//So we get an empty DialogScreen loading in and locking the game, because it has no buttons to close it
	//Doing it this way *does* leave a split second of black screen during the transition, but it's the best I can figure out right now
	public void CloseTavernDialog() {
		UI.Hide<DialogScreen>();
		MiniGames.EnterScene("TavernaMenu");
	}

	//=====================================================================================================================================	
	//  GUI Interaction Functions are the remaining code below. All of these functions control some aspect of the GUI based on state changes
	//=====================================================================================================================================	
	
	//-------------------------------------------------------------------------------------------------------------------------
	//   GAME OVER NOTIFICATIONS AND COMPONENTS

	public void GUI_ShowGameOverNotification() {
		Session.controlsLocked = true;
		//Set the notification window as active
		gameover_main.SetActive(true);
		//Setup the GameOver Message
		gameover_message.GetComponent<Text>().text = "You have lost! Your crew has died! Your adventure ends here!";
		//GUI_TAB_SetupAShrinePanel the GUI_restartGame button
		gameover_restart.GetComponent<Button>().onClick.RemoveAllListeners();
		gameover_restart.GetComponent<Button>().onClick.AddListener(() => GUI_RestartGame());

	}

	public void GUI_ShowGameIsFinishedNotification() {
		Session.controlsLocked = true;
		//Set the notification window as active
		gameover_main.SetActive(true);
		//Setup the GameOver Message
		gameover_message.GetComponent<Text>().text = "You have Won! Congratulations! Your adventure ends here!";
		//GUI_TAB_SetupAShrinePanel the GUI_restartGame button
		gameover_restart.GetComponent<Button>().onClick.RemoveAllListeners();
		gameover_restart.GetComponent<Button>().onClick.AddListener(() => GUI_RestartGame());

	}


	public void GUI_RestartGame() {
		gameover_main.SetActive(false);
		MainState.menuControlsLock = false;
		//Restart from Beginning
		Game.RestartGame();
	}


	//-------------------------------------------------------------------------------------------------------------------------
	//   DOCKING INFO PANEL AND COMPONENTS    

	public void GUI_ShowPortDockingNotification() {
		Session.controlsLocked = true;

		if(World.crewBeacon.Target == Session.currentSettlement) {
			Session.DeactivateNavigatorBeacon(World.crewBeacon);
		}
		if (World.navigatorBeacon.Target == Session.currentSettlement) {
			Session.DeactivateNavigatorBeacon(World.navigatorBeacon);
		}

		//if (useDialog) {
		port_dialog.SetActive(true);
		Debug.Log("Session.currentSettlement: " + (Session.currentSettlement == null ? "null" : Session.currentSettlement.name));
		port_dialog.GetComponent<DialogScreen>().StartDialog(Session.currentSettlement, useDebugDialog ? debugDialogNode : dialogNode, "port");
	}

	//-------------------------------------------------------------------------------------------------------------------------
	//   DOCKING INFO PANEL AND COMPONENTS    HELPER FUNCTIONS	

	public void GUI_ExitPortNotification() {
		//Turn off both nonport AND port notification windows
		//port_info_main.SetActive(false);
		Session.showPortDockingNotification = false;
		Session.controlsLocked = false;
		MainState.menuControlsLock = false;
	}

	public void GUI_EnterPort(Sprite heraldIcon = null, Sprite noHeraldIcon = null, Intention i = Intention.All, float heraldMod = 1.0f) 
	{
		//Turn off port welcome screen
		Session.showPortDockingNotification = false;
		//port_info_main.SetActive(false);
		//Check if current Settlement is part of the main quest line
		Quests.CheckCityTriggers(Session.currentSettlement.settlementID);
		//Add this settlement to the player's knowledge base
		//Debug.Log("Adding known city from script_GUI: " + Session.currentSettlement.name);
		Session.playerShipVariables.ship.playerJournal.AddNewSettlementToLog(Session.currentSettlement.settlementID);
		//Determine what settlements are available to the player in the tavern
		Session.showSettlementGUI = true;
		Session.showSettlementTradeButton = false;
		Session.controlsLocked = true;

		trade = new TradeViewModel(Session, heraldIcon, noHeraldIcon, i.Equals(Intention.Water), i.Equals(Intention.All), heraldMod);
		port = new PortViewModel(Session, Notifications, i.Equals(Intention.All));

		//-------------------------------------------------
		//NEW GUI FUNCTIONS FOR SETTING UP TAB CONTENT
		//Show Port Menu
		UI.Hide<Dashboard>();

		if (i.Equals(Intention.Water) || i.Equals(Intention.Trading)) {
			UI.Show<TownScreen, TradeViewModel>(trade);
		}
		else {
			UI.Show<PortScreen, PortViewModel>(port);
		}

		//Add a new route to the player journey log as a port entry
		Session.playerShipVariables.journey.AddRoute(new PlayerRoute(Session.playerShip.transform.position, Vector3.zero, Session.currentSettlement.settlementID, Session.currentSettlement.name, false, Session.playerShipVariables.ship.totalNumOfDaysTraveled), Session.playerShipVariables, Session.CaptainsLog);
		//We should also update the ghost trail with this route otherwise itp roduce an empty 0,0,0 position later
		Session.playerShipVariables.UpdatePlayerGhostRouteLineRenderer(Game.IS_NOT_NEW_GAME);

		//-------------------------------------------------
		// OTHER PORT GUI SETUP FUNCTIONS
		GetCrewHometowns();
	}

	public IEnumerable<string> GUI_GetListOfPlayerNetworkCities() {
		//Looks through the player's known settlements and adds it to a list
		var result = new List<string>();
		foreach (int knownSettlementID in Session.playerShipVariables.ship.playerJournal.knownSettlements) {
			result.Add( Database.GetSettlementFromID(knownSettlementID).name );
		}
		return result;
	}

	public IEnumerable<string> GetCrewHometowns() {
		//Looks through the hometowns of all crew and adds them to a list
		var result = new List<string>();
		foreach (CrewMember crewman in Session.playerShipVariables.ship.crewRoster) {
			result.Add(Database.GetSettlementFromID(crewman.originCity).name);
		}
		return result;
	}

	public string GUI_GetCrewMakeupList() {
		//Loops through all crewmembers and counts their jobs to put into a list
		int sailors = 0;
		int warriors = 0;
		int slaves = 0;
		int seers = 0;
		int other = 0;
		string list = "";
		foreach (CrewMember crewman in Session.playerShipVariables.ship.crewRoster) {
			switch (crewman.typeOfCrew) {
				case CrewType.Sailor:
					sailors++;
					break;
				case CrewType.Warrior:
					warriors++;
					break;
				case CrewType.Slave:
					slaves++;
					break;
				case CrewType.Seer:
					seers++;
					break;
				default:
					other++;
					break;
			}
		}
		list += "Sailors  - " + sailors.ToString() + "\n";
		list += "Warriors - " + warriors.ToString() + "\n";
		list += "Seers    - " + seers.ToString() + "\n";
		list += "Slaves   - " + slaves.ToString() + "\n";
		list += "Other   - " + other.ToString() + "\n";

		return list;
	}


	//=================================================================================================================
	// HELPER FUNCTIONS FOR IN-PORT TRADE WINDOW
	//=================================================================================================================	
	


	//This function activates the docking element when the dock button is clicked. A bool is passed to determine whether or not the button is responsive
	public void GUI_checkOutOrDockWithPort(bool isAvailable) {
		if (isAvailable) {
			//Figure out the tax on the cargo hold
			Session.currentPortTax = Session.Trade.GetTaxRateOnCurrentShipManifest();
			Session.showPortDockingNotification = true;
		}
		//Else do nothing
	}

	//============================================================================================================================================================================
	//============================================================================================================================================================================
	// ADDITIONAL FUNCTIONS FOR GUI BUTTONS (These are linked from the Unity Editor)
	//============================================================================================================================================================================

	//-----------------------------------------------------
	//THIS IS THE REST BUTTON

	// REFERENCED IN BUTTON CLICK UNITYEVENT
	public void GUI_restOverNight() {
		//If the controls are locked--we are traveling so force it to stop
		if (Session.controlsLocked && !Session.showSettlementGUI)
			Session.playerShipVariables.rayCheck_stopShip = true;
		//Run a script on the player controls that fast forwards time by a quarter day
		Session.controlsLocked = true;
		Session.playerShipVariables.PassTime(.25f, false);
	}

}
