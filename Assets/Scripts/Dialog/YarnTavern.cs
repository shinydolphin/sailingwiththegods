// Mylo Gonzalez

using Nav;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Yarn.Unity;

public class YarnTavern : MonoBehaviour
{
	private DialogScreen ds;
	private Navigation _Nav;
	private List<Resource> abundantGoods;
	private List<Resource> scarceGoods;

	void Start() 
	{
		ds = GetComponent<DialogScreen>();
		ds.Runner.AddCommandHandler("displayknownsettlements", GenerateKnownSettlementUI);
		_Nav = GameObject.Find("Nav").GetComponent<Navigation>();
	}

	#region Yarn Functions - Set Variables (Taverna)
	[YarnCommand("settaverninfo")]
	public void SetTavernInformation() {
		ds.YarnUI.onDialogueEnd.RemoveAllListeners();
		ds.YarnUI.onDialogueEnd.AddListener(ds.gui.CloseTavernDialog);
	}
	
	[YarnCommand("getcurrentsettlement")]
	public void GetCurrentSettlement() {
		ds.Storage.SetValue("$known_city", Globals.GameVars.currentSettlement.name);
		ds.Storage.SetValue("$known_city_ID", Globals.GameVars.currentSettlement.settlementID);
		ds.Storage.SetValue("$known_city_type", Globals.GameVars.currentSettlement.typeOfSettlement);
	}

	//We need this so we can make sure not to let the player order a guide to the city they're currently at
	[YarnCommand("checkifcurrent")]
	public void CheckIfAskingAboutCurrentSettlement() {
		ds.Storage.SetValue("$asking_current", ds.Storage.GetValue("$known_city_ID").AsNumber == Globals.GameVars.currentSettlement.settlementID);
	}

	[YarnCommand("getknownsettlementnumber")]
	public void GetNumberOfKnownSettlements() {
		ds.Storage.SetValue("$settlement_number", Globals.GameVars.playerShipVariables.ship.playerJournal.knownSettlements.Count);
	}
	#endregion

	#region Yarn Functions - Random (Taverna)
	[YarnCommand("randomfooddialog")]
	public void GenerateRandomFoodDialog() {
		// Begin pulling random food item.
		List<string> foodList = Globals.GameVars.foodDialogText;

		int i = Random.Range(1, foodList.Count);

		//ds.Storage.SetValue("$food_dialog_item", foodList[i].Item);
		ds.Storage.SetValue("$food_dialog_quote", foodList[i]);
	}

	[YarnCommand("randomwine")]
	public void GenerateRandomWineInfo() {
		// Begin pulling random wine items
		List<FoodText> wineList = Globals.GameVars.wineInfoText;

		int i = Random.Range(1, wineList.Count);
		
		ds.Storage.SetValue("$random_wine", wineList[i].Item);
		ds.Storage.SetValue("$wine_quote", wineList[i].Quote);
	}


	[YarnCommand("randomfood")]
	public void GenerateRandomFoodItem() 
	{
		// Begin pulling random food item.
		List<FoodText> foodList =  Globals.GameVars.foodItemText;

		int i = Random.Range(1, foodList.Count);
		
		ds.Storage.SetValue("$random_food", foodList[i].Item);
		ds.Storage.SetValue("$food_quote", foodList[i].Quote);
	}

	[YarnCommand("setcitygoods")]
	public void SetCity() {
		abundantGoods = new List<Resource>(CityDetailsViewModel.AbundantResource(CityFromName(ds.Storage.GetValue("$known_city").AsString)));
		scarceGoods = new List<Resource>(CityDetailsViewModel.ScarceResource(CityFromName(ds.Storage.GetValue("$known_city").AsString)));
	}

	[YarnCommand("gettradegoods")]
	public void GetTradeGoods() 
	{
		ds.Storage.SetValue("$trade_goods_finished", false);

		string cityName = ds.Storage.GetValue("$known_city").AsString;
		bool haveLots = Random.Range(1, 3) % 2 == 0;
		Debug.Log($"HaveLots {haveLots}");
		Settlement city = CityFromName(cityName);

		ds.Storage.SetValue("trade_resource", "null");

		if ((haveLots && abundantGoods.Count > 0) || (!haveLots && scarceGoods.Count == 0)) {
			Resource r = abundantGoods.RandomElement();
			abundantGoods.Remove(r);
			Debug.Log($"Getting an abundant good: HaveLots {haveLots} and {abundantGoods.Count} left (and {scarceGoods.Count} scarce left)");
			ds.Storage.SetValue("$trade_resource", r.name);
			ds.Storage.SetValue("$have_lots", true);
		}
		else if ((!haveLots && scarceGoods.Count > 0) || (haveLots && abundantGoods.Count == 0) ) {
			Resource r = scarceGoods.RandomElement();
			scarceGoods.Remove(r);
			Debug.Log($"Getting a scarce good: HaveLots {haveLots} and {scarceGoods.Count} left (and {abundantGoods.Count} abuntant left)");
			ds.Storage.SetValue("$trade_resource", r.name);
			ds.Storage.SetValue("$have_lots", false);
		}

		if (abundantGoods.Count == 0 && scarceGoods.Count == 0) {
			Debug.Log("Out of both types of goods");
			ds.Storage.SetValue("$trade_goods_finished", true);
		}
	}

