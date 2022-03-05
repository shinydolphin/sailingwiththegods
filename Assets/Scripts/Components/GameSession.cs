using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Only exists if a game is in progress.
/// </summary>
public class GameSession
{
	World World => Globals.World;
	QuestSystem Quests => Globals.Quests;
	Database Database => Globals.Database;
	Notifications Notifications => Globals.Notifications;
	GameUISystem UI => Globals.UI;

	public GameData data { get; set; }

	public CrewMember Jason => Crew.Jason;
	public IEnumerable<CrewMember> StandardCrew => Crew.StandardCrew;
	public IEnumerable<CrewMember> Pirates => Crew.Pirates;
	public IEnumerable<PirateType> PirateTypes => Crew.PirateTypes;
	public IEnumerable<CrewMember> AllNonCrew => Crew.AllNonCrew;

	// settlements
	public GameObject currentSettlementGameObject { get; set; }
	public Settlement currentSettlement { get; set; }
	public int currentPortTax { get; set; } = 0;        // this is derived from the currentSettlement. could be a getter on settlement object


	// ship
	public GameObject playerShip { get; private set; }
	public script_player_controls playerShipVariables { get; private set; }

	// captain's log
	private string currentCaptainsLog = "";
	private List<CaptainsLogEntry> currentLogPool = new List<CaptainsLogEntry>();
	public string CaptainsLog => currentCaptainsLog;

	// TODO: Delete once we are creating new sessions on restart and load
	public void ResetCaptainsLog(string content) {
		currentCaptainsLog = content;
	}

	public void AddToCaptainsLog(string message) {
		currentCaptainsLog = message + "\n\n" + currentCaptainsLog;
	}

	// game state
	public bool controlsLocked { get; set; } = false;
	public bool justLeftPort { get; set; } = false;
	public bool isPerformingRandomEvent { get; set; } = false;
	public bool isPassingTime { get; set; } = false;

	// TODO: Should really make all this game state stuff an actual state machine at some point
	public bool IsCutsceneMode = false;



	// high level game systems
	public Trade Trade { get; private set; }
	public Network Network { get; private set; }
	public Icons Icons { get; private set; }
	public Crew Crew { get; private set; }
	public bool isInNetwork => Network.CheckIfCityIDIsPartOfNetwork(currentSettlement.settlementID);

	//###################################
	//	GUI VARIABLES
	//###################################
	public bool showSettlementGUI { get; set; } = false;
	public bool showSettlementTradeButton { get; set; } = false;
	public bool showPortDockingNotification { get; set; } = false;
	public bool gameDifficulty_Beginner { get; set; } = false;
	public bool showNonPortDockButton { get; set; } = false;


	public GameSession() {
		playerShip = GameObject.FindGameObjectWithTag("playerShip");
		playerShipVariables = playerShip.GetComponent<script_player_controls>();

		// must be after playerShipVariables found
		ResetGameData();

		// must be after csv are loaded
		Network = new Network(this, Database);
		Trade = new Trade();
		Crew = new Crew(Database.masterCrewList, Database.masterPirateTypeList, playerShipVariables.ship);

		World.CreateSettlementsFromList();
		currentSettlementGameObject = World.settlement_masterList_parent.transform.GetChild(0).gameObject;
		currentSettlement = currentSettlementGameObject.GetComponent<SettlementComponent>().thisSettlement;
		//The lights are on at the start, so turn them off or they'll be on during the first day and no other day
		World.cityLightsParent.SetActive(false);

		//Load the basic log entries into the log pool
		AddEntriesToCurrentLogPool(0);
		StartPlayerShipAtOriginCity();
		World.GenerateCityLights();
	}
	
	// this pulls the references to the ship and journey log
	public void ResetGameData() {
		data = GameData.New(playerShipVariables.ship, playerShipVariables.journey);
	}

	public void ActivateNavigatorBeacon(Beacon beacon, Vector3 location) {
		beacon.IsBeaconActive = true;
		beacon.transform.position = location;
		beacon.GetComponent<LineRenderer>().SetPosition(0, new Vector3(location.x, 0, location.z));
		beacon.GetComponent<LineRenderer>().SetPosition(1, location + new Vector3(0, 400, 0));
		playerShipVariables.UpdateNavigatorBeaconAppearenceBasedOnDistance(beacon);
	}

