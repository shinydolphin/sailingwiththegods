using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class World : MonoBehaviour
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
		Globals.Register(new Game());
		Globals.Register(new Database());

		Globals.Database.Init();

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

}///////// END OF FILE