	[YarnCommand("randomQA")]
	public void GenerateRandomQAText(string input) 
	{
		// Get the city we know of
		string cityName = ds.Storage.GetValue("$known_city").AsString;
		List<DialogPair> matchingType = new List<DialogPair>();

		Settlement targetSettlement = CityFromName(cityName);

		switch (input) 
		{
			case "network":
				cityName = Globals.GameVars.networkDialogText.Exists(x => x.CityName == cityName) ? cityName : "ALLOTHERS";
				matchingType = Globals.GameVars.networkDialogText.FindAll(x => x.CityName == cityName);
				break;
			case "pirate":
				cityName = Globals.GameVars.pirateDialogText.Exists(x => x.CityName == cityName) ? cityName : "ALLOTHERS";
				matchingType = Globals.GameVars.pirateDialogText.FindAll(x => x.CityName == cityName);
				break;
			case "myth":
				if (!cityName.Equals(ds.Storage.GetValue("$current_myth_city").AsString)) 
				{
					ds.Storage.SetValue("$current_myth_count", 0);
					ds.Storage.SetValue("$current_myth_city", cityName);
				}
				else
					ds.Storage.SetValue("$current_myth_count", ds.Storage.GetValue("$current_myth_count").AsNumber + 1);
				cityName = Globals.GameVars.mythDialogText.Exists(x => x.CityName == cityName) ? cityName : "ALLOTHERS";
				matchingType = Globals.GameVars.mythDialogText.FindAll(x => x.CityName == cityName);
				break;
			default:
				Debug.Log("Error, probaby because of a misspelling");
				break;
		}

		int i = Random.Range(0, matchingType.Count);

		ds.Storage.SetValue("$question", matchingType[i].Question);
		ds.Storage.SetValue("$check_myth", matchingType.Count > ds.Storage.GetValue("$current_myth_count").AsNumber);

		if (cityName != "ALLOTHERS") 
		{
			if(cityName.Equals(ds.Storage.GetValue("$current_myth_city").AsString)) 
			{
				if(ds.Storage.GetValue("$check_myth").AsBool) 
				{
					// Clean this up for readability.
					ds.Storage.SetValue("$response", matchingType[(int)ds.Storage.GetValue("$current_myth_count").AsNumber].Answer);
					Globals.GameVars.AddToCaptainsLog("Myth of " + cityName + ":\n" + ds.Storage.GetValue("$response").AsString);
				}
				else
					ds.Storage.SetValue("$response", "There is nothing more for me to say!");
			}
			else
				ds.Storage.SetValue("$response", matchingType[i].Answer);
		}
		else
			ds.Storage.SetValue("$response", targetSettlement.description);

		// For wanting to learn more. May consider changing conditional to check if input == myth instead
		//if (input == "myth" && matchingType[i].TextQA.Length > 2)
		//	ds.Storage.SetValue("$response2", matchingType[i].TextQA[2]);

		//Special condition for home town?
	}
	#endregion

	#region Yarn Functions - Navigation
	[YarnCommand("randomguide")]
	public void GenerateGuideDialogue() 
	{
		List<string> guideText = Globals.GameVars.guideDialogText;

		int i = Random.Range(1, guideText.Count);

		ds.Storage.SetValue("$flavor_text1", guideText[i]); // Wrongfully added in CityType.
	}

	[YarnCommand("hirenavigator")]
	public void SetSettlementWaypoint()
	{
		_Nav.SetDestination(ds.Storage.GetValue("$known_city").AsString,Globals.GameVars.AllNonCrew.RandomElement().ID);		
	}
	#endregion

	public void GenerateKnownSettlementUI(string [] parameters, System.Action onComplete) 
	{
		ds.yarnOnComplete = onComplete;
		Globals.UI.Show<TavernView, TavernViewModel>(new TavernViewModel(ds));
	}

	private Settlement CityFromName(string name) 
	{
		// Obtain the known settlements we can talk about! (NOTE: will change to display known settlements and we'll search our info based on selection)
		Settlement[] settlementList = Globals.GameVars.settlement_masterList;
		Settlement targetSettlement = settlementList[0]; // Simple Assignment to ease compile errors.

		// Finding the currentSettlement
		foreach (Settlement a in settlementList) {
			if (a.name == name)
				targetSettlement = a;
		}
		return targetSettlement;
	}
}
