//=======================================================================================================================================
//
//  globalVariables.cs   -- Main Global Variable code interface
//
//    --This script is for setting up several things:
//
//      Game Inisitialization: This sets up the entire game world, and when finished, allows the player to interact with the game.  
//          --This involves building the wind and water current vectors, settlements, and other game objects that populate the world
//      Global Classes: Eventually all classes should reside here.
//      Global Variables: All variables that need to easily be accessed by all scripts are stored here for centralized access.
//          --This includes stored references to other script objects with variables, e.g. if the GUI needs to access variables
//          --of the player's current Ship object attached to the player_controls script
//      Global Functions: The bulk of this script is to handle most of the non-control logic of the game, e.g. running algorithms to
//          --determine aggregate clout scores, or the costs of various resources or other utilities the player can access.
//
//      NOTE: This script does not have an update function. It performs no loop in the game's core logic. It initializes the game world,
//              --and then acts as a resevoir of functions and data for the other scripts to access
//
//======================================================================================================================================

using UnityEngine;
using System.Collections;
using System.Text;
using System.IO;
using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;


//======================================================================================================================================================================
//======================================================================================================================================================================
//  SETUP ALL GLOBAL VARIABLES
//======================================================================================================================================================================
//======================================================================================================================================================================

public class GameVars : MonoBehaviour
{
	Notifications Notifications => Globals.Notifications;
	Database Database => Globals.Database;

	const int windZoneColumns = 64;
	const int windZoneRows = 32;

	const int currentZoneColumns = 128;
	const int currentZoneRows = 64;

	// TODO: Is this a bug? These never change.
	public const string TD_year = "2000";
	public const string TD_month = "1";
	public const string TD_day = "1";
	public const string TD_hour = "0";
	public const string TD_minute = "0";
	public const string TD_second = "0";

	[Header("World Scene Refs")]
	public GameObject terrain;

	[Header("GUI Scene Refs")]
	public script_GUI MasterGUISystem;

	[Header("Skybox Scene Refs")]
	public GameObject skybox_celestialGrid;
	public GameObject skybox_MAIN_CELESTIAL_SPHERE;
	public GameObject skybox_ecliptic_sphere;
	public GameObject skybox_clouds;
	public GameObject skybox_horizonColor;
	public GameObject skybox_sun;
	public GameObject skybox_moon;

	[Header("Material Asset Refs")]
	public Material mat_waterCurrents;
	public Material mat_water;

	[Header("World Scene Refs")]
	public GameObject FPVCamera;
	public GameObject camera_Mapview;
	public GameObject cityLightsParent;

	[Header("Ship Scene Refs")]
	public GameObject[] sails = new GameObject[6];
	public GameObject[] shipLevels;

	[Header("Ununorganized Scene Refs")]
	public List<CrewMember> currentlyAvailableCrewMembersAtPort; // updated every time ship docks at port
	[Header("GUI Scene Refs")]
	public GameObject selection_ring;

	[Header("Beacons")]
	public Beacon navigatorBeacon;
	public Beacon crewBeacon;

	//###################################
	//	DEBUG VARIABLES
	//###################################
	[ReadOnly] public int DEBUG_currentQuestLeg = 0;
	[HideInInspector] public bool DEBUG_MODE_ON = false;

	// TODO: unorganized variables
	[HideInInspector] public GameObject mainCamera;
	[HideInInspector] public GameObject playerTrajectory;
	[HideInInspector] public LineRenderer playerGhostRoute;
	[HideInInspector] public WindRose[,] windrose_January = new WindRose[10, 8];
	[HideInInspector] public GameObject windZoneParent;
	[HideInInspector] public GameObject waterSurface;
	[HideInInspector] public CurrentRose[,] currentRose_January;
	[HideInInspector] public GameObject currentZoneParent;

	[HideInInspector] public GameObject settlement_masterList_parent;

	// environment
	[HideInInspector] public Light mainLightSource;

	// title and start screens
	[HideInInspector] public bool startGameButton_isPressed = false;
	[HideInInspector] public GameObject camera_titleScreen;


	//###################################
	//	GUI VARIABLES
	//###################################
	[HideInInspector] public bool[] newGameCrewSelectList = new bool[40];
	[HideInInspector] public List<CrewMember> newGameAvailableCrew = new List<CrewMember>();


