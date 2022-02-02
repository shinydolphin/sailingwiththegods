using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RitualController : MonoBehaviour
{

	public enum RitualResult { Success, Failure, Refusal }

	[Header("General")]
	[Range(0f, 1f)]
	public float noResourcesMod = 0.5f;
	public MiniGameInfoScreen mgInfo;
	[TextArea(2, 15)]
	public string instructionsText;
	public Sprite stormIcon;

	[Header("Clout")]
	public int refusalLoss = 15;
	public Vector2Int survivalGain = new Vector2Int(5, 25);

	[Header("End-Game Health")]
	public float[] damageLevelPercents;
	[TextArea(2, 10)]
	public string[] damageLevelText;

	[Header("Buttons")]
	public ButtonExplanation performButton;
	public ButtonExplanation rejectButton;
	public Button startButton;
	public Button finishButton;
	public string winFinishText = "You escaped!";
	public string loseFinishText = "Game over!";

	private Ritual currentRitual = null;
	private CrewMember currentCrew = null;
	private int cloutChange = 0;
	private RandomizerForStorms rfs;


	private void Start() 
	{
		rfs = GetComponent<RandomizerForStorms>();
		DisplayStartingText();
		Globals.Game.Session.playerShip.GetComponent<script_player_controls>().cursorRing.SetActive(false);
	}

	public void DisplayStartingText() 
	{
		mgInfo.gameObject.SetActive(true);
		mgInfo.DisplayText(
			Globals.Database.stormTitles[0], 
			Globals.Database.stormSubtitles[0], 
			Globals.Database.stormStartText[0] + "\n\n" + instructionsText + "\n\n" + Globals.Database.stormStartText[Random.Range(1, Globals.Database.stormStartText.Count)], 
			stormIcon, 
			MiniGameInfoScreen.MiniGame.StormStart);
	}

	public void ChooseRitual() 
	{
		//Determine if the player has a seer or not
		List<Ritual> possibleRituals = new List<Ritual>();

		bool hasSeer = CheckForSeer();

		possibleRituals = Globals.Database.stormRituals.FindAll(x => x.HasSeer == hasSeer);

		//Select an appropriate ritual
		currentRitual = possibleRituals[RandomIndex(possibleRituals)];
		currentCrew = Globals.Game.Session.playerShipVariables.ship.crewRoster[RandomIndex(Globals.Game.Session.playerShipVariables.ship.crewRoster)];

		DisplayRitualText();
	}

	public void DisplayRitualText() 
	{
		string ritualText = currentRitual.RitualText;
		string introText = currentRitual.HasSeer ? Globals.Database.stormSeerText[0] : Globals.Database.stormNoSeerText[0];
		string closeText = currentRitual.HasSeer ? Globals.Database.stormSeerText[Random.Range(1, Globals.Database.stormSeerText.Count)] : 
			Globals.Database.stormNoSeerText[Random.Range(1, Globals.Database.stormNoSeerText.Count)];
		string finalRitualText = introText + "\n\n" + ritualText.Replace("{0}", currentCrew.name) + "\n\n" + closeText;

		mgInfo.DisplayText(Globals.Database.stormTitles[1], Globals.Database.stormSubtitles[1], finalRitualText, stormIcon, MiniGameInfoScreen.MiniGame.Storm);

		bool hasResources = CheckResources();
		string performText = "";
		if (!hasResources) 
		{
			mgInfo.AddToText("\n\nUnfortunately, you are missing some needed resources and will have a harder time with the ritual!");
			performText += $"{currentRitual.SuccessChance * noResourcesMod * 100}% Success Chance";
		}
		else 
		{
			performText += $"{currentRitual.SuccessChance * 100}% Success Chance";
		}

		
		for (int i = 0; i < currentRitual.ResourceTypes.Length; i++) 
		{
			switch (currentRitual.ResourceTypes[i]) 
			{
				case (-1):
					performText += $"\n{currentCrew.name} will die as a sacrifice";
					break;
				case (-2):
					performText += $"\n-{currentRitual.ResourceAmounts[i]} Drachma (You have {Globals.Game.Session.playerShipVariables.ship.currency} Drachma)";
					break;
				default:
					performText += $"\n-{currentRitual.ResourceAmounts[i]}kg {Globals.Database.masterResourceList[currentRitual.ResourceTypes[i]].name} " +
						$"(You have {Globals.Game.Session.playerShipVariables.ship.cargo[currentRitual.ResourceTypes[i]].amount_kg}kg)";
					break;
			}
		}

		if (!hasResources) 
		{
			performText += "\nYou are missing resources, so you will use what you have and hope for the best";
		}

		performButton.SetExplanationText(performText);
		rejectButton.SetExplanationText("If you refuse to do the ritual, the storm will surely get worse!");
	}

	/// <summary>
	/// Calculates if a ritual is a success or not, subtracts any resources, and updates the text
	/// </summary>
	/// <param name="action">0 or greater performs the ritual, less than 0 rejects it</param>
	public void CalculateRitualResults(int action) 
	{
		RandomizerForStorms.StormDifficulty result = RandomizerForStorms.StormDifficulty.Error;
		string extraText = "";
		string cloutText = "";

		if (action >= 0) {
			//Ritual is being performed
			float mod = CheckResources() ? 1 : noResourcesMod;
			float check = Random.Range(0.0f, 1.0f);
			result = check < (currentRitual.SuccessChance * mod) ? RandomizerForStorms.StormDifficulty.Easy : RandomizerForStorms.StormDifficulty.Medium;
			if (result == RandomizerForStorms.StormDifficulty.Easy) {
				cloutText = $"\n\nYour successful ritual has raised your clout by {currentRitual.CloutGain}.";
				cloutChange = currentRitual.CloutGain;
			}
			else {
				cloutText = $"\n\nYour failed ritual has lowered your clout by {currentRitual.CloutLoss}.";
				cloutChange = -currentRitual.CloutLoss;
			}
			SubtractCosts();
		}
		else {
			//Ritual was rejected
			result = RandomizerForStorms.StormDifficulty.Hard;
			cloutText = $"\n\nYou decision to reject the gods and refuse to perform a ritual has made some of your crew nervous, and your clout has decreased by {refusalLoss}";
			cloutChange = -refusalLoss;
		}

		mgInfo.DisplayText(
			Globals.Database.stormTitles[2], 
			Globals.Database.stormSubtitles[2], 
			result != RandomizerForStorms.StormDifficulty.Error ? extraText + Globals.Database.stormRitualResultsText[(int)result] + cloutText : "something went wrong", 
			stormIcon, 
			MiniGameInfoScreen.MiniGame.Start);

		startButton.onClick.RemoveAllListeners();
		startButton.onClick.AddListener(mgInfo.CloseDialog);
		startButton.onClick.AddListener(rfs.StartDamageTimer);
		startButton.onClick.AddListener(() => rfs.move.ToggleMovement(true));

		//Send the result to the difficulty calculator for the storm
		GetComponent<RandomizerForStorms>().SetDifficulty(result);
	}

	private bool CheckResources() 
	{
		//Make sure you remember: -1 is a crewmember, -2 is money
		bool hasResources = true;

		for (int i = 0; i < currentRitual.ResourceTypes.Length; i++) 
		{
			if (currentRitual.ResourceTypes[i] == -2) {
				hasResources = hasResources && (Globals.Game.Session.playerShipVariables.ship.currency > currentRitual.ResourceAmounts[i]);
			}
			else if (currentRitual.ResourceTypes[i] == -1) {
				//skip - you know they have at least one crewmember
			}
			else {
				hasResources = hasResources && (Globals.Game.Session.playerShipVariables.ship.cargo[currentRitual.ResourceTypes[i]].amount_kg > currentRitual.ResourceAmounts[i]);
			}
		}
		return hasResources;
	}

	private void SubtractCosts() 
	{
		for (int i = 0; i < currentRitual.ResourceTypes.Length; i++) {
			switch (currentRitual.ResourceTypes[i]) 
			{
				case (-2):
					Globals.Game.Session.playerShipVariables.ship.currency = Mathf.Max(0, Globals.Game.Session.playerShipVariables.ship.currency - currentRitual.ResourceAmounts[i]);
					break;
				case (-1):
					Globals.Game.Session.playerShipVariables.ship.crewRoster.Remove(currentCrew);
					break;
				default:
					int j = currentRitual.ResourceTypes[i];
					Globals.Game.Session.playerShipVariables.ship.cargo[j].amount_kg = Mathf.Max(0f, Globals.Game.Session.playerShipVariables.ship.cargo[j].amount_kg - currentRitual.ResourceAmounts[i]);
					break;
			}
		}
	}

	private int RandomIndex<T>(IList<T> array) 
	{
		return Random.Range(0, array.Count);
	}

	private bool CheckForSeer() 
	{
		bool hasSeer = false;

		for (int i = 0; i < Globals.Game.Session.playerShipVariables.ship.crew; i++) 
		{
			hasSeer = hasSeer || (Globals.Game.Session.playerShipVariables.ship.crewRoster[i].typeOfCrew == CrewType.Seer);
		}

		return hasSeer;
	}

	public void WinGame()
	{
		rfs.StopDamageTimer();
		ShipHealth h = GetComponent<ShipHealth>();
		//Figure out clout change based on damage taken relative to max health
		float percentDamage = h.Health / h.MaxHealth;
		int damageBracket = RandomizerForStorms.GetBracket(damageLevelPercents, percentDamage);
		int cloutGained = Mathf.CeilToInt((survivalGain.y - survivalGain.x) * percentDamage + survivalGain.x);
		string cloutText = damageLevelText[damageBracket] + "\n\n" + $"For making your way out of the storm with your ship intact, your clout has risen {Mathf.RoundToInt(cloutGained)}." + 
			$" Combined with the {cloutChange} from the ritual, your clout has changed a total of {Mathf.RoundToInt(cloutGained + cloutChange)}.";
		Globals.Game.Session.AdjustPlayerClout(cloutGained + cloutChange, false);

		mgInfo.gameObject.SetActive(true);
		mgInfo.DisplayText(
			Globals.Database.stormTitles[3], 
			Globals.Database.stormSubtitles[3], 
			Globals.Database.stormSuccessText[0] + "\n\n" + cloutText + "\n\n" + Globals.Database.stormSuccessText[Random.Range(1, Globals.Database.stormSuccessText.Count)], 
			stormIcon, 
			MiniGameInfoScreen.MiniGame.Finish);
		finishButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = winFinishText;
		finishButton.onClick.RemoveAllListeners();
		finishButton.onClick.AddListener(UnloadMinigame);
		rfs.move.ToggleMovement(false);
	}

	public void LoseGame() 
	{
		rfs.StopDamageTimer();
		mgInfo.gameObject.SetActive(true);
		mgInfo.DisplayText(
			Globals.Database.stormTitles[3], 
			Globals.Database.stormSubtitles[3], 
			Globals.Database.stormFailureText[0] + "\n\n" + Globals.Database.stormFailureText[Random.Range(1, Globals.Database.stormFailureText.Count)], 
			stormIcon, 
			MiniGameInfoScreen.MiniGame.Finish);
		finishButton.GetComponentInChildren<TMPro.TextMeshProUGUI>().text = loseFinishText;
		finishButton.onClick.RemoveAllListeners();
		finishButton.onClick.AddListener(UnloadMinigame);
		finishButton.onClick.AddListener(EndGame);
		rfs.move.ToggleMovement(false);
		
	}

	public void EndGame() 
	{
		Globals.Game.isGameOver = true;
	}

	public void UnloadMinigame() 
	{
		//UNLOAD MINIGAME CODE GOES HERE
		mgInfo.CloseDialog();
		gameObject.SetActive(false);
		Globals.Game.Session.playerShip.GetComponent<script_player_controls>().cursorRing.SetActive(true);
		Globals.MiniGames.Exit();
	}
}
