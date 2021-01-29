using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
 * This file contains data objects that are used in GameVars.loadSaveGame / saveGame to read/write player save data and game state (dynamic data) to/from disk.
 * It differs from StaticDataObjects, which contains non-changing (static data) read using CSVLoader and never changed in memory.
 */

// this is the root save data object for the JSON save file
// to avoid needing to refactor everything at once, some fields are only written to disk, not loaded (loading is still from the CSV). these are marked with:
// TODO: Still loaded from CSV. Review and load from JSON later.
[Serializable]
public class GameData
{
	public const int LatestVersion = 2;

	public static GameData New() => new GameData() { 
		_version = LatestVersion,
		Current = new GameState {
			ship = Globals.GameVars.playerShipVariables.ship
		},
		journey = Globals.GameVars.playerShipVariables.journey,
		History = new List<Snapshot>()
	};

	[SerializeField] int _version;
	public int Version => _version;

	// TODO: add options menu data here, since that is decoupled from "active games"

	public GameState Current;
	public List<Snapshot> History;

	// TODO: Still loaded from CSV. This should be made into an in-memory only structure soon. It's being replaced by the history snapshot list.
	// since this is a reference to the real ship, it's safe to modify from here as well as playerShipVariables.ship
	public PlayerJourneyLog journey;
}

[Serializable]
public class GameState
{
	// TODO: add new captain's log data here

	// TODO: Still loaded from CSV. Review and load from JSON later.
	// since this is a reference to the real ship, it's safe to modify from here as well as playerShipVariables.ship
	public Ship ship;
}

[Serializable]
public class Snapshot
{
	public PlayerRoute Route;
	public GameState State;
}

#region Nested Save Data Structures

/// <summary>
/// Helpers for managing game state and save data
/// </summary>
public static class GameDataExtensions
{
	/// <summary>
	/// Deep clone the GameState to make a snapshot of the current values
	/// by serializing in memory and returning the deserialize result.
	/// </summary>
	public static GameState Clone(this GameState self) {
		var serialized = JsonUtility.ToJson(self);
		return JsonUtility.FromJson<GameState>(serialized);
	}

	/// <summary>
	/// Record a snapshot of the current game state using a deep clone,
	/// and add it to the history list associated with the given route (which the caller should construct new and pre-fill)
	/// </summary>
	/// <param name="self"></param>
	/// <param name="route"></param>
	public static void AddSnapshot(this GameData self, PlayerRoute route) {
		self.History.Add(new Snapshot {
			Route = route,
			State = self.Current.Clone()
		});
	}
}

[Serializable]
public class Loan
{
	public int amount;
	public float interestRate;
	public float numOfDaysUntilDue;
	public float numOfDaysPassedUntilDue;
	public int settlementOfOrigin;

	public Loan(int amount, float interestRate, float numOfDaysUntilDue, int settlementOfOrigin) {
		this.amount = amount;
		this.interestRate = interestRate;
		this.numOfDaysUntilDue = numOfDaysUntilDue;
		this.numOfDaysPassedUntilDue = 0;
		this.settlementOfOrigin = settlementOfOrigin;
	}

	public int GetTotalAmountDueWithInterest() {
		return (int)(amount + (amount * (interestRate / 100)));
	}
}

[Serializable]
public class MainQuestLine
{
	// quest segements come from CSV, not serialized to json
	[NonSerialized] public List<QuestSegment> questSegments;
	public int currentQuestSegment;

	public MainQuestLine() {
		questSegments = new List<QuestSegment>();
		currentQuestSegment = 0;
	}
}

[Serializable]
public class Journal
{
	[Serializable] public class KnownSettlementList : SerializableObservableCollection<int> { }
	public KnownSettlementList knownSettlements;

	public Journal() {
		knownSettlements = new KnownSettlementList();
	}

	public void AddNewSettlementToLog(int settlementID) {
		//Debug.Log ("Adding New SET ID:  -->   " + settlementID);
		//First Check to see if there are any settlements in the log yet
		if (knownSettlements.Count > 0) {
			//check to make sure the id doesn't already exist
			for (int i = 0; i < knownSettlements.Count; i++) {
				if (knownSettlements[i] == settlementID)
					break;//if we find a match break the loop and exit
				else if (i == knownSettlements.Count - 1)//else if there are no matches and we're at the end of the list, add the ID
					this.knownSettlements.Add(settlementID);
			}
		}
		else {//if there are no settlements then just add the settlement
			this.knownSettlements.Add(settlementID);
		}
	}
}

