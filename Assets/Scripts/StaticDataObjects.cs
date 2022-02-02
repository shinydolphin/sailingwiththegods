using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/*
 * This file contains data objects that are used in CSVLoader to read non-changing (static data) from the CSV files.
 * It differs from DataObjects, which contains serialized data objects for player save data and game state (dynamic data).
 * 
 * Note: Some things are not actually non-changing right now. For example a Settlement's available crew list is modified at runtime. Search ObservableCollection for some of them.
 * TODO: This should be separated at some point so that the state is a separate object that just has a reference to its static definition (see MetaResource and Resource)
 * 
 * TODO: Ideally, these would all be immutable.
 */

public class QuestSegment
{
	#region Triggers
	public abstract class Trigger
	{
		public abstract TriggerType Type { get; }
	}

	public class CityTrigger : Trigger
	{
		public override TriggerType Type => TriggerType.City;

		public readonly int DestinationId;

		public CityTrigger(int destinationId) : base() {
			DestinationId = destinationId;
		}
	}

	public class CoordTrigger : Trigger
	{
		public override TriggerType Type => TriggerType.Coord;

		public readonly Vector2 LongXLatY;

		public CoordTrigger(Vector2 longXLatY) : base() {
			LongXLatY = longXLatY;
		}
	}

	public class UpgradeShipTrigger : Trigger
	{
		public override TriggerType Type => TriggerType.UpgradeShip;
	}

	public class NoneTrigger : Trigger
	{
		public override TriggerType Type => TriggerType.None;
	}

	public enum TriggerType
	{
		None,
		City,
		Coord,
		UpgradeShip
	}
	#endregion

	#region Arrival Events

	public abstract class ArrivalEvent
	{
		protected QuestSystem Quests => Globals.Quests;
		protected UISystem UI => Globals.UI;

		protected QuestSegment Segment { get; private set; }
		public abstract ArrivalEventType Type { get; }
		public abstract void Execute(QuestSegment segment);
	}

	public class MessageArrivalEvent : ArrivalEvent
	{
		public override ArrivalEventType Type => ArrivalEventType.Message;

		public override void Execute(QuestSegment segment) {

			UI.Show<QuestScreen, QuizScreenModel>(new QuizScreenModel(
				title: QuestSystem.QuestMessageIntro,
				message: Message,
				caption: segment.caption,
				icon: segment.image,
				choices: new ObservableCollection<ButtonViewModel> {
					new ButtonViewModel { Label = "OK", OnClick = () => UI.Hide<QuestScreen>() }
				}
			));

			Quests.CompleteQuestSegment(segment);
		}

		public readonly string Message;

		public MessageArrivalEvent(string message) {
			Message = message;
		}
	}

	public class QuizArrivalEvent : ArrivalEvent
	{
		public override ArrivalEventType Type => ArrivalEventType.Quiz;

		readonly string QuizName;

		public override void Execute(QuestSegment segment) {
			Quizzes.QuizSystem.StartQuiz(QuizName, () => Quests.CompleteQuestSegment(segment));
		}

		public QuizArrivalEvent(string quizName) {
			QuizName = quizName;
		}
	}

	public class NoneArrivalEvent : ArrivalEvent
	{
		public override ArrivalEventType Type => ArrivalEventType.None;

		// just immediately start the next quest with no additional popups
		public override void Execute(QuestSegment segment) {
			Quests.CompleteQuestSegment(segment);
		}
	}

	public enum ArrivalEventType
	{
		None,
		Message,
		Quiz
	}

	#endregion

	public int segmentID;
	public Trigger trigger;
	public bool skippable;
	public string objective;
	public bool isFinalSegment;
	public List<int> crewmembersToAdd;
	public List<int> crewmembersToRemove;
	public string descriptionOfQuest;
	public ArrivalEvent arrivalEvent;
	public List<int> mentionedPlaces;
	public Sprite image;
	public string caption;

