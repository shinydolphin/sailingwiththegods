using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DashboardViewModel : Model
{
	GameVars GameVars => Globals.GameVars;
	GameSession Session => Globals.Session;

	public string CaptainsLog => Session.CaptainsLog;
	public readonly CargoInventoryViewModel WaterInventory;
	public readonly CargoInventoryViewModel FoodInventory;
	public readonly ICollectionModel<CargoInventoryViewModel> CargoList;
	public readonly ICollectionModel<CrewManagementMemberViewModel> CrewList;

	public BoundModel<float> Clout;
	public CrewMember Jason => Session.Crew.Jason;

	public BoundModel<bool> SailsAreUnfurled { get; private set; }

	public BoundModel<string> Objective { get; private set; }

	public DashboardViewModel() {

		Clout = new BoundModel<float>(Session.playerShipVariables.ship, nameof(Session.playerShipVariables.ship.playerClout));

		var water = Session.playerShipVariables.ship.cargo.FirstOrDefault(r => r.name == Resource.Water);
		WaterInventory = new CargoInventoryViewModel(water);

		var food = Session.playerShipVariables.ship.cargo.FirstOrDefault(r => r.name == Resource.Provisions);
		FoodInventory = new CargoInventoryViewModel(food);

		CargoList = ValueModel.Wrap(new ObservableCollection<CargoInventoryViewModel>(Session.playerShipVariables.ship.cargo.Select(c => new CargoInventoryViewModel(c))));

		CrewList = ValueModel.Wrap(Session.playerShipVariables.ship.crewRoster)
			.Select(c => new CrewManagementMemberViewModel(c, OnCrewClicked, OnCrewCityClicked));

		SailsAreUnfurled = new BoundModel<bool>(Session.playerShipVariables.ship, nameof(Session.playerShipVariables.ship.sailsAreUnfurled));

		Objective = new BoundModel<string>(Session.playerShipVariables.ship, nameof(Session.playerShipVariables.ship.objective));
	}

	public void OnCrewCityClicked(CityViewModel city) {
		Debug.Log("City clicked: " + city.PortName);

		if(Globals.UI.IsShown<CityView>()) {
			Globals.UI.Hide<CityView>();
		}

		var beacon = GameVars.crewBeacon;
		if (city.City != beacon.Target) {
			beacon.Target = city.City;
			Session.ActivateNavigatorBeacon(GameVars.crewBeacon, city.City.theGameObject.transform.position);
			Session.RotateCameraTowards(city.City.theGameObject.transform.position);
			Globals.UI.Show<CityView, CityViewModel>(new CityDetailsViewModel(city.City, null));
		}
		else {
			beacon.IsBeaconActive = false;
		}
	}

	public void OnCrewClicked(CrewManagementMemberViewModel crew) {

		if (Globals.UI.IsShown<CityView>()) {
			Globals.UI.Hide<CityView>();
		}

		Globals.UI.Show<CrewDetailsScreen, CrewManagementMemberViewModel>(crew);
	}

	public void GUI_furlOrUnfurlSails() {
		if (Session.playerShipVariables.ship.sailsAreUnfurled) {
			Session.playerShipVariables.ship.sailsAreUnfurled = false;
			foreach (GameObject sail in GameVars.sails)
				sail.SetActive(false);
		}
		else {
			Session.playerShipVariables.ship.sailsAreUnfurled = true;
			foreach (GameObject sail in GameVars.sails)
				sail.SetActive(true);

		}
	}

	public void GUI_dropAnchor() {
		//If the controls are locked--we are traveling so force it to stop
		if (Session.controlsLocked && !Session.showSettlementGUI)
			Session.playerShipVariables.rayCheck_stopShip = true;
	}
}
