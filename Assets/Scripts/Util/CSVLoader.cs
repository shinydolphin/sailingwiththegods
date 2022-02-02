using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public static class CSVLoader
{
	static Resource[] ParseSettlementCargo(string[] records, string[] headers, int startColumnIdx, int population) {
		var results = new List<Resource>();
		for(var i = startColumnIdx; i < startColumnIdx + Resource.All.Length; i++) { 
			var probabilityOfAvailability = float.Parse(records[i]);

			//TODO The probability values are 1-100 and population affects the amount
			//  Population/2 x (probabilityOfResource/100)
			float amount = (population / 2) * (probabilityOfAvailability / 1.5f);

			// settlement csv may get names wrong, treat order as the important thing instead (TODO: which is obviously a mistake because it's easier to get names right than order probably)
			results.Add(new Resource(Resource.All[i - startColumnIdx], amount));
		}
		return results.ToArray();
	}

	public static Settlement[] LoadSettlementList() {
		char[] lineDelimiter = new char[] { '@' };
		char[] recordDelimiter = new char[] { '_' };

		string filename = "settlement_list_newgame";
		string[] fileByLine = TryLoadListFromGameFolder(filename);

		var settlement_masterList = new Dictionary<int, Settlement>();
		var headers = fileByLine[0].Split(lineDelimiter, StringSplitOptions.None);
		for (int row = 1; row < fileByLine.Length; row++) {
			string[] records = fileByLine[row].Split(lineDelimiter, StringSplitOptions.None);

			try {

				// parse indices before cargo list
				int id = int.Parse(records[0]);

				var settlement = new Settlement(
					settlementID: id,
					name: records[1],
					location_longXlatY: ParseEastingNorthing(records[2], records[3]),
					elevation: float.Parse(records[4]),
					population: int.Parse(records[5])
				);

				settlement.networks = ParseIntList(records[7]);
				settlement.tax_neutral = float.Parse(records[9]);
				settlement.tax_network = float.Parse(records[10]);

				const int cargoStartIndex = 11;
				settlement.cargo = ParseSettlementCargo(records, headers, cargoStartIndex, settlement.population);

				// parse indices after cargo list (using an offset from the end of it)
				int afterCargo = cargoStartIndex + Resource.All.Length;
				settlement.typeOfSettlement = int.Parse(records[afterCargo]);
				settlement.prefabName = records[afterCargo + 1];
				settlement.description = records[afterCargo + 2];
				settlement.godTax = int.Parse(records[afterCargo + 3]) == 1;
				settlement.godTaxAmount = int.Parse(records[afterCargo + 4]);
				settlement.transitTax = int.Parse(records[afterCargo + 5]) == 1;
				settlement.transitTaxPercent = float.Parse(records[afterCargo + 6]);
				settlement.foreignerFee = int.Parse(records[afterCargo + 7]) == 1;
				settlement.foreignerFeePercent = float.Parse(records[afterCargo + 8]);
				settlement.ellimenionPercent = float.Parse(records[afterCargo + 9]);
				settlement.coinText = records[afterCargo + 10];
				settlement.Region = Globals.Database.GetRegionByName(records[afterCargo + 11]);

				settlement_masterList.Add(id, settlement);

			}
			catch(Exception e) {
				Debug.LogError($"CSV Parse error on line {row}:");
				Debug.LogException(e);
			}
		}

		LoadAdjustedSettlementLocations(settlement_masterList);

		return settlement_masterList.Values.ToArray();

	}

	public static Region[] LoadRegionList() {
		char[] lineDelimiter = new char[] { '@' };
		char[] recordDelimiter = new char[] { '_' };

		string filename = "regions_list";
		string[] fileByLine = TryLoadListFromGameFolder(filename);

		var result = new List<Region>();
		for (int row = 1; row < fileByLine.Length; row++) {
			string[] records = fileByLine[row].Split(lineDelimiter, StringSplitOptions.None);

			// parse indices before cargo list
			var region = new Region {
				Name = records[0],
				Description = records[1]
			};
			result.Add(region);
		}

		return result.ToArray();
	}

	public static WindRose[,] LoadWindRoses(int width, int height) {
		char[] lineDelimiter = new char[] { ',' };

		string filename = "windroses_january";

		string[] fileByLine = TryLoadListFromGameFolder(filename);

		var windrose_January = new WindRose[width, height];

		//For each line of the wind rose file (the row)
		for (int row = 0; row < fileByLine.Length; row++) {
			//Debug.Log("-->" + fileByLine[lineCount]);
			string[] records = fileByLine[row].Split(lineDelimiter, StringSplitOptions.None);
			//Now loop through each column of the line and assign it to a windrose within January
			for (int col = 0; col < records.Length / 2; col++) {
				float direction = float.Parse(records[col * 2]);//there are double the amount of columns in the file--these formulas account for that
				float speed = float.Parse(records[(col * 2) + 1]);
				windrose_January[col, row] = new WindRose(direction, speed);
				//Debug.Log (col + " " + row + "   :   " + windrose_January[col,row].direction + " -> " + windrose_January[col,row].speed);
			}
		}

		return windrose_January;
	}

	public static CurrentRose[,] LoadWaterZonesFromFile(int width, int height) {
		char[] lineDelimiter = new char[] { ',' };

		string filename = "waterzones_january";

		string[] fileByLine = TryLoadListFromGameFolder(filename);

		var currentRose_January = new CurrentRose[width, height];

		//For each line of the wind rose file (the row)
		for (int row = 0; row < fileByLine.Length; row++) {
			//Debug.Log("-->" + fileByLine[lineCount]);
			string[] records = fileByLine[row].Split(lineDelimiter, StringSplitOptions.None);
			//Now loop through each column of the line and assign it to a windrose within January
			for (int col = 0; col < records.Length / 2; col++) {
				float direction = float.Parse(records[col * 2]);//there are double the amount of columns in the file--these formulas account for that
				float speed = float.Parse(records[(col * 2) + 1]);
				currentRose_January[col, row] = new CurrentRose(direction, speed);
				//Debug.Log (col + " " + row + "   :   " + currentRose_January[col,row].direction + " -> " + currentRose_January[col,row].speed);
			}
		}

		return currentRose_January;
	}
	public static CaptainsLogEntry[] LoadCaptainsLogEntries() {
		
		char[] lineDelimiter = new char[] { '@' };
		string filename = "captains_log_database";

		string[] fileByLine = TryLoadListFromGameFolder(filename);

		var captainsLogEntries = new CaptainsLogEntry[fileByLine.Length];
		//For each line of the wind rose file (the row)
		for (int row = 0; row < fileByLine.Length; row++) {
			//Debug.Log (captainsLogEntries.Length + "  :  " + row);
			string[] records = fileByLine[row].Split(lineDelimiter, StringSplitOptions.None);
			captainsLogEntries[row] = new CaptainsLogEntry(int.Parse(records[0]), records[1]);

		}

		//Debugging
		//for (int i = 0; i < captainsLogEntries.Length; i++)
		//Debug.Log (captainsLogEntries[i].settlementID + "  :  " + captainsLogEntries[i].logEntry);

		return captainsLogEntries;
	}

	static List<int> ParseIntList(string cellData) {
		return cellData.Split('_')
			.Select(id => int.Parse(id))
			.ToList();
	}

	static List<float> ParseFloatList(string cellData) {
		return cellData.Split('_')
			.Select(id => float.Parse(id))
			.ToList();
	}

	static Vector2 ToVector2(this List<float> list) {
		return new Vector2(list[0], list[1]);
	}

	static Vector2 ParseEastingNorthing(string easting, string northing) {
		return new [] {float.Parse(easting), float.Parse(northing)}.ToList().ToVector2().Reverse();
	}

	// csv uses is (lat/easting, long/northing) but the game uses (long/northing, lat/easting) 
	static Vector2 ParseEastingNorthing(string cellData) {
		return ParseFloatList(cellData).ToVector2().Reverse();
	}
	
	static QuestSegment.Trigger ParseTrigger(string triggerTypeCell, string triggerDataCell) {
		var triggerType = (QuestSegment.TriggerType)Enum.Parse(typeof(QuestSegment.TriggerType), triggerTypeCell);
		switch(triggerType) {
			case QuestSegment.TriggerType.City:
				return new QuestSegment.CityTrigger(int.Parse(triggerDataCell));
			case QuestSegment.TriggerType.Coord:
				return new QuestSegment.CoordTrigger(ParseEastingNorthing(triggerDataCell));			
			case QuestSegment.TriggerType.UpgradeShip:
				return new QuestSegment.UpgradeShipTrigger();
			case QuestSegment.TriggerType.None:
				return new QuestSegment.NoneTrigger();
			default:
				return null;
		}
	}

	static QuestSegment.ArrivalEvent ParseArrivalEvent(string eventTypeCell, string eventDataCell) {
		var triggerType = (QuestSegment.ArrivalEventType)Enum.Parse(typeof(QuestSegment.ArrivalEventType), eventTypeCell);
		switch (triggerType) {
			case QuestSegment.ArrivalEventType.Message:
				return new QuestSegment.MessageArrivalEvent(eventDataCell);
			case QuestSegment.ArrivalEventType.Quiz:
				return new QuestSegment.QuizArrivalEvent(eventDataCell);
			case QuestSegment.ArrivalEventType.None:
				return new QuestSegment.NoneArrivalEvent();
			default:
				return null;
		}
	}

	//This loads the main quest line from a CSV file in the resources
	public static MainQuestLine LoadMainQuestLine() {
		MainQuestLine mainQuest = new MainQuestLine();
		char[] lineDelimiter = new char[] { '@' };
		string filename = "main_questline_database";

		string[] fileByLine = TryLoadListFromGameFolder(filename);

		//start at index 1 to skip the record headers
		//For each line of the main quest file (the row)
		for (int row = 1; row < fileByLine.Length; row++) {
			string[] records = fileByLine[row].Split(lineDelimiter, StringSplitOptions.None);

			//now let's see if we're on the last segment of the questline
			bool isEnd = false;
			if (row == fileByLine.Length - 1)
				isEnd = true;

			//now add the segment to the main questline
			mainQuest.questSegments.Add(new QuestSegment(
				segmentID: int.Parse(records[0]), 
				trigger: ParseTrigger(records[1], records[2]), 
				skippable: bool.Parse(records[3]),
				objective: records[4],
				descriptionOfQuest: records[5], 
				arrivalEvent: ParseArrivalEvent(records[6], records[7]),
				crewmembersToAdd: ParseIntList(records[9]), 
				crewmembersToRemove: ParseIntList(records[10]), 
				isFinalSegment: isEnd, 
				mentionedPlaces: ParseIntList(records[8]),
				image: Resources.Load<Sprite>(records[11]),
				caption: records[12]
			));
		}

		return mainQuest;
	}

	public static List<PirateType> LoadMasterPirateTypes() {
		char[] lineDelimiter = new char[] { '@' };
		string filename = "pirate_types";
		string[] fileByLine = TryLoadListFromGameFolder(filename);

		var masterPirateTypeList = new List<PirateType>();

		//start at index 1 to skip the record headers
		//For each line of the main quest file (the row)
		for (int row = 1; row < fileByLine.Length; row++) {
			string[] records = fileByLine[row].Split(lineDelimiter, StringSplitOptions.None);

			masterPirateTypeList.Add(new PirateType {
				ID = row,
				name = records[1],
				difficulty = int.Parse(records[2])
			});
		}

		return masterPirateTypeList;
	}

	public static List<CrewMember> LoadMasterCrewRoster(List<PirateType> pirateTypes) {
		char[] lineDelimiter = new char[] { '@' };
		string filename = "crewmembers_database";
		string[] fileByLine = TryLoadListFromGameFolder(filename);

		var masterCrewList = new List<CrewMember>();

		//start at index 1 to skip the record headers
		//For each line of the main quest file (the row)
		for (int row = 1; row < fileByLine.Length; row++) {
			string[] records = fileByLine[row].Split(lineDelimiter, StringSplitOptions.None);

			bool isKillable = false;
			bool isPartOfMainQuest = false;
			if (int.Parse(records[6]) == 1)
				isKillable = true;
			if (int.Parse(records[7]) == 1)
				isPartOfMainQuest = true;

			bool isPirate = records[8] == "1";              // TODO: change to TRUE/FALSE in spreadsheet so bool.Parse will work

			//Let's add a crewmember to the master roster
			// TODO: Change CrewType+PirateType in CSV to a string so it's more readable and use Enum.Parse
			masterCrewList.Add(new CrewMember(
				ID: int.Parse(records[0]), 
				name: records[1], 
				originCity: int.Parse(records[2]), 
				clout: int.Parse(records[3]), 
				typeOfCrew: (CrewType)int.Parse(records[4]), 
				backgroundInfo: records[5], 
				isKillable: isKillable, 
				isPartOfMainQuest: isPartOfMainQuest, 
				isPirate: isPirate,
				pirateType: isPirate ? pirateTypes[int.Parse(records[9]) - 1] : null
			));
		}

		return masterCrewList;

	}

	static void LoadAdjustedSettlementLocations(Dictionary<int, Settlement> settlements) {
		
		char[] lineDelimiter = new char[] { ',' };
		int currentID = 0;
		string filename = "settlement_unity_position_offsets";

		string[] fileByLine = TryLoadListFromGameFolder(filename);

		for (int row = 0; row < fileByLine.Length; row++) {
			string[] records = fileByLine[row].Split(lineDelimiter, StringSplitOptions.None);
			currentID = int.Parse(records[0]);

			if (settlements.ContainsKey(currentID)) {
				var thisSettlement = settlements[currentID];
				thisSettlement.adjustedGamePosition = new Vector3(float.Parse(records[1]), float.Parse(records[2]), float.Parse(records[3]));
				thisSettlement.eulerY = float.Parse(records[4]);
			}
		}

	}

	public static List<MetaResource> LoadResourceList() {
		char[] lineDelimiter = new char[] { '@' };
		string filename = "resource_list";

		string[] fileByLine = TryLoadListFromGameFolder(filename);

		var masterResourceList = new List<MetaResource>();

		//start at index 1 to skip the record headers we have to then subtract 
		for (int lineCount = 1; lineCount < fileByLine.Length; lineCount++) {
			string[] records = fileByLine[lineCount].Split(lineDelimiter, StringSplitOptions.None);
			masterResourceList.Add(new MetaResource(records[1], int.Parse(records[0]), records[3], records[2], int.Parse(records[4])));
		}

		return masterResourceList;
	}


	// Kylie's Stuff!
	public static List<Ritual> LoadRituals() 
	{
		List<Ritual> rituals = new List<Ritual>();

		char[] lineDelimiter = new char[] { '@' };
		char[] resourcesDelimiter = new char[] { ';' };
		string filename = "ritual_types";

		string[] fileByLine = TryLoadListFromGameFolder(filename);

		//Ignore top header row
		for (int lineCount = 1; lineCount < fileByLine.Length; lineCount++) 
		{
			//0: has seer or not (int to be cast into a bool)
			//1: flavor text (string)
			//2: success chance (float)
			//3: clout gain (int)
			//4: lost resource ID (blank for none, int otherwise, separated by ; if more than one)
			//5: lost resource quantity (0 for none, int otherwise, separated by ; if more than one)

			string[] ritualInfo = fileByLine[lineCount].Split(lineDelimiter, StringSplitOptions.None);
			bool hasSeer = int.Parse(ritualInfo[0]) != 0;
			float successChance = float.Parse(ritualInfo[2]);
			int cloutGain = int.Parse(ritualInfo[3]);
			int cloutLoss = 10;
			int[] resourceID;
			if (ritualInfo[4] == "") {
				resourceID = new int[0];
			}
			else {
				string[] resources = ritualInfo[4].Split(resourcesDelimiter, StringSplitOptions.None);
				resourceID = new int[resources.Length];
				for (int i = 0; i < resources.Length; i++) {
					resourceID[i] = int.Parse(resources[i]);
				}
			}

			int[] resourceAmounts = new int[resourceID.Length];
			if (resourceAmounts.Length > 0) {
				string[] amts = ritualInfo[5].Split(resourcesDelimiter, StringSplitOptions.None);
				if (resourceAmounts.Length != amts.Length) {
					Debug.Log("Wrong quantities for resources");
				}
				for (int i = 0; i < resourceAmounts.Length; i++) {
					resourceAmounts[i] = int.Parse(amts[i]);
				}
			}

			Ritual r = new Ritual(hasSeer, ritualInfo[1], successChance, cloutGain, cloutLoss, resourceID, resourceAmounts);

			rituals.Add(r);
		}

		return rituals;
	}

	public static void LoadStormText(out List<string> titles, out List<string> subtitles, out List<string> startText, out List<string> ritualTextSeer, 
		out List<string> ritualTextNoSeer, out List<string> resultsText, out List<string> successText, out List<string> failText) 
	{
		List<List<string>> textList = new List<List<string>> {
			(titles = new List<string>()),
			(subtitles = new List<string>()),
			(resultsText = new List<string>()),
			(startText = new List<string>()),
			(ritualTextSeer = new List<string>()),
			(ritualTextNoSeer = new List<string>()),
			(successText = new List<string>()),
			(failText = new List<string>())
		};

		char[] lineDelimiter = new char[] { '@' };
		char newline = '%';
		string filename = "storm_flavor";

		string[] fileByLine = TryLoadListFromGameFolder(filename);

		if (textList.Count != fileByLine.Length) 
		{
			Debug.Log($"ERROR: wrong number of lines in the Storm Flavor file!\nShould have {textList.Count} but actually has {fileByLine.Length}");
		}

		for (int i = 0; i < fileByLine.Length; i++) 
		{
			string[] texts = fileByLine[i].Split(lineDelimiter);
			for (int j = 0; j < texts.Length; j++) 
			{
				if (texts[j] != "") 
				{
					string addText = StripAndAddNewlines(texts[j], newline);
					textList[i].Add(addText);
				}
			}
		}
	}

	public static void LoadPirateText(out List<string> titles, out List<string> subtitles, out List<string> startText, out List<string> pirateIntros, out List<string> negotiateText,
		out List<string> runSuccessText, out List<string> runFailureText, out List<string> successText, out List<string> failText) 
	{
		List<List<string>> textList = new List<List<string>> {
			(titles = new List<string>()),
			(subtitles = new List<string>()),
			(pirateIntros = new List<string>()),
			(startText = new List<string>()),
			(negotiateText = new List<string>()),
			(runSuccessText = new List<string>()),
			(runFailureText = new List<string>()),
			(successText = new List<string>()),
			(failText = new List<string>())
		};

		char[] lineDelimiter = new char[] { '@' };
		char newline = '%';
		string filename = "pirate_flavor";

		string[] fileByLine = TryLoadListFromGameFolder(filename);

		if (textList.Count != fileByLine.Length) {
			Debug.Log($"ERROR: wrong number of lines in the Pirate Flavor file!\nShould have {textList.Count} but actually has {fileByLine.Length}");
		}

		for (int i = 0; i < fileByLine.Length; i++) 
		{
			string[] texts = fileByLine[i].Split(lineDelimiter);
			for (int j = 0; j < texts.Length; j++) 
			{
				if (texts[j] != "") {
					string addText = StripAndAddNewlines(texts[j], newline);
					textList[i].Add(addText);
				}
			}
		}
	}

	public static List<DialogText> LoadPortDialog() 
	{
		List<DialogText> textList = new List<DialogText>();

		char[] lineDelimiter = new char[] { '@' };
		char newline = '%';
		string filename = "port_dialog";

		string[] fileByLine = TryLoadListFromGameFolder(filename);

		for (int i = 1; i < fileByLine.Length; i++) 
		{
			string[] texts = fileByLine[i].Split(lineDelimiter);
			string content = StripAndAddNewlines(texts[0], newline);
			DialogText t = new DialogText(texts[1], texts[2], content);
			textList.Add(t);
		}

		return textList;
	}

	public static void LoadTavernaGameBarks(out List<string> insults, out List<string> bragging) {

		insults = new List<string>();
		bragging = new List<string>();

		char[] lineDelimiter = new char[] { '@' };
		char newline = '%';
		string filename = "taverna_game_barks";

		string[] fileByLine = TryLoadListFromGameFolder(filename);

		for (int i = 0; i < fileByLine.Length; i++) {
			string[] texts = fileByLine[i].Split(lineDelimiter);
			string content = StripAndAddNewlines(texts[0], newline);
			if (texts[1] == "insult") {
				insults.Add(content);
			}
			else if (texts[1] == "bragging") {
				bragging.Add(content);
			}
			else {
				Debug.Log($"Taverna bark line {i} not marked insult or bragging");
			}
		}

	}

	public static void LoadPetteiaText(out List<string> flavor, out List<string> insults, out List<string> bragging, out List<string> win, out List<string> lose, out List<string> blocked) 
	{
		flavor = new List<string>();
		insults = new List<string>();
		bragging = new List<string>();
		win = new List<string>();
		lose = new List<string>();
		blocked = new List<string>();

		char[] lineDelimiter = new char[] { '@' };
		char newline = '%';
		string filename = "petteia_text";

		string[] fileByLine = TryLoadListFromGameFolder(filename);

		for (int i = 0; i < fileByLine.Length; i++) 
		{
			string[] texts = fileByLine[i].Split(lineDelimiter);
			string content = StripAndAddNewlines(texts[0], newline);
			switch (texts[1]) {
				case "flavor":
					flavor.Add(content);
					break;
				case "insult":
					insults.Add(content);
					break;
				case "brag":
					bragging.Add(content);
					break;
				case "blocked":
					blocked.Add(content);
					break;
				case "win":
					win.Add(content);
					break;
				case "lose":
					lose.Add(content);
					break;
				default:
					Debug.Log($"Petteia text line {i} not marked correctly: {texts[1]}!");
					break;
			}
		}
	}

	public static void LoadUrText(out List<string> intros, out List<string> rosette, out List<string> capture, out List<string> flip, out List<string> moveOff,
		out List<string> moveOn, out List<string> lose, out List<string> win, out List<string> insult) 
	{
		intros = new List<string>();
		rosette = new List<string>();
		capture = new List<string>();
		flip = new List<string>();
		moveOff = new List<string>();
		moveOn = new List<string>();
		lose = new List<string>();
		win = new List<string>();
		insult = new List<string>();

		char[] lineDelimiter = new char[] { '@' };
		char newline = '%';
		string filename = "ur_text";

		string[] fileByLine = TryLoadListFromGameFolder(filename);

		for (int i = 0; i < fileByLine.Length; i++) {
			string[] texts = fileByLine[i].Split(lineDelimiter);
			string content = StripAndAddNewlines(texts[0], newline);
			switch (texts[1]) {
				case "rosette":
					rosette.Add(content);
					break;
				case "capture":
					capture.Add(content);
					break;
				case "flip":
					flip.Add(content);
					break;
				case "off":
					moveOff.Add(content);
					break;
				case "on":
					moveOn.Add(content);
					break;
				case "lose":
					lose.Add(content);
					break;
				case "win":
					win.Add(content);
					break;
				case "insult":
					insult.Add(content);
					break;
				case "intro":
					insult.Add(content);
					break;
				default:
					Debug.Log($"Ur text line {i} not marked correctly: {texts[1]}!");
					break;
			}
		}
	}

	public static void LoadTavernaDialog(out List<DialogPair> network, out List<DialogPair> pirates, out List<DialogPair> myths, out List<string> guide, 
		out List<string> foodQuotes, out List<FoodText> foods, out List<FoodText> wines) 
	{
		network = new List<DialogPair>();
		pirates = new List<DialogPair>();
		myths = new List<DialogPair>();
		guide = new List<string>();
		foodQuotes = new List<string>();
		foods = new List<FoodText>();
		wines = new List<FoodText>();

		char[] lineDelimiter = new char[] { '@' };
		char newline = '%';
		string filename = "taverna_text";

		string[] fileByLine = TryLoadListFromGameFolder(filename);

		for (int i = 0; i < fileByLine.Length; i++) 
		{
			string[] texts = fileByLine[i].Split(lineDelimiter);
			string content;
			string cityName;
			string question;
			string answer;
			DialogPair dialogText;
			FoodText foodText;

			switch (texts[0]) 
			{
				case "network":
					cityName = texts[1];
					question = StripAndAddNewlines(texts[2], newline);
					answer = StripAndAddNewlines(texts[3], newline);
					dialogText = new DialogPair(cityName, question, answer);
					network.Add(dialogText);
					break;
				case "pirate":
					cityName = texts[1];
					question = StripAndAddNewlines(texts[2], newline);
					answer = StripAndAddNewlines(texts[3], newline);
					dialogText = new DialogPair(cityName, question, answer);
					pirates.Add(dialogText);
					break;
				case "myth":
					cityName = texts[1];
					question = StripAndAddNewlines(texts[2], newline);
					answer = StripAndAddNewlines(texts[3], newline);
					dialogText = new DialogPair(cityName, question, answer);
					myths.Add(dialogText);
					break;
				case "guide":
					content = StripAndAddNewlines(texts[1], newline);
					guide.Add(content);
					break;
				case "food quote":
					content = StripAndAddNewlines(texts[1], newline);
					foodQuotes.Add(content);
					break;
				case "food":
					content = StripAndAddNewlines(texts[2], newline);
					foodText = new FoodText(texts[1], content, FoodText.Type.Food);
					foods.Add(foodText);
					break;
				case "wine":
					content = StripAndAddNewlines(texts[2], newline);
					foodText = new FoodText(texts[1], content, FoodText.Type.Wine);
					wines.Add(foodText);
					break;
				default:
					Debug.Log($"Taverna text line {i} not marked correctly: {texts[0]}");
					break;
			}
		}
	}

	//Mylo's Addition
	//public static List<DialogText> LoadNetworkDialog() {
	//	List<DialogText> textList = new List<DialogText>();

	//	char[] lineDelimeter = new char[] { '@' };
	//	char newline = '%';
	//	string filename = "taverna_network_questions";

	//	string[] fileByLine = TryLoadListFromGameFolder(filename);

	//	for (int i = 1; i < fileByLine.Length; i++) {
	//		string[] texts = fileByLine[i].Split(lineDelimeter);
	//		string content = StripAndAddNewlines(texts[2], newline);
	//		//DialogText t = new DialogText(texts[0], new string[] { texts[1], content });
	//		DialogText t = new DialogText(texts[0], new string[] { texts[1], content });
	//		textList.Add(t);
	//	}

	//	//Debug.Log("CITY: " + textList[2].CityType);
	//	//Debug.Log("Q " + textList[2].TextQA[0]);
	//	//Debug.Log("A " + textList[2].TextQA[1]);

	//	return textList;

	//}

	//public static List<DialogText> LoadPirateDialog() {
	//	List<DialogText> textList = new List<DialogText>();

	//	char[] lineDelimeter = new char[] { '@' };
	//	char newline = '%';
	//	string filename = "taverna_pirate_questions";

	//	string[] fileByLine = TryLoadListFromGameFolder(filename);

	//	for (int i = 1; i < fileByLine.Length; i++) {
	//		string[] texts = fileByLine[i].Split(lineDelimeter);
	//		string content = StripAndAddNewlines(texts[2], newline);
	//		DialogText t = new DialogText(texts[0], new string[] { texts[1], content });
	//		textList.Add(t);
	//	}

	//	//Debug.Log("Pirate: " + textList[2].CityType);
	//	//Debug.Log("Q " + textList[2].TextQA[0]);
	//	//Debug.Log("A " + textList[2].TextQA[1]);

	//	return textList;

	//}

	//public static List<DialogText> LoadMythDialog() {
	//	List<DialogText> textList = new List<DialogText>();

	//	char[] lineDelimeter = new char[] { '@' };
	//	char newline = '%';
	//	string filename = "taverna_myth_questions";

	//	string[] fileByLine = TryLoadListFromGameFolder(filename);

	//	for (int i = 1; i < fileByLine.Length; i++) {
	//		string[] texts = fileByLine[i].Split(lineDelimeter);
	//		string content = StripAndAddNewlines(texts[2], newline);
	//		DialogText t = new DialogText(texts[0], new string[] { texts[1], content });
	//		textList.Add(t);
	//	}

	//	//Debug.Log("Myth: " + textList[2].CityType);
	//	//Debug.Log("Q " + textList[2].TextQA[0]);
	//	//Debug.Log("A " + textList[2].TextQA[1]);

	//	return textList;

	//}

	//public static List<DialogText> LoadHireGuideDialog() {
	//	List<DialogText> textList = new List<DialogText>();

	//	char[] lineDelimeter = new char[] { '@' };
	//	char newline = '%';
	//	string filename = "taverna_guide_hire";

	//	string[] fileByLine = TryLoadListFromGameFolder(filename);

	//	for (int i = 1; i < fileByLine.Length; i++) {
	//		string[] texts = fileByLine[i].Split(lineDelimeter);
	//		string content = StripAndAddNewlines(texts[0], newline);
	//		DialogText t = new DialogText(content, new string[] { texts[1], texts[2] });
	//		textList.Add(t);
	//	}

	//	return textList;

	//}

	//// Food Items load
	//public static List<FoodText> LoadFoodItemsList() {
	//	List<FoodText> foodList = new List<FoodText>();

	//	char[] lineDelimeter = new char[] { '@' };
	//	char newline = '%';
	//	string filename = "taverna_food_list";

	//	string[] fileByLine = TryLoadListFromGameFolder(filename);

	//	for (int i = 2; i < fileByLine.Length - 1; i++) {
	//		string[] texts = fileByLine[i].Split(lineDelimeter);
	//		string content = StripAndAddNewlines(texts[3], newline);
	//		FoodText f = new FoodText();

	//		//f.Source = texts[0];
	//		//f.Chapter = texts[1];
	//		f.Item = texts[2];
	//		f.Quote = content;
	//		//f.Speaker = texts[4];


	//		foodList.Add(f);
	//	}

	//	return foodList;

	//}

	//// Food Dialogue load
	//public static List<FoodText> LoadFoodDialogList() {
	//	List<FoodText> foodList = new List<FoodText>();

	//	char[] lineDelimeter = new char[] { '@' };
	//	char newline = '%';
	//	string filename = "taverna_food_dialog";

	//	string[] fileByLine = TryLoadListFromGameFolder(filename);

	//	for (int i = 2; i < fileByLine.Length - 1; i++) {
	//		string[] texts = fileByLine[i].Split(lineDelimeter);
	//		string content = StripAndAddNewlines(texts[2], newline);
	//		FoodText f = new FoodText();

	//		//f.Source = texts[0];
	//		//f.Chapter = texts[1];
	//		f.Quote = content;
	//		//f.Speaker = texts[3];
	//		//f.Scenario = texts[4];


	//		foodList.Add(f);
	//	}

	//	return foodList;

	//}

	//// Wine Info load
	//public static List<FoodText> LoadWineInfoList() {
	//	List<FoodText> foodList = new List<FoodText>();

	//	char[] lineDelimeter = new char[] { '@' };
	//	char newline = '%';
	//	string filename = "taverna_wine_list";

	//	string[] fileByLine = TryLoadListFromGameFolder(filename);

	//	for (int i = 1; i < fileByLine.Length - 1; i++) {
	//		string[] texts = fileByLine[i].Split(lineDelimeter);
	//		string content = StripAndAddNewlines(texts[3], newline);
	//		FoodText f = new FoodText();

	//		//f.Source = texts[0];
	//		//f.Chapter = texts[1];
	//		f.Item = texts[2];
	//		f.Quote = content;

	//		foodList.Add(f);
	//	}

	//	return foodList;

	//}
	// End Mylo's Addition


	static string TryLoadFromGameFolder(string filename) {
		try {
			var localFile = "";
			var filePath = Application.dataPath + "/Resources/" + filename + ".txt";
			if (File.Exists(filePath)) {
				localFile = File.ReadAllText(filePath);
			}
			else {
				Debug.Log(filename + " does not exist!");
			}

			//Debug.Log(Application.dataPath + "/" + filename + ".txt");
			//Debug.Log(localFile);
			if (localFile == "") {
				TextAsset file = (TextAsset)Resources.Load(filename, typeof(TextAsset));
				return file.text;
			}
			return localFile;

		}
		catch (Exception error) {
			Debug.Log("Sorry! No file: " + filename + " was found in the game directory '" + Application.dataPath + "' or the save file is corrupt!\nError Code: " + error);
			TextAsset file = (TextAsset)Resources.Load(filename, typeof(TextAsset));
			return file.text;
		}

	}

	static string[] TryLoadListFromGameFolder(string filename) {
		string[] splitFile = new string[] { "\r\n", "\r", "\n" };

		string filetext = TryLoadFromGameFolder(filename);
		string[] fileByLine = filetext.Split(splitFile, StringSplitOptions.None);

		// remove any trailing newlines since the parsers assume there's no newline at the end of the file, but VS auto-adds one
		return fileByLine
			.Where(line => !string.IsNullOrEmpty(line))
			.ToArray();
	}

	static string StripAndAddNewlines(string modify, char newline) {
		string s = modify.Replace(newline, '\n');
		if (s[0] == '\"') 
		{
			s = s.Substring(1, s.Length - 2);
		}
		
		return s;
	}
}