	//###################################
	//	RANDOM EVENT VARIABLES
	//###################################
	[HideInInspector] public List<int> activeSettlementInfluenceSphereList = new List<int>();





	//======================================================================================================================================================================
	//======================================================================================================================================================================
	//  INITIALIZE THE GAME WORLD
	//======================================================================================================================================================================
	//======================================================================================================================================================================


	// Use this for initialization
	void Awake() {

		Globals.Register(this);

		mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
		camera_titleScreen = GameObject.FindGameObjectWithTag("camera_titleScreen");
		waterSurface = GameObject.FindGameObjectWithTag("waterSurface");
		playerGhostRoute = GameObject.FindGameObjectWithTag("playerGhostRoute").GetComponent<LineRenderer>();
		playerTrajectory = GameObject.FindGameObjectWithTag("playerTrajectory");
		mainLightSource = GameObject.FindGameObjectWithTag("main_light_source").GetComponent<Light>();

		// instance to avoid changing material on disk
		mat_water = new Material(mat_water);
		mat_waterCurrents = new Material(mat_waterCurrents);

		Globals.Register(new Notifications());
		Globals.Register(new MainState());
		Globals.Register(new Database());

		// wind and current init
		BuildWindZoneGameObjects();
		BuildCurrentZoneGameObjects();
		windrose_January = CSVLoader.LoadWindRoses(windZoneColumns, windZoneRows);
		currentRose_January = CSVLoader.LoadWaterZonesFromFile(currentZoneColumns, currentZoneRows);
		SetInGameWindZonesToWindRoseData();
		SetInGameWaterZonesToCurrentRoseData();
	}


	//======================================================================================================================================================================
	//======================================================================================================================================================================
	//  THE REMAINDER OF THE SCRIPT IS ALL GLOBALLY ACCESSIBLE FUNCTIONS
	//======================================================================================================================================================================
	//======================================================================================================================================================================
	

	//====================================================================================================
	//      CSV / DATA LOADING FUNCTIONS
	//====================================================================================================


