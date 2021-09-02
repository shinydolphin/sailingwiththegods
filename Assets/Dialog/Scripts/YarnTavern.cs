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
	public Navigation nav;

	void Start() 
	{
		ds = GetComponent<DialogScreen>();
		ds.Runner.AddCommandHandler("displayknownsettlements", GenerateKnownSettlementUI);
	}

	#region Yarn Functions - Set Variables (Taverna)
	[YarnCommand("settaverninfo")]
	public void SetTavernInformation() {
		ds.YarnUI.onDialogueEnd.RemoveAllListeners();
		ds.YarnUI.onDialogueEnd.AddListener(ds.gui.CloseTavernDialog);
	}
	
	[YarnCommand("getcurrentsettlement")]
	public void GetCurrentSettlement() {
		ds.Storage.SetValue("$known_city", Globals.Game.Session.currentSettlement.name);
		ds.Storage.SetValue("$known_city_ID", Globals.Game.Session.currentSettlement.settlementID);
		ds.Storage.SetValue("$known_city_type", Globals.Game.Session.currentSettlement.typeOfSettlement);
	}

	//We need this so we can make sure not to let the player order a guide to the city they're currently at
	[YarnCommand("checkifcurrent")]
	public void CheckIfAskingAboutCurrentSettlement() {
		ds.Storage.SetValue("$asking_current", ds.Storage.GetValue("$known_city_ID").AsNumber == Globals.Game.Session.currentSettlement.settlementID);
	}

	[YarnCommand("getknownsettlementnumber")]
	public void GetNumberOfKnownSettlements() {
		ds.Storage.SetValue("$settlement_number", Globals.Game.Session.playerShipVariables.ship.playerJournal.knownSettlements.Count);
	}
	#endregion

	#region Yarn Functions - Random (Taverna)
	[YarnCommand("randomfooddialog")]
	public void GenerateRandomFoodDialog() {
		// Begin pulling random food item.
		List<FoodText> foodList = Globals.Database.foodDialogText;

		int i = Random.Range(1, foodList.Count);

		if (foodList[i].FoodCost == 0) {
			foodList[i].FoodCost = (int)ds.Storage.GetValue("$dracma_cost").AsNumber;
		}

		ds.Storage.SetValue("$food_dialog_item", foodList[i].Item);
		ds.Storage.SetValue("$food_dialog_quote", foodList[i].GetQuote);
		ds.Storage.SetValue("$drachma_cost", foodList[i].FoodCost);
	}

	[YarnCommand("randomwine")]
	public void GenerateRandomWineInfo() {
		// Begin pulling random wine items
		List<FoodText> wineList = Globals.Database.wineInfoText;

		int i = Random.Range(1, wineList.Count);

		if (wineList[i].FoodCost == 0) {
			wineList[i].FoodCost = (int)ds.Storage.GetValue("$dracma_cost").AsNumber;
		}

		ds.Storage.SetValue("$drachma_cost", wineList[i].FoodCost);
		ds.Storage.SetValue("$random_wine", wineList[i].Item);
		ds.Storage.SetValue("$wine_quote", wineList[i].GetQuote);
	}


	[YarnCommand("randomfood")]
	public void GenerateRandomFoodItem() 
	{
		// Begin pulling random food item.
		List<FoodText> foodList =  Globals.Database.foodItemText;

		int i = Random.Range(1, foodList.Count);

		if (foodList[i].FoodCost == 0) {
			foodList[i].FoodCost = (int)ds.Storage.GetValue("$generated_cost").AsNumber;
			//Debug.Log("Cost of this item: " + foodList[i].FoodCost + " while i is " + i + " Item should be " + foodList[i].Item);
		}

		ds.Storage.SetValue("$drachma_cost", foodList[i].FoodCost);
		ds.Storage.SetValue("$random_food", foodList[i].Item);
		ds.Storage.SetValue("$food_quote", foodList[i].GetQuote);
	}

	[YarnCommand("randomQA")]
	public void GenerateRandomQAText(string input) 
	{
		// Get the city we know of
		string e = ds.Storage.GetValue("$known_city").AsString;
		List<DialogText> matchingType = new List<DialogText>();

		// Obtain the known settlements we can talk about! (NOTE: will change to display known settlements and we'll search our info based on selection)
		Settlement[] settlementList = Globals.Database.settlement_masterList;
		Settlement targetSettlement = settlementList[0]; // Simple Assignment to ease compile errors.

		// Finding the currentSettlement
		foreach (Settlement a in settlementList) 
		{
			if (a.name == e)
				targetSettlement = a;
		}

		switch (input) 
		{
			case "network":
				e = Globals.Database.networkDialogText.Exists(x => x.CityType == e) ? e : "ALLOTHERS";
				matchingType = Globals.Database.networkDialogText.FindAll(x => x.CityType == e);
				break;
			case "pirate":
				e = Globals.Database.pirateDialogText.Exists(x => x.CityType == e) ? e : "ALLOTHERS";
				matchingType = Globals.Database.pirateDialogText.FindAll(x => x.CityType == e);
				break;
			case "myth":
				if (!e.Equals(ds.Storage.GetValue("$current_myth_city").AsString)) 
				{
					ds.Storage.SetValue("$current_myth_count", 0);
					ds.Storage.SetValue("$current_myth_city", e);
				}
				else
					ds.Storage.SetValue("$current_myth_count", ds.Storage.GetValue("$current_myth_count").AsNumber + 1);
				e = Globals.Database.mythDialogText.Exists(x => x.CityType == e) ? e : "ALLOTHERS";
				matchingType = Globals.Database.mythDialogText.FindAll(x => x.CityType == e);
				break;
			default:
				Debug.Log("Error, probaby because of a misspelling");
				break;
		}

		int i = Random.Range(0, matchingType.Count);

		ds.Storage.SetValue("$question", matchingType[i].TextQA[0]);
		ds.Storage.SetValue("$check_myth", matchingType.Count > ds.Storage.GetValue("$current_myth_count").AsNumber);

		if (e != "ALLOTHERS") 
		{
			if(e.Equals(ds.Storage.GetValue("$current_myth_city").AsString)) 
			{
				if(ds.Storage.GetValue("$check_myth").AsBool) 
				{
					// Clean this up for readability.
					ds.Storage.SetValue("$response", matchingType[(int)ds.Storage.GetValue("$current_myth_count").AsNumber].TextQA[1]);
					Globals.Game.Session.AddToCaptainsLog("Myth of " + e + ":\n" + ds.Storage.GetValue("$response").AsString);
				}
				else
					ds.Storage.SetValue("$response", "There is nothing more for me to say!");
			}
			else
				ds.Storage.SetValue("$response", matchingType[i].TextQA[1]);
		}
		else
			ds.Storage.SetValue("$response", targetSettlement.description);

		// For wanting to learn more. May consider changing conditional to check if input == myth instead
		if (input == "myth" && matchingType[i].TextQA.Length > 2)
			ds.Storage.SetValue("$response2", matchingType[i].TextQA[2]);

		//Special condition for home town?
	}
	#endregion

	#region Yarn Functions - Navigation
	[YarnCommand("randomguide")]
	public void GenerateGuideDialogue() 
	{
		List<DialogText> guideText = Globals.Database.guideDialogText;

		int i = Random.Range(1, guideText.Count);

		if(guideText[i].TextQA[0].Equals("")) 
		{
			guideText[i].TextQA = guideText[1].TextQA;
			
		}
		if (guideText[i].TextQA[1].Equals("")) {
			guideText[i].TextQA = guideText[1].TextQA;
		}

		ds.Storage.SetValue("$flavor_text1", guideText[i].CityType); // Wrongfully added in CityType.
		ds.Storage.SetValue("$flavor_text2", guideText[i].TextQA[0]);
		ds.Storage.SetValue("$flavor_text3", guideText[i].TextQA[1]);
	}

	[YarnCommand("hirenavigator")]
	public void SetSettlementWaypoint()
	{
		nav.SetDestination(ds.Storage.GetValue("$known_city").AsString, Globals.Game.Session.currentSettlement.availableCrew.RandomElement().ID);
	}
	#endregion

	public void GenerateKnownSettlementUI(string [] parameters, System.Action onComplete) 
	{
		ds.yarnOnComplete = onComplete;
		Globals.UI.Show<TavernView, TavernViewModel>(new TavernViewModel(ds));
	}
}
