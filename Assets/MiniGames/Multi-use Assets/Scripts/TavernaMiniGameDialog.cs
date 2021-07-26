using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TavernaMiniGameDialog : MonoBehaviour
{
	public enum MinigameType { Petteia, Ur }
	public MinigameType gameType;
	public GameObject textBackground;
	public Text dialog;

	protected List<string> braggingTexts;
	protected List<string> insultingTexts;

	private void Start() 
	{
		textBackground.SetActive(false);

		SetTextLists();
	}

	protected void SetTextLists() 
	{
		if (gameType == MinigameType.Petteia) 
		{
			//petteia brag/insult
			if (Globals.GameVars != null) {
				insultingTexts = Globals.GameVars.petteiaGameInsults;
				braggingTexts = Globals.GameVars.petteiaGameBragging;
			}
			else {
				insultingTexts = new List<string> { "Petteia insult 1", "Petteia insult 2", "Petteia insult 3" };
				braggingTexts = new List<string> { "Petteia brag 1", "Petteia brag 2", "Petteia brag 3" };
			}
		}
		else 
		{
			//ur brag/insult
			if (Globals.GameVars != null) {
				insultingTexts = Globals.GameVars.tavernaGameInsults;
				braggingTexts = Globals.GameVars.tavernaGameBragging;
			}
			else {
				insultingTexts = new List<string> { "Ur insult 1", "Ur insult 2", "Ur insult 3" };
				braggingTexts = new List<string> { "Ur brag 1", "Ur brag 2", "Ur brag 3" };
			}
		}
	}

	/// <summary>
	/// Displays an insult
	/// </summary>
	public void DisplayInsult() {
		Time.timeScale = 0;
		textBackground.SetActive(true);
		dialog.text = insultingTexts.RandomElement();
	}

	/// <summary>
	/// Displays a brag
	/// </summary>
	public void DisplayBragging() {
		Time.timeScale = 0;
		textBackground.SetActive(true);
		dialog.text = braggingTexts.RandomElement();
	}

	public void CloseDialog() {
		Time.timeScale = 1;
		textBackground.SetActive(false);
	}
}