	public bool LoadSavedGame() {
		PlayerJourneyLog loadedJourney = new PlayerJourneyLog();
		Ship ship = playerShipVariables.ship;

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
		playerShipVariables.journey = loadedJourney;

		//Now use the last line of data to update the current player status and load the game
		string[] playerVars = fileByLine[fileByLine.Length - 1].Split(lineDelimiter, StringSplitOptions.None);

		//Update in game Time
		ship.totalNumOfDaysTraveled = float.Parse(playerVars[1]);
		//Update Sky to match time
		playerShipVariables.UpdateDayNightCycle(MainState.IS_NOT_NEW_GAME);

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
		playerShip.transform.position = new Vector3(float.Parse(parsedXYZ[0]), float.Parse(parsedXYZ[1]), float.Parse(parsedXYZ[2]));

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
		playerShipVariables.dayCounterStarving = int.Parse(playerVars[32]);
		playerShipVariables.dayCounterThirsty = int.Parse(playerVars[33]);

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
			for (int x = 0; x < settlement_masterList_parent.transform.childCount; x++)
				if (settlement_masterList_parent.transform.GetChild(x).GetComponent<script_settlement_functions>().thisSettlement.settlementID == targetID)
					location = settlement_masterList_parent.transform.GetChild(x).position;
			ActivateNavigatorBeacon(navigatorBeacon, location);
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
		currentCaptainsLog = restoreCommasAndNewLines.Replace('*', '\n');
		//Debug.Log (currentCaptainsLog);

		// KDTODO: This needs to be done every time we add a new field right now. Needs a rewrite.
		ship.upgradeLevel = int.Parse(playerVars[40]);
		ship.crewCapacity = int.Parse(playerVars[41]);
		ship.cargo_capicity_kg = int.Parse(playerVars[42]);
		SetShipModel(ship.upgradeLevel);

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

	//====================================================================================================
	//      GAMEOBJECT BUILDING TO POPULATE WORLD FUNCTIONS
	//====================================================================================================

	public void CreateSettlementsFromList() {
		settlement_masterList_parent = Instantiate(new GameObject(), Vector3.zero, transform.rotation) as GameObject;
		settlement_masterList_parent.name = "Settlement Master List";
		foreach (Settlement settlement in Database.settlement_masterList) {
			GameObject currentSettlement;
			//Here we add a model/prefab to the settlement based on it's
			try {
				//Debug.Log ("BEFORE TRYING TO LOAD SETTLEMENT PREFAB    " + settlement.prefabName + "  :   " + settlement.name);
				currentSettlement = Instantiate(Resources.Load("City Models/" + settlement.prefabName, typeof(GameObject))) as GameObject;
				//Debug.Log ("AFTER TRYING TO LOAD SETTLEMENT PREFAB    " + settlement.prefabName);
			}
			catch {
				currentSettlement = Instantiate(Resources.Load("City Models/PF_settlement", typeof(GameObject))) as GameObject;
			}
			//We need to check if the settlement has an adjusted position or not--if it does then use it, otherwise use the given lat long coordinate
			if (settlement.adjustedGamePosition.x == 0) {
				Vector2 tempXY = CoordinateUtil.Convert_WebMercator_UnityWorld(CoordinateUtil.ConvertWGS1984ToWebMercator(settlement.location_longXlatY));
				Vector3 tempPos = new Vector3(tempXY.x, terrain.GetComponent<Terrain>().SampleHeight(new Vector3(tempXY.x, 0, tempXY.y)), tempXY.y);
				currentSettlement.transform.position = tempPos;
			}
			else {
				currentSettlement.transform.position = settlement.adjustedGamePosition;
				currentSettlement.transform.eulerAngles = new Vector3(0, settlement.eulerY, 0);
			}
			currentSettlement.tag = "settlement";
			currentSettlement.name = settlement.name;
			currentSettlement.layer = 8;
			//Debug.Log ("*********************************************  <<>>>" + currentSettlement.name + "   :   " + settlement.settlementID);
			currentSettlement.GetComponent<script_settlement_functions>().thisSettlement = settlement;
			currentSettlement.transform.SetParent(settlement_masterList_parent.transform);
			settlement.theGameObject = currentSettlement;
		}
	}

	public void BuildWindZoneGameObjects() {
		//We need to create a gridded system of GameObjects to represent the windzones
		//It should be a Main Parent GameObject with a series of zones with a rotater and particle system
		//	--WindZones
		//		--0_0
		//			--Particle Rotater
		//				--Wind particle system
		windZoneParent = new GameObject();
		windZoneParent.name = "WindZones Parent Object";
		float originX = 0;
		float originZ = 4096; //Unity's 2D top-down Y axis is Z
		float zoneHeight = 128;
		float zoneWidth = 64;

		for (int col = 0; col < windZoneColumns; col++) {
			for (int row = 0; row < windZoneRows; row++) {
				GameObject newZone = new GameObject();
				GameObject rotater = new GameObject();
				GameObject windParticles;// = Instantiate(new GameObject(), Vector3.zero, transform.rotation) as GameObject;
				newZone.transform.position = new Vector3(originX + (col * zoneWidth), 0, originZ - (row * zoneHeight));
				newZone.transform.localScale = new Vector3(zoneWidth, 1f, zoneHeight);
				newZone.name = col + "_" + row;
				newZone.tag = "windDirectionVector";
				newZone.AddComponent<BoxCollider>();
				newZone.GetComponent<BoxCollider>().isTrigger = true;
				newZone.GetComponent<BoxCollider>().size = new Vector3(.95f, 10, .95f);
				newZone.layer = 20;
				rotater.AddComponent<script_WaterWindCurrentVector>();
				rotater.transform.position = newZone.transform.position;
				rotater.transform.rotation = newZone.transform.rotation;
				rotater.name = "Particle Rotater";
				windParticles = Instantiate(Resources.Load("PF_windParticles", typeof(GameObject))) as GameObject;
				windParticles.transform.position = new Vector3(newZone.transform.position.x, newZone.transform.position.y, newZone.transform.position.z - (zoneHeight / 2));

				windParticles.transform.parent = rotater.transform;
				rotater.transform.parent = newZone.transform;
				newZone.transform.parent = windZoneParent.transform;
				rotater.SetActive(false);
			}
		}
	}

	public void BuildCurrentZoneGameObjects() {
		//We need to create a gridded system of GameObjects to represent the windzones
		//It should be a Main Parent GameObject with a series of zones with a rotater and particle system
		//	--WindZones
		//		--0_0
		//			--Particle Rotater
		//				--Wind particle system
		currentZoneParent = new GameObject();
		currentZoneParent.name = "CurrentZones Parent Object";
		float originX = 0;
		float originZ = 4096; //Unity's 2D top-down Y axis is Z
		float zoneHeight = 64;
		float zoneWidth = 32;

		for (int col = 0; col < currentZoneColumns; col++) {
			for (int row = 0; row < currentZoneRows; row++) {
				GameObject newZone = new GameObject();
				GameObject rotater = new GameObject();
				GameObject currentParticles;// = Instantiate(new GameObject(), Vector3.zero, transform.rotation) as GameObject;
				newZone.transform.position = new Vector3(originX + (col * zoneWidth), 0, originZ - (row * zoneHeight));
				newZone.transform.localScale = new Vector3(zoneWidth, 1f, zoneHeight);
				newZone.name = col + "_" + row;
				newZone.tag = "currentDirectionVector";
				newZone.AddComponent<BoxCollider>();
				newZone.GetComponent<BoxCollider>().isTrigger = true;
				newZone.GetComponent<BoxCollider>().size = new Vector3(.95f, 10, .95f);
				newZone.layer = 19;
				rotater.AddComponent<script_WaterWindCurrentVector>();
				rotater.transform.position = newZone.transform.position;
				rotater.transform.rotation = newZone.transform.rotation;
				rotater.name = "Particle Rotater";
				currentParticles = Instantiate(Resources.Load("PF_currentParticles", typeof(GameObject))) as GameObject;
				currentParticles.transform.position = new Vector3(newZone.transform.position.x, newZone.transform.position.y, newZone.transform.position.z - (zoneHeight / 2));
				currentParticles.transform.Translate(-transform.forward * .51f, Space.Self);
				currentParticles.transform.parent = rotater.transform;
				rotater.transform.parent = newZone.transform;
				newZone.transform.parent = currentZoneParent.transform;
				rotater.SetActive(false);

			}
		}
	}

	public void GenerateCityLights() {
		for (int i = 0; i < settlement_masterList_parent.transform.childCount; i++) {
			GameObject currentCityLight = Instantiate(Resources.Load("PF_cityLights", typeof(GameObject))) as GameObject;

			// use the center of the collider bounds instead of the position since the models are weirdly offset in many of these
			currentCityLight.transform.SetParent(cityLightsParent.transform);
			currentCityLight.transform.position = settlement_masterList_parent.transform.GetChild(i).GetComponent<script_settlement_functions>().anchor.position;
		}
	}

	public void SetInGameWindZonesToWindRoseData() {

		//For each of the zones in the Wind Zone parent GameObject, we need to loop through them
		//	--and set the rotation of each to match the windrose data
		for (int currentZone = 0; currentZone < windZoneParent.transform.childCount; currentZone++) {
			string zoneID = windZoneParent.transform.GetChild(currentZone).name;
			//Debug.Log(zoneID);
			int col = int.Parse(zoneID.Split('_')[0]);
			int row = int.Parse(zoneID.Split('_')[1]);

			//Find the matching wind rose in the month of january
			float speed = 1;
			float direction = UnityEngine.Random.Range(0f, 90f);
			if (windrose_January[col, row] != null) {
				speed = windrose_January[col, row].speed;
				direction = windrose_January[col, row].direction;
			}
			windZoneParent.transform.GetChild(currentZone).GetChild(0).transform.eulerAngles = new Vector3(0, -1f * (direction - 90f), 0); //We subtract 90 because Unity's 'zero' is set at 90 degrees and Unity's positive angle is CW and not CCW like normal trig
			windZoneParent.transform.GetChild(currentZone).GetChild(0).GetComponent<script_WaterWindCurrentVector>().currentMagnitude = speed;
			//if (speed == 0) windZoneParent.transform.GetChild (currentZone).GetChild(0).gameObject.SetActive(false);
			//else windZoneParent.transform.GetChild (currentZone).GetChild(0).gameObject.SetActive(true);
		}

	}

	public void SetInGameWaterZonesToCurrentRoseData() {

		//For each of the zones in the Wind Zone parent GameObject, we need to loop through them
		//	--and set the rotation of each to match the windrose data
		for (int currentZone = 0; currentZone < currentZoneParent.transform.childCount; currentZone++) {
			string zoneID = currentZoneParent.transform.GetChild(currentZone).name;
			//Debug.Log(zoneID);
			int col = int.Parse(zoneID.Split('_')[0]);
			int row = int.Parse(zoneID.Split('_')[1]);

			//Find the matching current rose in the month of january
			float speed = 1;
			float direction = UnityEngine.Random.Range(0f, 90f);
			if (currentRose_January[col, row] != null) {
				speed = currentRose_January[col, row].speed;
				direction = currentRose_January[col, row].direction;
			}
			currentZoneParent.transform.GetChild(currentZone).GetChild(0).transform.eulerAngles = new Vector3(0, -1f * (direction - 90f), 0); //We subtract 90 because Unity's 'zero' is set at 90 degrees and Unity's positive angle is CW and not CCW like normal trig
			currentZoneParent.transform.GetChild(currentZone).GetChild(0).GetComponent<script_WaterWindCurrentVector>().currentMagnitude = speed;
			//if (speed == 0) currentZoneParent.transform.GetChild (currentZone).GetChild(0).gameObject.SetActive(false);
			//else currentZoneParent.transform.GetChild (currentZone).GetChild(0).gameObject.SetActive(true);
			//Debug.Log ("Turning water on?");
		}

	}




	//====================================================================================================
	//      DATA SAVING FUNCTIONS
	//====================================================================================================
	public void SaveUserGameData() {
		string delimitedData = playerShipVariables.journey.ConvertJourneyLogToCSVText();
		Debug.Log(delimitedData);
		string filePath = Application.persistentDataPath + "/";

		string fileNameServer = "";
		if (DEBUG_MODE_ON)
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
			System.IO.File.WriteAllText(Application.persistentDataPath + "/save.json", JsonUtility.ToJson(playerShipVariables.ship));
		}
		catch (Exception e) {
			Notifications.ShowANotificationMessage("ERROR: a backup wasn't saved at: " + Application.persistentDataPath + "  - which means it may not have uploaded either: " + e.Message);
		}
		//Only upload to the server is the DebugMode is OFF
		if (!DEBUG_MODE_ON) SaveUserGameDataToServer(filePath, fileNameServer);

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

		//Debug.Log ("Quest Seg: " + playerShipVariables.ship.mainQuest.currentQuestSegment);
		//First we need to save the game that just ended
		SaveUserGameData();
		//Then we need to re-initialize all the player's variables
		playerShipVariables.Reset();

		//Reset Other Player Ship Variables
		playerShipVariables.numOfDaysTraveled = 0;
		playerShipVariables.numOfDaysWithoutProvisions = 0;
		playerShipVariables.numOfDaysWithoutWater = 0;
		playerShipVariables.dayCounterStarving = 0;
		playerShipVariables.dayCounterThirsty = 0;

		//Take player back to title screen
		//Debug.Log ("GOING TO TITLE SCREEN");
		Globals.UI.HideAll();
		Globals.UI.Show<TitleScreen, GameViewModel>(new GameViewModel());
		controlsLocked = true;
		MainState.isTitleScreen = true;
		RenderSettings.fog = false;
		FPVCamera.SetActive(false);
		camera_titleScreen.SetActive(true);
		MainState.isTitleScreen = true;
		MainState.runningMainGameGUI = false;

		SetShipModel(playerShipVariables.ship.upgradeLevel);

		//clear captains log
		currentCaptainsLog = "";
		
	}