	public QuestSegment(int segmentID, Trigger trigger, bool skippable, string objective, string descriptionOfQuest, ArrivalEvent arrivalEvent, List<int> crewmembersToAdd, List<int> crewmembersToRemove, bool isFinalSegment, List<int> mentionedPlaces, Sprite image, string caption) {
		this.segmentID = segmentID;
		this.trigger = trigger;
		this.skippable = skippable;
		this.objective = objective;
		this.descriptionOfQuest = descriptionOfQuest;
		this.arrivalEvent = arrivalEvent;
		this.crewmembersToAdd = crewmembersToAdd;
		this.crewmembersToRemove = crewmembersToRemove;
		this.isFinalSegment = isFinalSegment;
		this.mentionedPlaces = mentionedPlaces;
		this.image = image;
		this.caption = caption;
	}
}

public class Region
{
	public string Name;
	public string Description;
}

public class Settlement
{

	public int settlementID;
	public Vector2 location_longXlatY;
	public string name;
	public int population;
	public float elevation;
	public Resource[] cargo;
	public float tax_neutral;
	public float tax_network;
	public GameObject theGameObject;
	public Vector3 adjustedGamePosition;
	public float eulerY;
	public int typeOfSettlement;
	public string description;
	public List<int> networks;
	public ObservableCollection<CrewMember> availableCrew;
	public string prefabName;

	// tax factors
	public bool godTax;
	public int godTaxAmount;
	public bool transitTax;
	public float transitTaxPercent;
	public bool foreignerFee;
	public float foreignerFeePercent;
	public float ellimenionPercent;

	public string coinText;
	public Region Region;

	public Resource GetCargoByName(string name) => cargo.FirstOrDefault(c => c.name == name);
	public Resource GetCargo(Resource r) => cargo.FirstOrDefault(c => c.name == r.name);

	public Settlement(int settlementID, string name, Vector2 location_longXlatY, float elevation, int population) {
		this.settlementID = settlementID;
		this.location_longXlatY = location_longXlatY;
		this.elevation = elevation;
		this.name = name;
		this.population = population;
		cargo = Resource.All.Select(r => new Resource(r, 0f)).ToArray();
		networks = new List<int>();
		availableCrew = new ObservableCollection<CrewMember>();
	}

	//This is a debug class to make a blank settlement for testing
	public Settlement(int id, string name, int networkID) {
		this.settlementID = id;
		this.name = name;
		this.population = 0;
		this.elevation = 0;
		this.tax_network = 0;
		this.tax_neutral = 0;
		this.description = "FAKE SETTLEMENT--LOOK INTO THIS ERROR";
		this.networks = new List<int>();
		availableCrew = new ObservableCollection<CrewMember>();

	}

	override public string ToString() {
		string mString = this.name + ":\n" + "Population: " + population + "\n\n RESOURCES \n";
		for (int i = 0; i < this.cargo.Length; i++) {
			mString += this.cargo[i].name + ":  " + this.cargo[i].amount_kg + "kg\n";
		}

		return mString;
	}

}

public class MetaResource
{
	public string name;
	public int id;
	public int trading_priority;
	public string description;
	public string icon;

	public MetaResource(string name, int id, string description, string icon, int trading_priority) {
		this.name = name;
		this.id = id;
		this.description = description;
		this.icon = icon;
		this.trading_priority = trading_priority;
	}

}

public class CurrentRose
{
	public float direction;
	public float speed;

	public CurrentRose(float direction, float speed) {
		this.direction = direction;
		this.speed = speed;

	}
}

public class WindRose
{
	public float direction;
	public float speed;

	public WindRose(float direction, float speed) {
		this.direction = direction;
		this.speed = speed;

	}
}

public enum CrewType
{
	Sailor = 0,
	Warrior = 1,
	Slave = 2,
	Passenger = 3,
	Navigator = 4,
	Guide = 5,
	Assistant = 6,
	Royalty = 7,
	Seer = 8,
	Lawyer = 9
}

public class PirateType
{
	public int ID;
	public string name;
	public int difficulty;
}

public class CrewMember
{
	GameSession Session => Globals.Game.Session;

