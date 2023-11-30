using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class TavernCityViewModel : CityViewModel
{
	private DialogScreen ds;
	public int CostForHint {
		get {
			float initialCost = CoordinateUtil.GetDistanceBetweenTwoLatLongCoordinates(Session.currentSettlement.location_longXlatY, City.location_longXlatY) / 10000f;
			return Mathf.RoundToInt(initialCost - (initialCost * Session.GetOverallCloutModifier(City.settlementID)));
		}
	}

	public int CostToHire {
		get {
			float initialCost = CoordinateUtil.GetDistanceBetweenTwoLatLongCoordinates(Session.currentSettlement.location_longXlatY, City.location_longXlatY) / 1000f;
			return Mathf.RoundToInt(initialCost - (initialCost * Session.GetOverallCloutModifier(City.settlementID)));
		}
	}
	
	public DialogScreen GetDS {
		get { return ds; }
	}

	public TavernCityViewModel(GameSession session, Settlement city, DialogScreen d) : base(session, city, null) 
	{
		ds = d;
	}

	public string GetInfoOnNetworkedSettlementResource(Resource resource) {
		if (resource.amount_kg < 100)
			return "I hear they are running inredibly low on " + resource.name;
		else if (resource.amount_kg < 300)
			return "Someone mentioned that they have modest stores of " + resource.name;
		else
			return "A sailor just came from there and said he just unloaded an enormous quantity of " + resource.name;

	}

	// User for Trading Goods. This is getting resource from city.
	public void GUI_BuyHint() {

		if (Session.playerShipVariables.ship.currency < CostForHint) {
			Notifications.ShowANotificationMessage("Not enough money to buy this information!");
		}
		else {
			Session.playerShipVariables.ship.currency -= CostForHint;
			Notifications.ShowANotificationMessage(GetInfoOnNetworkedSettlementResource(City.cargo[UnityEngine.Random.Range(0, City.cargo.Length)]));
		}

	}
	// NOT USED. Navigator currently set in YarnTavern.cs
	public void GUI_HireANavigator() {
		//Do this if button pressed
		//Check to see if player has enough money to hire
		if (Session.playerShipVariables.ship.currency >= CostToHire) {
			//subtract the cost from the players currency
			Session.playerShipVariables.ship.currency -= (int)CostToHire;
			//change location of beacon
			Vector3 location = Vector3.zero;
			for (int x = 0; x < World.settlement_masterList_parent.transform.childCount; x++)
				if (World.settlement_masterList_parent.transform.GetChild(x).GetComponent<SettlementComponent>().thisSettlement.settlementID == City.settlementID)
					location = World.settlement_masterList_parent.transform.GetChild(x).position;
			Session.ActivateNavigatorBeacon(World.navigatorBeacon, location);
			Session.playerShipVariables.ship.currentNavigatorTarget = City.settlementID;
			Notifications.ShowANotificationMessage("You hired a navigator to " + City.name + " for " + CostToHire + " drachma.");
			//If not enough money, then let the player know
		}
		else {
			Notifications.ShowANotificationMessage("You can't afford to hire a navigator to " + City.name + ".");
		}
	}
}

public class TavernViewModel : Model
{
	Database Database => Globals.Database;

	public ICollectionModel<CityViewModel> Cities { get; private set; }

	public TavernViewModel(GameSession session, DialogScreen d) {
		Cities = ValueModel.Wrap(session.playerShipVariables.ship.playerJournal.knownSettlements)
			//.Where(id => id != Session.currentSettlement.settlementID)
			.Select(id => new TavernCityViewModel(session, Database.GetSettlementFromID(id), d) as CityViewModel);
	}
}

public class TavernView : ViewBehaviour<TavernViewModel>
{
	[SerializeField] CityListView CityList = null;

	public override void Bind(TavernViewModel model) {
		base.Bind(model);

		CityList.Bind(model.Cities);
	}
}