	public void FillNewGameCrewRosterAvailability() {
		//We need to fill a list of 40 crewmembers for the player to choose from on a new game start
		//--The first set will come from the Argonautica, and the top of the list will be populated with necessary characters for the plot
		//--The remainder will be filled from the remaining available argonautica start crew and then randomly generated crew to choose from to create 40 members

		//initialize a fresh List of crew and corresponding array of 40 bools
		newGameAvailableCrew = new List<CrewMember>();
		newGameCrewSelectList = new bool[40];

		//TODO FIX THIS LATER Let's remove the randomly generated crew--this is just a safety precaution--might not be needed.
		playerShipVariables.ship.crewRoster.Clear();

		//Let's add all the optional crew from the Argonautica
		foreach (int crewID in playerShipVariables.ship.mainQuest.questSegments[0].crewmembersToAdd) {
			CrewMember currentMember = Database.GetCrewMemberFromID(crewID);
			if (currentMember.isKillable && !currentMember.isJason) {
				newGameAvailableCrew.Add(currentMember);
			}
		}

		//Now let's add all the possible non-quest historical people for hire
		foreach (CrewMember thisMember in Crew.StandardCrew) {
			//make sure we don't go over 40 listings
			if (newGameAvailableCrew.Count == 40)
				break;

			if (!thisMember.isPartOfMainQuest) {
				newGameAvailableCrew.Add(thisMember);
			}
		}


		//Now let's add randomly generated crew to the list until the quota of 40 is fulfilled
		while (newGameAvailableCrew.Count < 40) {
			newGameAvailableCrew.Add(GenerateRandomCrewMembers(1)[0]);
		}

		// filter out people who don't have connections at the ports in your starting bay or have overwhelmingly large networks
		// prefer random people with small networks over argonautica crew who have very large networks. you should have to hire these people later
		var nearestToStart = new string[] { "Pagasae", "Iolcus", "Pherai (Thessaly)", "Phylace", "Tisaia", "Histiaia/Oreos" };
		var bestOptions = from c in newGameAvailableCrew
						  let network = Network.GetCrewMemberNetwork(c)
						  where network.Any(s => nearestToStart.Contains(s.name)) && network.Count() < 10
						  select c;

		// use people with low # connections as backup options. this is just to keep the early game from being confusing
		var backupOptions = from c in newGameAvailableCrew
							let network = Network.GetCrewMemberNetwork(c)
							where network.Count() < 10
							select c;

		var remainingNeeded = Ship.StartingCrewSize - bestOptions.Count();
		if(remainingNeeded > 0) {
			newGameAvailableCrew = bestOptions.Concat(backupOptions.Take(remainingNeeded)).ToList();
		}
		else {
			newGameAvailableCrew = bestOptions.ToList();
		}

	}