[Serializable]
public class PlayerRoute
{
	//TODO This method as well as the Player Journey Log is a pretty dirty solution that needs some serious clean up and tightening. It's a bit brute force and messy right now.
	public Vector3[] theRoute;
	public int settlementID;
	public string settlementName;
	public bool isLeaving;
	public float timeStampInDays;
	public Vector3 UnityXYZEndPoint;

	public PlayerRoute(Vector3 origin, Vector3 destination, float timeStampInDays) {
		this.theRoute = new Vector3[] { origin, destination };
		this.settlementID = -1;
		this.timeStampInDays = timeStampInDays;
		this.UnityXYZEndPoint = destination;
		Debug.Log("Player route getting called...." + origin.x + "  " + origin.z);

	}

	public PlayerRoute(Vector3 origin, Vector3 destination, int settlementID, string settlementName, bool isLeaving, float timeStampInDays) {
		this.theRoute = new Vector3[] { origin, destination };
		this.settlementID = settlementID;
		this.isLeaving = isLeaving;
		this.settlementName = settlementName;
		this.timeStampInDays = timeStampInDays;
		this.UnityXYZEndPoint = origin;
		Debug.Log("Player route getting called 2...." + origin.x + "  " + origin.z);
		//TODO this is a dirty fix--under normal route conditions the Unity XYZ is changed to XZY to match 3d geometry conventions (z is up and not forward like in Unity)
		//TODO --I'm manually making this port stop XZY here so I don't have to change to much of the ConvertJourneyLogToCSVText() function in the PlayerJourneyLog Class
		//TODO --We only need to change the 'origin' XYZ to XZY because port stops will have Vector3.zero (0,0,0) for the destination values always 
		//TODO --Maybe it's lazy--but I've been at this for about 15 hours non-stop and I'm running out of sophisticated brain power. Probably the narrative of all this code.
		//this.theRoute[0] = new Vector3(this.theRoute[0].x,this.theRoute[0].z,this.theRoute[0].y);
		//this.theRoute[1] = new Vector3(this.theRoute[1].x,this.theRoute[1].z,this.theRoute[1].y);
	}

}

// KDTODO: Need to do a deep dive into everything in AddOtherAttributes / CSVHeader to make sure it's handled correctly before loading from CSV
[Serializable]
public class PlayerJourneyLog
{
	//TODO This whole function needs retooling--rather htan 3 separate arrays, the PlayerRoute object should store all the necessary variables--In fact this could be replaced with a simple List of Player Routes rather than having two separate objects no apparent reason.
	public List<PlayerRoute> routeLog;
	public List<string> cargoLog;
	//This is a quick dirty solution TODO need to make it a bit easier with current timelines of work
	public List<string> otherAttributes;
	public string CSVheader;

	public PlayerJourneyLog() {
		this.routeLog = new List<PlayerRoute>();
		this.cargoLog = new List<string>();
		this.otherAttributes = new List<string>();
		this.CSVheader = "Unique_Machine_ID,timestamp,originE,originN,originZ,endE,endN,endZ," +
			"Water_kg,Provisions_kg,Grain_kg,Wine_kg,Timber_kg,Gold_kg,Silver_kg," +
			"Copper_kg,Tin_kg,Obsidian_kg,Lead_kg,Livestock_kg,Iron_kg,Bronze_kg,Luxury_kg,Is_Leaving_Port,PortID,PortName," +
			"CrewMemberIDs,UnityXYZ,Current_Questleg,ShipHP,Clout,PlayerNetwork,DaysStarving,DaysThirsty,Currency,LoanAmount,LoanOriginID,CurrentNavigatorTarget,KnownSettlements,CaptainsLog,upgradeLevel,crewCap,cargoCap\n";
	}

	public void AddRoute(PlayerRoute routeToAdd, script_player_controls playerShipVars, string captainsLog) {
		this.routeLog.Add(routeToAdd);
		Debug.Log("Add Route getting called 3...." + routeToAdd.theRoute[0].x + "  " + routeToAdd.theRoute[0].z);
		AddCargoManifest(playerShipVars.ship.cargo);
		AddOtherAttributes(playerShipVars, captainsLog, routeToAdd);
	}

