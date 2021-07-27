using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Game
{
	public enum Difficulty {
		Normal,
		Beginner
	}

	Notifications Notifications => Globals.Notifications;
	Database Database => Globals.Database;
	World World => Globals.World;

	public GameSession Session { get; private set; }

	// TODO: Is this a bug? These never change.
	public const bool IS_NEW_GAME = true;
	public const bool IS_NOT_NEW_GAME = false;

	public bool isTitleScreen { get; set; } = true;
	public bool isStartScreen { get; set; } = false;
	public bool isLoadedGame { get; set; } = false;
	public bool runningMainGameGUI { get; set; } = false;

	public bool isGameOver { get; set; } = false;
	public bool menuControlsLock { get; set; } = false;
	public bool gameIsFinished { get; set; } = false;


	#region UI Triggered functions

	public void StartNewGame(Difficulty difficulty) {

		isTitleScreen = false;
		isStartScreen = true;

		Globals.UI.Hide<TitleScreen>();

		FillNewGameCrewRosterAvailability();

		if (difficulty == Difficulty.Normal) Session.gameDifficulty_Beginner = false;
		else Session.gameDifficulty_Beginner = true;
		SetupBeginnerGameDifficulty();

		// since we're skipping crew select, force pick the first StartingCrewSize members
		for (var i = 0; i < Ship.StartingCrewSize; i++) {
			World.newGameCrewSelectList[i] = true;
		}

		// TODO: For now, skip straight to starting the game since i turned off crew selection
		StartMainGame();
		//play the intro for the game
		//time_Line_controll.play();//start the intro seen
		// TODO: Turned off crew selection because it's too overwhelming. Needs to be reworked.
		//title_crew_select.SetActive(true);
		//GUI_SetupStartScreenCrewSelection();

	}

	void StartMainGame() {
		World.camera_titleScreen.SetActive(false);

		//Turn on the environment fog
		RenderSettings.fog = true;

		//Now turn on the main player controls camera
		World.FPVCamera.SetActive(true);

		//Turn on the player distance fog wall
		Session.playerShipVariables.fogWall.SetActive(true);

		//Now change titleScreen to false
		isTitleScreen = false;
		isStartScreen = false;

		//Now enable the controls
		Session.controlsLocked = false;

		//Initiate the main questline
		Globals.Quests.InitiateMainQuestLineForPlayer();

		//Reset Start Game Button
		World.startGameButton_isPressed = false;

		// TODO: Crew select disabled for now
		//title_crew_select.SetActive(false);

		//Turn on the ship HUD
		Globals.UI.Show<Dashboard, DashboardViewModel>(new DashboardViewModel());
	}

	public void LoadGame(Difficulty difficulty) {
		if (LoadSavedGame()) {
			isLoadedGame = true;
			isTitleScreen = false;
			isStartScreen = false;

			Globals.UI.Hide<TitleScreen>();

			World.camera_titleScreen.SetActive(false);



			//Turn on the environment fog
			RenderSettings.fog = true;
			//Now turn on the main player controls camera
			World.FPVCamera.SetActive(true);
			//Turn on the player distance fog wall
			Session.playerShipVariables.fogWall.SetActive(true);
			//Now enable the controls
			Session.controlsLocked = false;
			//Set the player's initial position to the new position
			Session.playerShipVariables.lastPlayerShipPosition = Session.playerShip.transform.position;
			//Update Ghost Route
			LoadSavedGhostRoute();


			//Setup Difficulty Level
			if (difficulty == Game.Difficulty.Normal) Session.gameDifficulty_Beginner = false;
			else Session.gameDifficulty_Beginner = true;
			SetupBeginnerGameDifficulty();

			//Turn on the ship HUD
			Globals.UI.Show<Dashboard, DashboardViewModel>(new DashboardViewModel());

			Session.controlsLocked = false;
			//Flag the main GUI scripts to turn on
			runningMainGameGUI = true;
		}
	}

	#endregion

	#region Main Functions

	public bool LoadSavedGame() {
		var session = Session;

		PlayerJourneyLog loadedJourney = new PlayerJourneyLog();
		Ship ship = session.playerShipVariables.ship;

		string[] splitFile = new string[] { "\r\n", "\r", "\n" };
		char[] lineDelimiter = new char[] { ',' };
		char[] recordDelimiter = new char[] { '_' };

		//Look for a save game file and tell the player if none is found.
		string saveText;
		try {
			saveText = File.ReadAllText(Application.persistentDataPath + "/player_save_game.txt");
		}
		catch (Exception error) {
			Notifications.ShowANotificationMessage("Sorry! No load game 'player_save_game.txt' was found in the game directory '" + Application.persistentDataPath + "' or the save file is corrupt!\nError Code: " + error);
			return false;
		}
		//	TextAsset saveGame = (TextAsset)Resources.Load("player_save_game", typeof(TextAsset));
		string[] fileByLine = saveText.Split(splitFile, StringSplitOptions.None);
		Debug.Log("file://" + Application.persistentDataPath + "/player_save_game.txt");
		Debug.Log(saveText);

		if (fileByLine.Length == 0) return false;
		//start at index 1 to skip the record headers we have to then subtract 
		//one when adding NEW entries to the list to ensure we start at ZERO and not ONE
		//all past routes will be stored as text, but the last route(last line of file) will also be done this way, but will additionally be parsed out for editing in-game values
		for (int lineCount = 1; lineCount < fileByLine.Length; lineCount++) {
			string[] records = fileByLine[lineCount].Split(lineDelimiter, StringSplitOptions.None);

			//First Add the basic route
			Vector3 origin = new Vector3(float.Parse(records[2]), float.Parse(records[3]), float.Parse(records[4]));
			Vector3 destination = new Vector3(float.Parse(records[5]), float.Parse(records[6]), float.Parse(records[7]));
			float numOfDays = float.Parse(records[1]);

			loadedJourney.routeLog.Add(new PlayerRoute(origin, destination, numOfDays));

			//Next add the cargo manifest
			string CSVcargo = "";
			for (int i = 8; i < 23; i++) {
				CSVcargo += "," + records[i];
			}
			loadedJourney.cargoLog.Add(CSVcargo);

			//Next add the other attributes string
			// KDTODO: This needs to update whenever we add a new field right now. Needs a rewrite.
			string CSVotherAtt = "";
			for (int i = 23; i < 42; i++) {
				CSVotherAtt += "," + records[i];
			}
			loadedJourney.otherAttributes.Add(CSVotherAtt);

			//Update Ship Position
			string[] XYZ = records[27].Split(recordDelimiter, StringSplitOptions.None);
			loadedJourney.routeLog[loadedJourney.routeLog.Count - 1].UnityXYZEndPoint = new Vector3(float.Parse(XYZ[0]), float.Parse(XYZ[1]), float.Parse(XYZ[2]));

		}
		session.playerShipVariables.journey = loadedJourney;

		//Now use the last line of data to update the current player status and load the game
		string[] playerVars = fileByLine[fileByLine.Length - 1].Split(lineDelimiter, StringSplitOptions.None);

		//Update in game Time
		ship.totalNumOfDaysTraveled = float.Parse(playerVars[1]);
		//Update Sky to match time
		session.playerShipVariables.UpdateDayNightCycle(Game.IS_NOT_NEW_GAME);

		//Update all Cargo Holds
		int fileStartIndex = 8;
		foreach (Resource resource in ship.cargo) {
			resource.amount_kg = float.Parse(playerVars[fileStartIndex]);
			fileStartIndex++;
		}

		//Update all Crewmen
		List<CrewMember> updatedCrew = new List<CrewMember>();
		string[] parsedCrew = playerVars[26].Split(recordDelimiter, StringSplitOptions.None);
		foreach (string crewID in parsedCrew) {
			updatedCrew.Add(Database.GetCrewMemberFromID(int.Parse(crewID)));
		}
		ship.crewRoster.Clear();
		updatedCrew.ForEach(c => ship.crewRoster.Add(c));

		//Update Ship Position
		string[] parsedXYZ = playerVars[27].Split(recordDelimiter, StringSplitOptions.None);
		session.playerShip.transform.position = new Vector3(float.Parse(parsedXYZ[0]), float.Parse(parsedXYZ[1]), float.Parse(parsedXYZ[2]));

		//Update Current Quest Leg
		ship.mainQuest.currentQuestSegment = int.Parse(playerVars[28]);

		// Update objective
		ship.objective = Globals.Quests.CurrSegment?.objective;

		//Update Ship Health
		ship.health = float.Parse(playerVars[29]);

		//Update player clout
		ship.playerClout = float.Parse(playerVars[30]);

		//Update player networks
		List<int> loadedNetworks = new List<int>();
		string[] parsedNetworks = playerVars[31].Split(recordDelimiter, StringSplitOptions.None);
		foreach (string netID in parsedNetworks) {
			loadedNetworks.Add(int.Parse(netID));
		}
		ship.networks = loadedNetworks;

		//Update player starving and thirsty day counters
		session.playerShipVariables.dayCounterStarving = int.Parse(playerVars[32]);
		session.playerShipVariables.dayCounterThirsty = int.Parse(playerVars[33]);

		//Update Currency
		ship.currency = int.Parse(playerVars[34]);

		//Add any Loans
		//--If Loan exists then add otherwise make null
		if (int.Parse(playerVars[35]) != -1) {
			//TODO right now we aren't storing the loan variable properly so relaly a loaded game means a player can cheat currently--whoops--and have plenty of time to pay it back and their interest disappears. Need to put on fix list
			ship.currentLoan = new Loan(int.Parse(playerVars[35]), 0f, 0f, int.Parse(playerVars[36]));
		}
		else {
			ship.currentLoan = null;
		}

		//Add Current Navigator Destination
		int targetID = int.Parse(playerVars[37]);
		if (targetID != -1) {
			ship.currentNavigatorTarget = targetID;
			//change location of beacon
			Vector3 location = Vector3.zero;
			for (int x = 0; x < World.settlement_masterList_parent.transform.childCount; x++)
				if (World.settlement_masterList_parent.transform.GetChild(x).GetComponent<script_settlement_functions>().thisSettlement.settlementID == targetID)
					location = World.settlement_masterList_parent.transform.GetChild(x).position;
			session.ActivateNavigatorBeacon(World.navigatorBeacon, location);
		}
		else {
			ship.currentNavigatorTarget = -1;
		}
		//Add the Known Settlements

		string[] parsedKnowns = playerVars[38].Split(recordDelimiter, StringSplitOptions.None);
		//Debug.Log ("PARSED KNOWNS: " + playerVars[38]);
		foreach (string settlementID in parsedKnowns) {
			//Debug.Log ("PARSED KNOWNS: " + settlementID);
			ship.playerJournal.knownSettlements.Add(int.Parse(settlementID));
		}
		//Add Captains Log
		string restoreCommasAndNewLines = playerVars[39].Replace('^', ',');
		session.ResetCaptainsLog(restoreCommasAndNewLines.Replace('*', '\n'));
		//Debug.Log (currentCaptainsLog);

		// KDTODO: This needs to be done every time we add a new field right now. Needs a rewrite.
		ship.upgradeLevel = int.Parse(playerVars[40]);
		ship.crewCapacity = int.Parse(playerVars[41]);
		ship.cargo_capicity_kg = int.Parse(playerVars[42]);
		session.SetShipModel(ship.upgradeLevel);

		// KDTODO: Once the save game routines are rewritten, need to save the crew available in each city instead of regenerating since this is exploitable
		// it's just too much hassle to support saving this right now because the save format is limiting
		// setup each city with 5 crew available and for now, they never regenerate.
		foreach (var settlement in Database.settlement_masterList) {
			settlement.availableCrew.Clear();
			GenerateRandomCrewMembers(5).ForEach(c => settlement.availableCrew.Add(c));
		}

		//If no errors then return true
		return true;
	}

	public void SaveUserGameData() {
		var session = Session;

		string delimitedData = session.playerShipVariables.journey.ConvertJourneyLogToCSVText();
		Debug.Log(delimitedData);
		string filePath = Application.persistentDataPath + "/";

		string fileNameServer = "";
		if (World.DEBUG_MODE_ON)
			fileNameServer += "DEBUG_DATA_" + SystemInfo.deviceUniqueIdentifier + "_player_data_" + System.DateTime.UtcNow.ToString("HH-mm-ss_dd_MMMM_yyyy") + ".csv";
		else
			fileNameServer += SystemInfo.deviceUniqueIdentifier + "_player_data_" + System.DateTime.UtcNow.ToString("HH-mm-ss_dd_MMMM_yyyy") + ".csv";

		string fileName = "player_save_game.txt";

		//Adding a try/catch block around this write because if someone tries playing the game out of zip on mac--it throws an error that is unoticeable but also
		//causes the code to fall short and quit before saving to the server
		try {
			//save a backup before Joanna's edits
			System.IO.File.WriteAllText(Application.persistentDataPath + "/BACKUP-" + SystemInfo.deviceUniqueIdentifier + "_player_data_" + System.DateTime.UtcNow.ToString("HH-mm-ss_dd_MMMM_yyyy") + ".csv", delimitedData);
			System.IO.File.WriteAllText(Application.persistentDataPath + "/" + fileName, delimitedData);
			//TODO Temporary addition for joanna to remove the captains log from the server upload
			string fileToUpload = RemoveCaptainsLogForJoanna(delimitedData);
			System.IO.File.WriteAllText(Application.persistentDataPath + "/" + fileNameServer, fileToUpload);
			Debug.Log(Application.persistentDataPath);

			// secretly save a JSON version of the save data to prep for a move to make that the canonical save file - but it's not hooked up to be loaded yet
			System.IO.File.WriteAllText(Application.persistentDataPath + "/save.json", JsonUtility.ToJson(session.playerShipVariables.ship));
		}
		catch (Exception e) {
			Notifications.ShowANotificationMessage("ERROR: a backup wasn't saved at: " + Application.persistentDataPath + "  - which means it may not have uploaded either: " + e.Message);
		}
		//Only upload to the server is the DebugMode is OFF
		if (!World.DEBUG_MODE_ON) SaveUserGameDataToServer(filePath, fileNameServer);

	}

	public void SaveUserGameDataToServer(string localPath, string localFile) {
		Debug.Log("Starting FTP");
		string user = "SamoGameBot";
		string pass = "%Mgn~WxH+CRzj>4Z";
		string host = "34.193.207.222";
		string initialPath = "";

		FileInfo file = new FileInfo(localPath + localFile);
		if (!file.Exists) {
			Debug.LogError("No save file created, so could not save game data to server: " + localFile);
			return;
		}

		Uri address = new Uri("ftp://" + host + "/" + Path.Combine(initialPath, file.Name));
		FtpWebRequest request = FtpWebRequest.Create(address) as FtpWebRequest;

		// Upload options:

		// Provide credentials
		request.Credentials = new NetworkCredential(user, pass);

		// Set control connection to closed after command execution
		request.KeepAlive = false;

		// Specify command to be executed
		request.Method = WebRequestMethods.Ftp.UploadFile;

		// Specify data transfer type
		request.UseBinary = true;

		// Notify server about size of uploaded file
		request.ContentLength = file.Length;

		//Make sure we have a timeout for the connection because the default is Infinite--for instance, if the
		//player is offline, the timeout will never happen if there isn't a value set. We'll set it
		//to 5 seconds(5000ms) as a time
		request.Timeout = 5000;

		// Set buffer size to 2KB.
		var bufferLength = 2048;
		var buffer = new byte[bufferLength];
		var contentLength = 0;

		// Open file stream to read file
		var fs = file.OpenRead();

		try {
			// Stream to which file to be uploaded is written.
			var stream = request.GetRequestStream();

			// Read from file stream 2KB at a time.
			contentLength = fs.Read(buffer, 0, bufferLength);

			// Loop until stream content ends.
			while (contentLength != 0) {
				//Debug.Log("Progress: " + ((fs.Position / fs.Length) * 100f));
				// Write content from file stream to FTP upload stream.
				stream.Write(buffer, 0, contentLength);
				contentLength = fs.Read(buffer, 0, bufferLength);
			}

			// Close file and request streams
			stream.Close();
			fs.Close();
		}
		catch (Exception e) {
			Debug.LogError("Error uploading file: " + e.Message);
			Notifications.ShowANotificationMessage("ERROR: No Upload--The server timed out or you currently do not have a stable internet connection\n" + e.Message);
			return;
		}

		Debug.Log("Upload successful.");
		Notifications.ShowANotificationMessage("File: '" + localFile + "' successfully uploaded to the server!");
	}

	//TODO: This is an incredibly specific function that won't be needed later
	public string RemoveCaptainsLogForJoanna(string file) {
		string[] splitFile = new string[] { "\r\n", "\r", "\n" };
		string newFile = "";
		string[] fileByLine = file.Split(splitFile, StringSplitOptions.None);

		//For each line of the save file (the row)
		for (int row = 0; row < fileByLine.Length; row++) {
			int index = fileByLine[row].LastIndexOf(",");
			newFile += fileByLine[row].Substring(0, index) + "\n";
			//Debug.Log (fileByLine [row]); 
			//Debug.Log (fileByLine [row].Substring (0, index));
		}

		return newFile;

	}


	public void RestartGame() {

		// TOOD: make the session completely recreated on restart. didn't do it yet because i bet the code in this function assumes some leftovers.
		var session = Session;

		//Debug.Log ("Quest Seg: " + playerShipVariables.ship.mainQuest.currentQuestSegment);
		//First we need to save the game that just ended
		SaveUserGameData();
		//Then we need to re-initialize all the player's variables
		session.playerShipVariables.Reset();

		//Reset Other Player Ship Variables
		session.playerShipVariables.numOfDaysTraveled = 0;
		session.playerShipVariables.numOfDaysWithoutProvisions = 0;
		session.playerShipVariables.numOfDaysWithoutWater = 0;
		session.playerShipVariables.dayCounterStarving = 0;
		session.playerShipVariables.dayCounterThirsty = 0;

		//Take player back to title screen
		//Debug.Log ("GOING TO TITLE SCREEN");
		Globals.UI.HideAll();
		Globals.UI.Show<TitleScreen, GameViewModel>(new GameViewModel());
		session.controlsLocked = true;
		isTitleScreen = true;
		RenderSettings.fog = false;
		World.FPVCamera.SetActive(false);
		World.camera_titleScreen.SetActive(true);
		isTitleScreen = true;
		runningMainGameGUI = false;

		session.SetShipModel(session.playerShipVariables.ship.upgradeLevel);

		//clear captains log
		session.ResetCaptainsLog(string.Empty);

	}

	public void FillNewGameCrewRosterAvailability() {
		var session = Session;

		//We need to fill a list of 40 crewmembers for the player to choose from on a new game start
		//--The first set will come from the Argonautica, and the top of the list will be populated with necessary characters for the plot
		//--The remainder will be filled from the remaining available argonautica start crew and then randomly generated crew to choose from to create 40 members

		//initialize a fresh List of crew and corresponding array of 40 bools
		World.newGameAvailableCrew = new List<CrewMember>();
		World.newGameCrewSelectList = new bool[40];

		//TODO FIX THIS LATER Let's remove the randomly generated crew--this is just a safety precaution--might not be needed.
		session.playerShipVariables.ship.crewRoster.Clear();

		//Let's add all the optional crew from the Argonautica
		foreach (int crewID in session.playerShipVariables.ship.mainQuest.questSegments[0].crewmembersToAdd) {
			CrewMember currentMember = Database.GetCrewMemberFromID(crewID);
			if (currentMember.isKillable && !currentMember.isJason) {
				World.newGameAvailableCrew.Add(currentMember);
			}
		}

		//Now let's add all the possible non-quest historical people for hire
		foreach (CrewMember thisMember in session.Crew.StandardCrew) {
			//make sure we don't go over 40 listings
			if (World.newGameAvailableCrew.Count == 40)
				break;

			if (!thisMember.isPartOfMainQuest) {
				World.newGameAvailableCrew.Add(thisMember);
			}
		}


		//Now let's add randomly generated crew to the list until the quota of 40 is fulfilled
		while (World.newGameAvailableCrew.Count < 40) {
			World.newGameAvailableCrew.Add(GenerateRandomCrewMembers(1)[0]);
		}

		// filter out people who don't have connections at the ports in your starting bay or have overwhelmingly large networks
		// prefer random people with small networks over argonautica crew who have very large networks. you should have to hire these people later
		var nearestToStart = new string[] { "Pagasae", "Iolcus", "Pherai (Thessaly)", "Phylace", "Tisaia", "Histiaia/Oreos" };
		var bestOptions = from c in World.newGameAvailableCrew
						  let network = Session.Network.GetCrewMemberNetwork(c)
						  where network.Any(s => nearestToStart.Contains(s.name)) && network.Count() < 10
						  select c;

		// use people with low # connections as backup options. this is just to keep the early game from being confusing
		var backupOptions = from c in World.newGameAvailableCrew
							let network = Session.Network.GetCrewMemberNetwork(c)
							where network.Count() < 10
							select c;

		var remainingNeeded = Ship.StartingCrewSize - bestOptions.Count();
		if (remainingNeeded > 0) {
			World.newGameAvailableCrew = bestOptions.Concat(backupOptions.Take(remainingNeeded)).ToList();
		}
		else {
			World.newGameAvailableCrew = bestOptions.ToList();
		}

	}

	public List<CrewMember> GenerateRandomCrewMembers(int numberOfCrewmanNeeded) {
		//This function pulls from the list of available crewmembers in the world and selects random crewman from that list of a defined
		//	--size that isn't already on board the ship and returns it. This may not return a full list if the requested number is too high--it will return
		//	--the most it has available
		List<CrewMember> availableCrew = new List<CrewMember>();
		int numOfIterations = 0;
		int numStandardCrew = Session.Crew.StandardCrew.Count();
		while (numberOfCrewmanNeeded != availableCrew.Count) {
			CrewMember thisMember = Session.Crew.StandardCrew.RandomElement();
			if (!thisMember.isPartOfMainQuest) {
				//Now make sure this crewmember isn't already in the current crew
				if (!Session.playerShipVariables.ship.crewRoster.Contains(thisMember)) {
					availableCrew.Add(thisMember);
				}
			}
			//Break from the main loop if we've tried enough crewman
			if (numStandardCrew == numOfIterations)
				break;
			numOfIterations++;
		}

		//Return the final List of crewman--it might not be the full amount requested if there aren't enough to pull form
		return availableCrew;

	}

	public void SetupBeginnerGameDifficulty() {
		//Set difficulty level variables
		if (Session.gameDifficulty_Beginner)
			World.camera_Mapview.GetComponent<Camera>().enabled = true;
		else
			World.camera_Mapview.GetComponent<Camera>().enabled = false;
	}

	public void LoadSavedGhostRoute() {
		//For the loadgame function--it just fills the ghost trail with the routes that exist
		World.playerGhostRoute.positionCount = Session.playerShipVariables.journey.routeLog.Count;
		for (int routeIndex = 0; routeIndex < Session.playerShipVariables.journey.routeLog.Count; routeIndex++) {
			Debug.Log("GhostRoute Index: " + routeIndex);
			World.playerGhostRoute.SetPosition(routeIndex, Session.playerShipVariables.journey.routeLog[routeIndex].UnityXYZEndPoint - new Vector3(0, Session.playerShip.transform.position.y, 0));
			//set player last origin point for next route add on
			if (routeIndex == Session.playerShipVariables.journey.routeLog.Count - 1) {
				Session.playerShipVariables.travel_lastOrigin = Session.playerShipVariables.journey.routeLog[routeIndex].UnityXYZEndPoint - new Vector3(0, Session.playerShip.transform.position.y);
				Session.playerShipVariables.originOfTrip = Session.playerShipVariables.journey.routeLog[routeIndex].UnityXYZEndPoint - new Vector3(0, Session.playerShip.transform.position.y);
			}
		}
	}

	#endregion
}
