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

		SetTextLists();

	}

	//private void Update() {
	//	if (Input.GetKeyDown(KeyCode.Space)) {
	//		if (Random.Range(1, 3) % 2 == 0) {
	//			DisplayInsult();
	//		}
	//		else {
	//			DisplayBragging();
	//		}
	//	}
	//}
}