	public void DeactivateNavigatorBeacon(Beacon beacon) {
		beacon.IsBeaconActive = false;
	}

	public void RotateCameraTowards(Vector3 target) {
		CameraLookTarget = target;
	}

	public Vector3? CameraLookTarget;

	void UpdateCameraRotation() {

		if (CameraLookTarget.HasValue) {

			var camToTarget = CameraLookTarget.Value - World.FPVCamera.transform.parent.parent.transform.position;
			var angle = Vector3.SignedAngle(World.FPVCamera.transform.parent.parent.forward, camToTarget.normalized, Vector3.up);

			World.FPVCamera.transform.parent.parent.RotateAround(World.FPVCamera.transform.parent.parent.position, World.FPVCamera.transform.parent.parent.up, angle * Time.deltaTime);

			if (Mathf.Abs(angle) < 2f) {
				CameraLookTarget = null;
			}

		}

	}

	public void Update() {
		UpdateCameraRotation();
		DebugHotkeys();
	}

	void DebugHotkeys() {
		// uncomment to use debug hotkeys
#if UNITY_EDITOR
		if(Input.GetKeyUp(KeyCode.E)) {
			UI.Show<InfoScreen, InfoScreenModel>(new InfoScreenModel { Message = " Panda" });
		}

		//if(Input.GetKeyUp(KeyCode.E)) {
		//	var storm = new StormAtSea();
		//	storm.Init(this, playerShipVariables.ship, new ShipSpeedModifiers(), playerShip.transform, 1);
		//	storm.Execute();
		//}
#endif
	}

	//====================================================================================================
	//      PLAYER INITIALIZATION FUNCTIONS
	//====================================================================================================

	void StartPlayerShipAtOriginCity() {
		//first set the origin city to the first available as a default
		//GameObject originCity = World.settlement_masterList_parent.transform.GetChild(0).gameObject;
		foreach (Transform child in World.settlement_masterList_parent.transform) {
			//if the settlement we want exists, then use it as the default instead
			if (child.name == "Samothrace") {
				//originCity = child.gameObject;
				break;
			}
		}
		//now set the player ship to the origin city coordinate
		//!TODO This is arbotrarily set to samothrace right now
		playerShip.transform.position = new Vector3(1702.414f, .23f, 2168.358f);
		//mainCamera.transform.position = new Vector3(originCity.transform.position.x, 30f, originCity.transform.position.z);
	}


	public void UpgradeShip(int costToBuyUpgrade) {
		playerShipVariables.ship.upgradeLevel = 1;
		playerShipVariables.ship.currency -= costToBuyUpgrade;

		// TODO: These should be defined per uprade level, but until we have a better idea how upgrades will work long term, just hard here
		playerShipVariables.ship.crewCapacity = 30;
		playerShipVariables.ship.cargo_capicity_kg = 1200;

		UI.Hide<RepairsView>();

		// this will automatically add the story crew that was previously being added manually in a hack
		Quests.CheckUpgradeShipTriggers();

		// show the new ship model
		SetShipModel(playerShipVariables.ship.upgradeLevel);
		controlsLocked = true;
	}

	// TODO: Only public because RestartGame uses it. If we make the game session completely deleted we could have the Destroy reset the ship model or something.
	public void SetShipModel(int shipLevel) {
		foreach (var upgradeLevel in World.shipLevels) {
			upgradeLevel.SetActive(false);
		}
		World.shipLevels[shipLevel].SetActive(true);
	}


	//====================================================================================================
	//    PLAYER MODIFICATION FUNCTIONS
	//====================================================================================================   
	
