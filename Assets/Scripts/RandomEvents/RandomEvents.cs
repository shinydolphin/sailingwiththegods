using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public static class RandomEvents
{
	static World World => Globals.World;
	static GameSession Session => Globals.Game.Session;
	static Notifications Notifications => Globals.Notifications;

	//#########################################################################################################
	//	RANDOM  EVENT  FUNCTION
	//=========================
	//		--This function determines whether or not a random event will happen to the ship. Regardless of 
	//		--whether or not a random event occurs, it will trigger journal messages based on whether or not
	//		--the ship is in open sea or near a sphere of influence of a settlement/location of interest
	//
	//#########################################################################################################


	// it makes sense for this to have access to the ship, the ship's movement data and position, and the crew, as well as things like clout
	// doesn't need access to things like the GUI
	public static void WillARandomEventHappen(Ship ship, ShipSpeedModifiers shipSpeedModifiers, Transform shipTransform) {


		//Random Events have a chance to occur every half day of travel
		//-------------------------------------------------------------
		//These values help determine the half day of travel
		float tenthPlaceTemp = (ship.totalNumOfDaysTraveled - Mathf.FloorToInt(ship.totalNumOfDaysTraveled));
		tenthPlaceTemp *= 10;
		//Debug.Log (tenthPlaceTemp + "  " + hundredthPlaceTemp);

		const int noon = 5;
		const int midnight = 9;

		//If we are at a half day's travel, then see if a random event occurs
		if ((Mathf.FloorToInt(tenthPlaceTemp) == noon || Mathf.FloorToInt(tenthPlaceTemp) == midnight /*|| Mathf.FloorToInt(tenthPlaceTemp) ==1 || Mathf.FloorToInt(tenthPlaceTemp) ==3 || Mathf.FloorToInt(tenthPlaceTemp) ==7*/) && !Session.isPerformingRandomEvent) {
			Session.isPerformingRandomEvent = true;
			float chanceOfEvent = .95f; //0 - 1 value representing chance of a random event occuring
										//We determine if the 
			if (Random.Range(0f, 1f) <= chanceOfEvent) {
				//Debug.Log ("Triggering Random Event");
				//When we trigger a random event, let's make the ship drop anchor!
				Session.playerShipVariables.rayCheck_stopShip = true;

				//We separate Random events into two possible categories: Positive, and Negative.
				//First we need to determine if the player has a positive or negative event occur
				//--The basic chance is a 50/50 chance of either or, but we need to figure out if the
				//--crew makeup has any augers, and if so, each auger decreases the chance of a negative (now controlled by PostiveEvent modifiers)
				//--event by 10%. We then roll an aggregate clout score to further reduce the chance by a maximum of 20%

				//Get the 0-1 aggregate clout score. Here we use the current zone of influence's network id to check
				int currentZoneID = 0;
				//TODO Right now this just uses the relevant city's ID to check--but in the aggregate score function--it should start using the networks--not the city.
				if (World.activeSettlementInfluenceSphereList.Count > 0) currentZoneID = World.activeSettlementInfluenceSphereList[0];
				float aggregateCloutScore = Session.GetOverallCloutModifier(currentZoneID);
				//Now determine the final weighted chance score that will be .5f and under
				chanceOfEvent = .5f - ship.crewRoster.Sum(c => c.changeOnHire.PositiveEvent / 100f) - (.2f * aggregateCloutScore);


				//If we roll under our range, that means we hit a NEGATIVE random event
				//Clout is clamped to prevent positive events from giving 0 resource as a reward, for example
				float clampedClout = Mathf.Clamp(aggregateCloutScore, 0.1f, 1f);
				if (Random.Range(0f, 1f) <= chanceOfEvent) {
					ExecuteEvent(GetSubclassesOfType<NegativeEvent>(), ship, shipSpeedModifiers, shipTransform, clampedClout);
				}
				else {
					ExecuteEvent(GetSubclassesOfType<PositiveEvent>(), ship, shipSpeedModifiers, shipTransform, clampedClout);
				}

			}
			//If we do or don't get a random event, we should always get a message from the crew--let's call them tales
			//here they describe things like any cities nearby if the crew is familiar or snippets of greek mythology, or they
			//may be from a list of messages concering any nearby zones of influence from passing settlements/locations of interest
			var log = Session.GetRandomCaptainsLogFromPool();
			if(log != null) {
				ship.shipCaptainsLog.Add(log);
				log.dateTimeOfEntry = ship.totalNumOfDaysTraveled + " days";
				Session.AddToCaptainsLog(log.dateTimeOfEntry + "\n" + log.logEntry);
			}
		}


		//let's make sure the trigger for a new log  / event doesn't happen again until needed by
		//	by turning it off when the the trigger number changes--which means it won't take effect
		//	again until the next time the trigger number occurs
		//Debug.Log (Mathf.FloorToInt(tenthPlaceTemp));
		if (Mathf.FloorToInt(tenthPlaceTemp) != noon && Mathf.FloorToInt(tenthPlaceTemp) != midnight /*&& Mathf.FloorToInt(tenthPlaceTemp) !=1 && Mathf.FloorToInt(tenthPlaceTemp) !=3 && Mathf.FloorToInt(tenthPlaceTemp) !=7*/) 		Session.isPerformingRandomEvent = false;

	}

	public abstract class PositiveEvent : Event { }
	public abstract class NegativeEvent : Event { }
	public abstract class Event
	{
		protected World World => Globals.World;
		protected GameSession Session => Globals.Game.Session;
		protected Database Database => Globals.Database;
		protected Notifications Notifications => Globals.Notifications;
		protected MiniGames MiniGames => Globals.MiniGames;

		protected Ship ship { get; private set; }
		protected ShipSpeedModifiers shipSpeedModifiers { get; private set; }
		protected Transform shipTransform { get; private set; }
		protected float aggregateCloutScore { get; private set; }

		public void Init(Ship ship, ShipSpeedModifiers shipSpeedModifiers, Transform shipTransform, float aggregateCloutScore) {
			this.ship = ship;
			this.shipSpeedModifiers = shipSpeedModifiers;
			this.shipTransform = shipTransform;
			this.aggregateCloutScore = aggregateCloutScore;
		}

		public virtual bool isValid() {
			return true;
		}

		public virtual float Weight() {
			return 1f;
		}

		public abstract void Execute();

		// HELPER FUNCTIONS THAT MAY BE USEFUL FOR ALL SUBCLASSES

		protected static CrewMember RemoveRandomCrewMember(Ship ship) {
			//Find a random crewmember to kill if they can be killed

			CrewMember killedMate = new CrewMember(-1);
			List<int> listOfPossibleCrewToKill = new List<int>();

			//generate a list of possible crew that can be killed
			for (int i = 0; i < ship.crewRoster.Count; i++) {
				if (ship.crewRoster[i].isKillable) listOfPossibleCrewToKill.Add(i);
			}

			//if we don't find any available crewmembers to kill, return an empty crewman as a flag that none exist to be killed
			if (listOfPossibleCrewToKill.Count != 0) {
				int randomMemberToKill = listOfPossibleCrewToKill[Random.Range(0, listOfPossibleCrewToKill.Count - 1)];
				//Store the crewman in a temp variable
				killedMate = ship.crewRoster[randomMemberToKill];
				//Remove the crewmember
				ship.crewRoster.Remove(killedMate);
				//return the removed crewmember
				return killedMate;


			}

			//If there are no available members then just return a null flagged member initialize in the beggining of this function
			return killedMate;
		}
	}

	static IEnumerable<System.Type> GetSubclassesOfType<T>() {
		return System.Reflection.Assembly.GetAssembly(typeof(T))
			.GetTypes()
			.Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T)));
	}

	static Event CreateEvent(System.Type eventType, Ship ship, ShipSpeedModifiers shipSpeedModifiers, Transform shipTransform, float aggregateCloutScore) {
		var result = System.Activator.CreateInstance(eventType) as Event;
		result.Init(ship, shipSpeedModifiers, shipTransform, aggregateCloutScore);
		return result;
	}

	static void ExecuteEvent(IEnumerable<System.Type> options, Ship ship, ShipSpeedModifiers shipSpeedModifiers, Transform shipTransform, float aggregateCloutScore) {
		
		IEnumerable<Event> events = options.Select(type => CreateEvent(type, ship, shipSpeedModifiers, shipTransform, aggregateCloutScore));

		IEnumerable<Event> filteredEvents = events.Where(evnt => evnt.isValid() == true);

		//calls for an event/ mini game to play wiht a higher chance that minigames will happen due to their higher Weight() return value
		var eventObj = filteredEvents.WeightedRandomElement(filteredEvents.Select(element => element.Weight()));

		if (eventObj != null) {
			eventObj.Execute();
		}
		else {
			Debug.Log("EVENT WAS NULL.");
		}
	}


}