	public void AddCargoManifest(Resource[] cargoToAdd) {
		string CSVstring = "";
		foreach (Resource resource in cargoToAdd) {
			CSVstring += "," + resource.amount_kg;
		}
		this.cargoLog.Add(CSVstring);
	}

	public void AddOtherAttributes(script_player_controls playerShipVars, string captainsLog, PlayerRoute currentRoute) {
		string CSVstring = "";
		Ship playerShip = playerShipVars.ship;
		//Add the applicable port docking info
		//If it isn't -1, then it's a port stop
		if (currentRoute.settlementID != -1) {
			CSVstring += "," + currentRoute.isLeaving + "," + currentRoute.settlementID + "," + currentRoute.settlementName;
		}
		else {
			CSVstring += "," + -1 + "," + -1 + "," + -1;
		}

		//Add the crewID's 
		CSVstring += ",";
		for (int index = 0; index < playerShip.crewRoster.Count; index++) {
			//Debug.Log ("ID: "  + playerShip.crewRoster[index].ID);
			CSVstring += playerShip.crewRoster[index].ID;
			if (index < playerShip.crewRoster.Count - 1)
				CSVstring += "_";
		}

		//Add the Unity XYZ coordinate of ship
		Vector3 playerLocation = playerShipVars.transform.position;
		CSVstring += "," + playerLocation.x + "_" + playerLocation.y + "_" + playerLocation.z;
		//Add the current questleg
		CSVstring += "," + playerShip.mainQuest.currentQuestSegment;
		//Add Ship HP
		CSVstring += "," + playerShip.health;
		//Add Player Clout
		CSVstring += "," + playerShip.playerClout;
		//Add Player Networks
		CSVstring += ",";
		for (int index = 0; index < playerShip.networks.Count; index++) {
			CSVstring += playerShip.networks[index];
			if (index < playerShip.networks.Count - 1)
				CSVstring += "_";
		}
		//Add Days Starving
		CSVstring += "," + playerShipVars.numOfDaysWithoutProvisions;
		//Add Days Thirsty
		CSVstring += "," + playerShipVars.numOfDaysWithoutWater;
		//Add currency
		CSVstring += "," + playerShip.currency;
		//Add Loan Amount Owed
		if (playerShip.currentLoan != null)
			CSVstring += "," + playerShip.currentLoan.amount;
		else
			CSVstring += ",-1";
		//Add Loan Origin City
		if (playerShip.currentLoan != null)
			CSVstring += "," + playerShip.currentLoan.settlementOfOrigin;
		else
			CSVstring += ",-1";
		//Add Current Navigator Target
		CSVstring += "," + playerShip.currentNavigatorTarget;
		//Add the list of known settlements in the player's acquired journal settlement knowledge
		CSVstring += ",";
		//Debug.Log (CSVstring);
		foreach (int settlementID in playerShip.playerJournal.knownSettlements)
			CSVstring += settlementID + "_";
		//remove trailing '_' from list of known settlements
		CSVstring = CSVstring.Remove(CSVstring.Length - 1);
		//Debug.Log ("After substring: " + CSVstring);
		//Add Captains Log: first we need to switch commas in the log to a "|" so it doesn't hurt the delimeters TODO This could be nicer but is fine for now until we get a better database setup
		//--also need tp scrub newlines
		string scrubbedLog = captainsLog.Replace(',', '^');
		scrubbedLog = scrubbedLog.Replace('\n', '*');
		CSVstring += "," + scrubbedLog;

		CSVstring += "," + playerShip.upgradeLevel;
		CSVstring += "," + playerShip.crewCapacity;
		CSVstring += "," + playerShip.cargo_capicity_kg;

		//Add a new row to match the route of all these attributes
		this.otherAttributes.Add(CSVstring);

	}