	/// <summary>
	/// Changes the player's clout score
	/// </summary>
	/// <param name="cloutAdjustment">Change in the clout</param>
	/// <param name="useMod">If you are writing new code, set this to false</param>
	public void AdjustPlayerClout(int cloutAdjustment, bool useMod = true) {
		int cloutModifier = useMod ? 100 : 1; //We have a modifier to help link the new system in with the old functions.
		int clout = (int)playerShipVariables.ship.playerClout;
		//adjust the players clout by the given amount
		playerShipVariables.ship.playerClout += (cloutAdjustment * cloutModifier);
		//if the player's clout exceeds 100 after the adjustment, then reduce it back to 100 as a cap
		if (playerShipVariables.ship.playerClout > 5000)
			playerShipVariables.ship.playerClout = 5000;
		//if the player's clout is reduced below 0 after the adjustment, then increase it to 0 again
		if (playerShipVariables.ship.playerClout < 0)
			playerShipVariables.ship.playerClout = 0;
		Debug.Log("Clout " + playerShipVariables.ship.playerClout);
		//First check if a player reaches a new clout level
		//If the titles don't match after adjustment then we have a change!
		if (Database.GetCloutTitleEquivalency(clout) != Database.GetCloutTitleEquivalency((int)playerShipVariables.ship.playerClout)) {
			//Next we need to determine whether or not it was a level down or level up
			//If it was an increase then show a positive message
			if (clout < (clout + cloutAdjustment)) {
				Debug.Log("Gained a level: " + clout);
				if (clout >= 4000)
				{
			        Debug.Log("Winning the game");
			        UI.EndingScreen.SetActive(true);
				}
				else
				{
				    Notifications.ShowANotificationMessage("Congratulations! You have reached a new level of influence! Before this day you were Jason, " + Database.GetCloutTitleEquivalency(clout) + ".....But now...You have become Jason " + Database.GetCloutTitleEquivalency((int)playerShipVariables.ship.playerClout) + "!");
				}
			}
			else {
				Debug.Log("Lost a level");
				Notifications.ShowANotificationMessage("Unfortunately you sunk to a new low level of respect in the world! Before this day you were Jason, " + Database.GetCloutTitleEquivalency(clout) + ".....But now...You have become Jason " + Database.GetCloutTitleEquivalency((int)playerShipVariables.ship.playerClout) + "!");
			}
		}


	}

	public void AdjustCrewsClout(int cloutAdjustment) {
		foreach (CrewMember crew in playerShipVariables.ship.crewRoster) {
			//adjust the crews clout by the given amount
			crew.clout += cloutAdjustment;
			//if the crew's clout exceeds 100 after the increase, then reduce it back to 100 as a cap
			if (crew.clout > 5000)
				crew.clout = 5000;
			//if the crew's clout is reduced below 0 after the adjustment, then increase it to 0 again
			if (crew.clout < 0)
				crew.clout = 0;
		}

	}

	public void AdjustPlayerShipHealth(int healthAdjustment) {
		//adjust the health by the given amount
		playerShipVariables.ship.health += healthAdjustment;
		//if the health exceeds 100 after the adjustment, then reduce it back to 100 as a cap
		if (playerShipVariables.ship.health > 100)
			playerShipVariables.ship.health = 100;
		//if the health is reduced below 0 after the adjustment, then increase it back to 0
		if (playerShipVariables.ship.health < 0)
			playerShipVariables.ship.health = 0;
	}

	public void AddEntriesToCurrentLogPool(int logID) {
		for (int i = 0; i < Database.captainsLogEntries.Length; i++) {
			if (Database.captainsLogEntries[i].settlementID == logID) {
				currentLogPool.Add(Database.captainsLogEntries[i]);
			}
		}
	}

	public void RemoveEntriesFromCurrentLogPool(int logID) {
		currentLogPool.RemoveAll(entry => entry.settlementID == logID);
	}

	public CaptainsLogEntry GetRandomCaptainsLogFromPool() => currentLogPool.RandomElement();



	//====================================================================================================
	//    OTHER FUNCTIONS
	//====================================================================================================   

