using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TavernaMiniGameDialog : MonoBehaviour
{
	public GameObject textBackground;
	public Text dialog;

	protected List<string> braggingTexts;
	protected List<string> insultingTexts;

	private void Start() 
	{
		textBackground.SetActive(false);

		//Will be replaced with pulling texts from a CSV
		braggingTexts = new List<string> { "Player brag 1", "Player brag 2" };
		insultingTexts = new List<string> { "Player insult 1", "Player insult 2" };
	}

	/// <summary>
	/// Displays an insult from the opponent to the player - that is, the opponent is upset with the player
	/// </summary>
	public void DisplayInsult() {
		Time.timeScale = 0;
		textBackground.SetActive(true);
		if (Globals.World != null) {
			dialog.text = insultingTexts.RandomElement();
		}
		else {
			dialog.text = "Insult goes here";
		}
	}

	/// <summary>
	/// Displays a brag from the opponent - that is, the opponent is happy
	/// </summary>
	public void DisplayBragging() {
		Time.timeScale = 0;
		textBackground.SetActive(true);
		if (Globals.World != null) {
			dialog.text = braggingTexts.RandomElement();
		}
		else {
			dialog.text = "Bragging goes here";
		}
	}

	public void CloseDialog() {
		Time.timeScale = 1;
		textBackground.SetActive(false);
	}
}
