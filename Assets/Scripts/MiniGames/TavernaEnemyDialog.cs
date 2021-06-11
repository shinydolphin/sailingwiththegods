using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class TavernaEnemyDialog : TavernaMiniGameDialog
{
	public Text enemyName;
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

		if (Globals.GameVars != null) {
			insultingTexts = Globals.GameVars.tavernaGameInsults;
			braggingTexts = Globals.GameVars.tavernaGameBragging;
		}
		else {
			braggingTexts = new List<string> { "Enemy brag 1", "Enemy brag 2" };
			insultingTexts = new List<string> { "Enemy insult 1", "Enemy insult 2" };
		}

	}
}
