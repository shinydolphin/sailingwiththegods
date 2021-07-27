using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Database
{
	// settlements
	public Region[] region_masterList { get; private set; }
	public Settlement[] settlement_masterList { get; private set; }

	// resources
	public List<MetaResource> masterResourceList { get; private set; } = new List<MetaResource>();

	//###################################
	//	Crew Member Variables
	//###################################
	public List<PirateType> masterPirateTypeList { get; private set; } = new List<PirateType>();
	public List<CrewMember> masterCrewList { get; private set; } = new List<CrewMember>();
	public List<DialogText> portDialogText { get; private set; } = new List<DialogText>();

	//Taverna
	public List<DialogText> networkDialogText { get; private set; } = new List<DialogText>();
	public List<DialogText> pirateDialogText { get; private set; } = new List<DialogText>();
	public List<DialogText> mythDialogText { get; private set; } = new List<DialogText>();
	public List<DialogText> guideDialogText { get; private set; } = new List<DialogText>();
	public List<DialogText> tradingDialogText { get; private set; } = new List<DialogText>(); // Perhaps
	public List<FoodText> foodItemText { get; private set; } = new List<FoodText>();
	public List<FoodText> wineInfoText { get; private set; } = new List<FoodText>();
	public List<FoodText> foodDialogText { get; private set; } = new List<FoodText>();

	public List<string> tavernaGameInsults;
	public List<string> tavernaGameBragging;

	//STORMS
	public List<Ritual> stormRituals = new List<Ritual>();
	public List<string> stormTitles;
	public List<string> stormSubtitles;
	public List<string> stormStartText;
	public List<string> stormSeerText;
	public List<string> stormNoSeerText;
	public List<string> stormRitualResultsText;
	public List<string> stormSuccessText;
	public List<string> stormFailureText;

	//PIRATES
	public List<string> pirateTitles;
	public List<string> pirateSubtitles;
	public List<string> pirateStartText;
	public List<string> pirateTypeIntroText;
	public List<string> pirateNegotiateText;
	public List<string> pirateRunSuccessText;
	public List<string> pirateRunFailText;
	public List<string> pirateSuccessText;
	public List<string> pirateFailureText;

	public CaptainsLogEntry[] captainsLogEntries { get; private set; }


	public void Init() {

		//Load all txt database files
		masterPirateTypeList = CSVLoader.LoadMasterPirateTypes();
		masterCrewList = CSVLoader.LoadMasterCrewRoster(masterPirateTypeList);
		captainsLogEntries = CSVLoader.LoadCaptainsLogEntries();
		masterResourceList = CSVLoader.LoadResourceList();
		stormRituals = CSVLoader.LoadRituals();
		CSVLoader.LoadStormText(out stormTitles, out stormSubtitles, out stormStartText, out stormSeerText, out stormNoSeerText,
			out stormRitualResultsText, out stormSuccessText, out stormFailureText);
		CSVLoader.LoadPirateText(out pirateTitles, out pirateSubtitles, out pirateStartText, out pirateTypeIntroText, out pirateNegotiateText,
			out pirateRunSuccessText, out pirateRunFailText, out pirateSuccessText, out pirateFailureText);
		portDialogText = CSVLoader.LoadPortDialog();

		CSVLoader.LoadTavernaGameBarks(out tavernaGameInsults, out tavernaGameBragging);

		// Mylo's Addition
		networkDialogText = CSVLoader.LoadNetworkDialog();
		pirateDialogText = CSVLoader.LoadPirateDialog();
		mythDialogText = CSVLoader.LoadMythDialog();
		guideDialogText = CSVLoader.LoadHireGuideDialog();
		// trading goods here
		foodItemText = CSVLoader.LoadFoodItemsList();
		foodDialogText = CSVLoader.LoadFoodDialogList();
		wineInfoText = CSVLoader.LoadWineInfoList();


		// end Mylo's Addition

		region_masterList = CSVLoader.LoadRegionList();
		settlement_masterList = CSVLoader.LoadSettlementList();     // depends on resource list, region list, and crew list
	}

	public string GetJobClassEquivalency(CrewType jobCode) {
		//This function simply returns a predefined string based on the number code of a crewmembers job
		//	--based on their constant values held in the global variables 
		string title = "";
		switch (jobCode) {
			case CrewType.Sailor: title = "Sailor"; break;
			case CrewType.Warrior: title = "Warrior"; break;
			case CrewType.Slave: title = "Slave"; break;
			case CrewType.Passenger: title = "Passenger"; break;
			case CrewType.Navigator: title = "Navigator"; break;
			case CrewType.Assistant: title = "Assistant"; break;
			case CrewType.Guide: title = "Guide"; break;
			case CrewType.Lawyer: title = "Lawyer"; break;
			case CrewType.Royalty: title = "Royalty"; break;
			case CrewType.Seer: title = "Seer"; break;
		}
		return title;
	}



	public string GetCloutTitleEquivalency(int clout) {
		//This function simply returns a predefined string based on the number value of the clout provided
		string title = "";
		if (clout > 1 && clout <= 499) title = "Goatherd";
		else if (clout > 500 && clout <= 999) title = "Farmer";
		else if (clout > 1000 && clout <= 1499) title = "Merchant";
		else if (clout > 1500 && clout <= 1999) title = "Mercenary";
		else if (clout > 2000 && clout <= 2499) title = "Knight";
		else if (clout > 2500 && clout <= 2999) title = "War Chief";
		else if (clout > 3000 && clout <= 3499) title = "Boule Leader";
		else if (clout > 3500 && clout <= 3999) title = "Ambassador";
		else if (clout > 4000 && clout <= 4499) title = "Prince";
		else if (clout > 4500 && clout <= 4999) title = "King";
		else if (clout >= 5000) title = "The God";
		else if (clout == 0) title = "Dead";
		else title = "ERROR: clout is not between 0 and 100";
		return title;
	}

	public Region GetRegionByName(string name) => region_masterList.FirstOrDefault(r => r.Name == name);
	public Settlement GetSettlementByName(string name) => settlement_masterList.FirstOrDefault(s => s.name == name);

	public Settlement GetSettlementFromID(int ID) {
		//Debug.Log (settlement_masterList.Length);
		foreach (Settlement city in settlement_masterList) {
			//Debug.Log ("DEBUG: city: " + city.name);
			if (city.settlementID == ID) {
				return city;
			}
		}
		//if no matches(this shouldn't be possible--return a fake settlement rather than a null
		//	--this is more sophisticated than a null--it won't crash but the error is obvious.
		Debug.Log("ERROR: DIDNT FIND ID MATCH IN GetSettlementFromID Function: Looking for settlement ID:  " + ID);
		return new Settlement(-1, "ERROR", -1);
	}

	public CrewMember GetCrewMemberFromID(int ID) {
		foreach (CrewMember crewman in masterCrewList) {
			if (crewman.ID == ID)
				return crewman;
		}
		return null;
	}
}