	public float GetOverallCloutModifier(int settlementID) {
		//This is the main function that processes ALL clout-based modifiers and returns a floating point value 0-1
		//	--to represent the influence level the player has at any particular moment in an interaction. This
		//	--interaction might be through the buying and selling of goods, interactions with other settlements,
		//	--or interactions with pirates and random events.
		float finalModifier = 0;

		float playerClout = 0;
		float calculatedCrewClout = 0;
		float playerNetworkModifier = 0;
		float playerOriginNetworkModifier = 0;
		float crewMembersNetworkModifier = 0;

		//###### First get the player's clout and convert it to a 0-100 value
		playerClout = playerShipVariables.ship.playerClout;

		playerClout = (int)Math.Floor((playerClout / 5000) * 100); //5000 is the cap so we divide the current amount to get the 0/1 ratio

		//###### Next we need to cycle through all the crew members and tally up the clout there
		//	--This will be a 1 - 100 value that is a sum of of percentage of total possible clout
		//	--e.g. if there are 10 crew members, the total possible clout is 50,000--if it adds
		//	--up to 25,000, the returned clout is 50--or 50%
		float sumOfCrewClout = 0;
		foreach (CrewMember member in playerShipVariables.ship.crewRoster) {
			sumOfCrewClout += member.clout;
		}
		//Here's where we divide the sum by the total possible clout on board the ship--clout will ALWAYS be between 1 - 100
		calculatedCrewClout = sumOfCrewClout / (playerShipVariables.ship.crewRoster.Count * 100);

		//###### Now we need to determine whether or not the current city (or representative thereof) 
		//	--is part of the player's individual network--if it is the value is 100, otherwise it's 50.
		//	--the player's individual network is ALL the settlements he/she knows of in their knowledgebase--not necessarily IN it's main attached network like the Samothrace network of influence
		foreach (int playerNetworkID in playerShipVariables.ship.playerJournal.knownSettlements) {
			if (playerNetworkID == settlementID) {
				playerNetworkModifier = 100f;
			}
			else {
				playerNetworkModifier = 0f;
			}
		}

		//###### Now we need to determine whether or not the current city (or representative thereof) 
		//	--is part of the player's hometown/main network(s), e.g. if the player and city is part of the Samothracian network
		Debug.Log("DEBUG SETTLEMENT ID:  " + settlementID);
		//If there is no city attached(settlementID == 0) then we are in open waters so return 0
		if (settlementID != 0) {
			foreach (int playerNetworkID in playerShipVariables.ship.networks) {
				foreach (int settlementNetID in Database.GetSettlementFromID(settlementID).networks) {
					//If we find a network match through the network ID then return the city's population to represent its influence in the network
					//	--if we don't find a match, a value of 0 is returned			
					if (playerNetworkID == settlementNetID) {
						playerOriginNetworkModifier = Database.GetSettlementFromID(settlementID).population;
					}
					else {
						playerOriginNetworkModifier = 0f;
					}
				}

			}
		}
		else {
			playerNetworkModifier = 0f;
		}

		//###### Now let's find out if ANY of the crewmembers' origin towns are connected with this settlement ID of interest
		//	--If there is a member, then return a value of 50, otherwise a value of 0. This value (50) is less than the
		//	--players home town/network(s), because the player is the captain--and assumedly has more influence in that sense.
		//If there is no city attached(settlementID == 0) then we are in open waters so return 0
		if (settlementID != 0) {
			foreach (CrewMember member in playerShipVariables.ship.crewRoster) {
				foreach (int crewNetworkID in Database.GetSettlementFromID(member.originCity).networks) {
					foreach (int settlementNetID in Database.GetSettlementFromID(settlementID).networks) {
						//If we find a network match through the network ID then return the city's population / 2 to represent its influence in the network
						//	--if we don't find a match, a value of 0 is returned			
						if (crewNetworkID == settlementNetID) {
							crewMembersNetworkModifier = Database.GetSettlementFromID(settlementID).population / 2f;
						}
						else {
							crewMembersNetworkModifier = 0f;
						}
					}
				}

			}
		}

		//###### Now that we have all of our clout values, the sum total will be out of 500 possible points
		//	--the player's own clout is weighted at double, the crew modifiers are weighted both at half, and the players network modifers are weighted as normal (1)
		finalModifier = (playerClout * 2) + (calculatedCrewClout / 2) + playerNetworkModifier + playerOriginNetworkModifier + (crewMembersNetworkModifier / 2);
		finalModifier /= 500;
		//We return the final percentage point modifier. It will be between 0-1
		return finalModifier;

	}


	public bool CheckIfShipBackAtLoanOriginPort() {
		bool isAtPort = false;
		if (playerShipVariables.ship.currentLoan.settlementOfOrigin == currentSettlement.settlementID) {
			isAtPort = true;
		}
		else {
			isAtPort = false;
		}

		return isAtPort;


	}















}
