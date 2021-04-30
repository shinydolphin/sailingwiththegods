using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class TavernaMiniGameDialog : MonoBehaviour
{
	public GameObject textBackground;
	public Text enemyName;
	public Text dialog;
	public Image enemyImage;
	private const string ResourcePath = "crew_portraits";
	private const string DefaultPortrait = "crew_portraits/phoenician_sailor";

	private CrewMember crew;

    void Start()
    {
		textBackground.SetActive(false);

		//These null checks are for testing purposes, so you can run this script in a scene without the main scene loaded additively in the background
		if (Globals.GameVars != null) {
			crew = Globals.GameVars.currentSettlement.availableCrew.RandomElement();
			enemyName.text = crew.name;
			enemyImage.sprite = Resources.Load<Sprite>(ResourcePath + "/" + crew.ID) ?? Resources.Load<Sprite>(DefaultPortrait);
		}

	}

	/// <summary>
	/// Displays an insult from the opponent to the player - that is, the opponent is upset with the player
	/// </summary>
	public void DisplayInsult() {
		Time.timeScale = 0;
		textBackground.SetActive(true);
		if (Globals.GameVars != null) {
			dialog.text = Globals.GameVars.tavernaGameInsults.RandomElement();
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
		if (Globals.GameVars != null) {
			dialog.text = Globals.GameVars.tavernaGameBragging.RandomElement();
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