	public List<CrewMember> GenerateRandomCrewMembers(int numberOfCrewmanNeeded) {
		//This function pulls from the list of available crewmembers in the world and selects random crewman from that list of a defined
		//	--size that isn't already on board the ship and returns it. This may not return a full list if the requested number is too high--it will return
		//	--the most it has available
		List<CrewMember> availableCrew = new List<CrewMember>();
		int numOfIterations = 0;
		int numStandardCrew = Crew.StandardCrew.Count();
		while (numberOfCrewmanNeeded != availableCrew.Count) {
			CrewMember thisMember = Crew.StandardCrew.RandomElement();
			if (!thisMember.isPartOfMainQuest) {
				//Now make sure this crewmember isn't already in the current crew
				if(!playerShipVariables.ship.crewRoster.Contains(thisMember)) {
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
		if (gameDifficulty_Beginner)
			camera_Mapview.GetComponent<Camera>().enabled = true;
		else
			camera_Mapview.GetComponent<Camera>().enabled = false;
	}

	public void LoadSavedGhostRoute() {
		//For the loadgame function--it just fills the ghost trail with the routes that exist
		playerGhostRoute.positionCount = playerShipVariables.journey.routeLog.Count;
		for (int routeIndex = 0; routeIndex < playerShipVariables.journey.routeLog.Count; routeIndex++) {
			Debug.Log("GhostRoute Index: " + routeIndex);
			playerGhostRoute.SetPosition(routeIndex, playerShipVariables.journey.routeLog[routeIndex].UnityXYZEndPoint - new Vector3(0, playerShip.transform.position.y, 0));
			//set player last origin point for next route add on
			if (routeIndex == playerShipVariables.journey.routeLog.Count - 1) {
				playerShipVariables.travel_lastOrigin = playerShipVariables.journey.routeLog[routeIndex].UnityXYZEndPoint - new Vector3(0, playerShip.transform.position.y);
				playerShipVariables.originOfTrip = playerShipVariables.journey.routeLog[routeIndex].UnityXYZEndPoint - new Vector3(0, playerShip.transform.position.y);
			}
		}
	}




}///////// END OF FILE