	public string ConvertJourneyLogToCSVText() {
		string CSVfile = "";
		//Ship playerShip = playerShipVars.ship;
		//Let's first add the headings to each column
		CSVfile += this.CSVheader;
		Debug.Log("Route Count: " + routeLog.Count);
		//first make sure the route isn't empty--if it is, then return a blank file
		if (routeLog.Count == 0) {
			CSVfile = "There is no player data to save currently";
		}
		else {

			//Loop through each route in the list and respectively the cargo(they will always be the same size so a single for loop index can handle both at once
			//	Each loop represents a single line in the CSV file
			for (int i = 0; i < routeLog.Count; i++) {
				//First add the player's unique machine ID--this will be different depending on the Operating System of the user, but will always be a consistent unique ID based on the users hardware(assuming there aren't major hardware changes)
				CSVfile += SystemInfo.deviceUniqueIdentifier + ",";
				//we're converting it to Web Mercator before saving it
				Vector3 mercator_origin = new Vector3((routeLog[i].theRoute[0].x * 1193.920898f) + (526320 - 0), (routeLog[i].theRoute[0].z * 1193.920898f) + (2179480 - 0), routeLog[i].theRoute[0].y);
				Vector3 mercator_end = new Vector3((routeLog[i].theRoute[1].x * 1193.920898f) + (526320 - 0), (routeLog[i].theRoute[1].z * 1193.920898f) + (2179480 - 0), routeLog[i].theRoute[1].y);

				Vector2 longXlatY_origin = CoordinateUtil.ConvertWebMercatorToWGS1984(new Vector2(mercator_origin.x, mercator_origin.y));
				Vector2 longXlatY_end = CoordinateUtil.ConvertWebMercatorToWGS1984(new Vector2(mercator_end.x, mercator_end.y));


				//TODO: This needs to be cleaned up below--the lat/long bit so it's not wasting resources on a pointless conversion above
				//TODO -- Seriously this XYZ / XZY business is a frankenstein monster of confusion--I can't even fathom in my current sleep deprived state why I'm having to put the y as a z here. My god. Someone Save me!
				//If this isn't a player travel route, but a port stop, then we don't need to worry about the conversion of of lat / long--it's already in lat long
				if (routeLog[i].settlementID != -1) {
					longXlatY_origin = new Vector2(routeLog[i].theRoute[0].x, routeLog[i].theRoute[0].z);
					longXlatY_end = Vector2.zero;

				}

				CSVfile += routeLog[i].timeStampInDays + "," + longXlatY_origin.x + "," + longXlatY_origin.y + "," + mercator_origin.z + "," +
					longXlatY_end.x + "," + longXlatY_end.y + "," + mercator_end.z;

				/*CSVfile += ((routeLog[i].theRoute[0].x * 1193.920898f) + (526320 - 0)) + "," + 
						   ((routeLog[i].theRoute[0].z * 1193.920898f) + (2179480 - 0)) + "," + 
						    routeLog[i].theRoute[0].y + "," +
						    
						   ((routeLog[i].theRoute[1].x * 1193.920898f) + (526320 - 0)) + "," + 
						   ((routeLog[i].theRoute[1].z * 1193.920898f) + (2179480 - 0)) + "," + 
						   routeLog[i].theRoute[1].y;
				*/



				//Add the Resources to the line record
				CSVfile += cargoLog[i];
				CSVfile += otherAttributes[i];


				//Add a newline if not on last route
				if (i != (routeLog.Count - 1)) {
					CSVfile += "\n";
					//Debug.Log ("Adding a NEW Line?");	
				}

				//Debug.Log (CSVfile);
			}
		}

		return CSVfile;
	}
}

[Serializable]
public class Resource : Model
{
	public const string Water = "Water";
	public const string Provisions = "Provisions";
	public const string Grain = "Grain";
	public const string Wine = "Wine";
	public const string Timber = "Timber";
	public const string Gold = "Gold";
	public const string Silver = "Silver";
	public const string Copper = "Copper";
	public const string Tin = "Tin";
	public const string Obsidian = "Obsidian";
	public const string Lead = "Lead";
	public const string Livestock = "Livestock";
	public const string Iron = "Iron";
	public const string Bronze = "Bronze";
	public const string PrestigeGoods = "Prestige Goods";

	public static readonly string[] All = new string[] { Water, Provisions, Grain, Wine, Timber, Gold, Silver, Copper, Tin, Obsidian, Lead, Livestock, Iron, Bronze, PrestigeGoods };

	[SerializeField] private string _name;
	public string name => _name;

	[SerializeField] private float _initial_amount_kg;
	public float initial_amount_kg { get => _initial_amount_kg; set { _initial_amount_kg = value; Notify(); } }

	[SerializeField] private float _amount_kg;
	public float amount_kg { get => _amount_kg; set { _amount_kg = value; Notify(); } }

	public Resource(string name, float amount_kg) {
		_name = name;
		Initialize(amount_kg);
	}

	public void Initialize(float amount_kg) {
		this.amount_kg = amount_kg;
		this.initial_amount_kg = amount_kg;
	}
}