	public int ID;
	public string name;
	public int originCity;
	public int clout;
	public string backgroundInfo;
	public bool isKillable;
	public bool isPartOfMainQuest;
	public CrewType typeOfCrew;
	public bool isPirate;
	public PirateType pirateType;

	public bool isJason => name == "Jason";

	SkillModifiers _changeOnHire;
	public SkillModifiers changeOnHire { get { if (_changeOnHire == null) InitChangeOnHire(Session); return _changeOnHire; } }

	SkillModifiers _changeOnFire;
	public SkillModifiers changeOnFire { get { if (_changeOnFire == null) InitChangeOnFire(Session); return _changeOnFire; } }

	SkillModifiers _currentContribution;
	public SkillModifiers currentContribution { get { if (_currentContribution == null) InitCurrentContribution(Session); return _currentContribution; } }

	//0= sailor  1= warrior  2= slave  3= passenger 4= navigator 5= auger
	//A sailor is the base class--no benefits/detriments
	//	--navigators provide maps to different settlements and decrease negative random events
	//	--warriors make sure encounters with pirates or other raiding activities go better in your favor
	//	--slaves have zero clout--few benefits--but they never leave the ship unless they die
	public CrewMember(int ID, string name, int originCity, int clout, CrewType typeOfCrew, string backgroundInfo, bool isKillable, bool isPartOfMainQuest, bool isPirate, PirateType pirateType) {
		this.ID = ID;
		this.name = name;
		this.originCity = originCity;
		this.clout = clout;
		this.typeOfCrew = typeOfCrew;
		this.backgroundInfo = backgroundInfo;
		this.isKillable = isKillable;
		this.isPartOfMainQuest = isPartOfMainQuest;
		this.isPirate = isPirate;
		this.pirateType = pirateType;
	}

	//This is a helper class to create a void crewman
	public CrewMember(int id) {
		ID = id;
		_changeOnHire = new SkillModifiers();
		_changeOnFire = new SkillModifiers();
		_currentContribution = new SkillModifiers();
	}

	void InitChangeOnHire(GameSession session) {
		_changeOnHire = new SkillModifiers {
			CitiesInNetwork = session.Network.GetCrewMemberNetwork(this).Count(s => !session.Network.MyCompleteNetwork.Contains(s)),
			BattlePercentChance = typeOfCrew == CrewType.Warrior ? 5 : 0,
			Navigation = typeOfCrew == CrewType.Sailor ? 1 : 0,
			PositiveEvent = typeOfCrew == CrewType.Guide ? 10 : 0
		};
	}

	void InitChangeOnFire(GameSession session) {
		// the cities in network calculation is too expensive right now. disabled temporarily
		_changeOnFire = new SkillModifiers {
			CitiesInNetwork = -session.Network.GetCrewMemberNetwork(this).Count(s => !session.Network.CrewMembersWithNetwork(s).Any(crew => crew != this) && !session.Network.MyImmediateNetwork.Contains(s)),
			BattlePercentChance = typeOfCrew == CrewType.Warrior ? -5 : 0,
			Navigation = typeOfCrew == CrewType.Sailor ? -1 : 0,
			PositiveEvent = typeOfCrew == CrewType.Guide ? -10 : 0
		};
	}

	void InitCurrentContribution(GameSession session) {
		// very similar to changeOnFire, but shows it as positives. this is their contribution to your team, not what you'll lose if you fire them (but it's basically the same).
		_currentContribution = new SkillModifiers {
			CitiesInNetwork = session.Network.GetCrewMemberNetwork(this).Count(s => !session.Network.CrewMembersWithNetwork(s).Any(crew => crew != this) && !session.Network.MyImmediateNetwork.Contains(s)),
			BattlePercentChance = typeOfCrew == CrewType.Warrior ? 5 : 0,
			Navigation = typeOfCrew == CrewType.Sailor ? 1 : 0,
			PositiveEvent = typeOfCrew == CrewType.Guide ? 10 : 0
		};
	}

}