// TODO: some instances of this are loaded from CSV (GameVars.captainsLogEntries). should separate static from serialized data
[Serializable]
public class CaptainsLogEntry
{
	public int settlementID;
	public string logEntry;
	public string dateTimeOfEntry;

	public CaptainsLogEntry(int settlementID, string logEntry) {
		this.settlementID = settlementID;
		this.logEntry = logEntry;
	}
}

// TODO: for now, the ship structure is only saved to JSON, it's not loaded from JSON at all aside from the ShipData field
// this is done for migration purposes to avoid needing to do the whole refactor at once.
[Serializable]
public class Ship : Model
{
	public const int StartingCrewCap = 10;		// 30
	public const int StartingCargoCap = 200;	// 1200
	public const int StartingWater = 50;		// 300
	public const int StartingFood = 50;			// 300
	public const int StartingCrewSize = 5;		// 30

	public string name;
	public float speed;
	public float cargo_capicity_kg;
	public Resource[] cargo;
	public int networkID;
	public List<CaptainsLogEntry> shipCaptainsLog;
	public Journal playerJournal;
	public int currentNavigatorTarget;
	public Loan currentLoan;
	public List<int> networks;
	public int originSettlement;

	public MainQuestLine mainQuest;

	// TODO: Reconcile mainQuest and objective concepts. These systems seem like they should be merged
	[SerializeField] private string _objective;
	public string objective { get => _objective; set { _objective = value; Notify(); } }

	public int crew => crewRoster.Count;

	[Serializable] public class CrewRosterList : SerializableObservableCollection<CrewMember, int>
	{
		public CrewRosterList() : base(crew => crew.ID, id => Globals.GameVars.GetCrewMemberFromID(id)) { }
	}
	public CrewRosterList crewRoster;

	[SerializeField] private float _totalNumOfDaysTraveled;
	public float totalNumOfDaysTraveled { get => _totalNumOfDaysTraveled; set { _totalNumOfDaysTraveled = value; Notify(); } }

	[SerializeField] private int _crewCapacity = StartingCrewCap;
	public int crewCapacity { get => _crewCapacity; set { _crewCapacity = value; Notify(); } }

	[SerializeField] private bool _sailsAreUnfurled = true;
	public bool sailsAreUnfurled { get => _sailsAreUnfurled; set { _sailsAreUnfurled = value; Notify(); } }

	[SerializeField] private int _upgradeLevel;
	public int upgradeLevel { get => _upgradeLevel; set { _upgradeLevel = value; Notify(); } }

	[SerializeField] private float _health;
	public float health { get => _health; set { _health = value; Notify(); } }

	[SerializeField] private string _builtMonuments = "";
	public string builtMonuments { get => _builtMonuments; set { _builtMonuments = value; Notify(); } }

	[SerializeField] private int _currency;
	public int currency { get => _currency; set { _currency = value; Notify(); } }

	public float _playerClout;
	public float playerClout { get => _playerClout; set { _playerClout = value; Notify(); } }

	public float CurrentCargoKg => cargo.Sum(c => c.amount_kg);

	public Resource GetCargoByName(string name) => cargo.FirstOrDefault(c => c.name == name);

	public Ship(string name, float speed, int health, float cargo_capcity_kg) {

		this.name = name;
		this.speed = speed;
		this.health = health;
		this.cargo_capicity_kg = cargo_capcity_kg;
		this.shipCaptainsLog = new List<CaptainsLogEntry>();
		this.crewRoster = new CrewRosterList();
		this.playerJournal = new Journal();

		cargo = Resource.All.Select(r => new Resource(r, 0f)).ToArray();
		GetCargoByName(Resource.Water).Initialize(100f);
		GetCargoByName(Resource.Provisions).Initialize(100f);

		this.currency = 500;
		this.crewCapacity = StartingCrewCap;
		this.playerClout = 50f;
		this.currentNavigatorTarget = -1;
		this.totalNumOfDaysTraveled = 0;
		this.networks = new List<int>();
		this.originSettlement = 246; //TODO Default set to Iolcus--Jason's hometown
		this.networks.Add(1);//TODO Default set to Samothrace network.
	}

	public float GetTotalCargoAmount() {
		float total = 0;
		foreach (Resource resource in cargo) {
			total += resource.amount_kg;
		}
		return total;
	}
}

#endregion

