//David Herrod	
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.UI;

public class UrDiceRoller : MonoBehaviour
{
	public float skipTurnWaitTime = 1.5f;
	public Text diceResultText;
	public Animator[] diceModels;
	public Transform[] markUpPositions;
	public Transform[] blankUpPositions;
	public float diceSpinTime;
	public float diceSpeed;

	private UrGameController urGC;

	private void Start() {
		urGC = GetComponent<UrGameController>();
	}

	private IEnumerator RollAndRotate(Animator anim, string trigger) 
	{
		yield return null;
		anim.SetTrigger("Reset");
		anim.transform.rotation = Quaternion.identity;
		anim.SetTrigger(trigger);
		yield return null;
		anim.ResetTrigger("Reset");
		yield return null;
		anim.transform.eulerAngles += Vector3.up * Random.Range(1f, 361f);
	}

	public int RollDice(bool playerTurn)
	{
		//1 is a blank, 2 is a mark
		int[] diceRolls = new int[diceModels.Length];

		for (int i = 0; i < diceRolls.Length; i++) {
			diceRolls[i] = Random.Range(1, 3);
		}

		//Calculate - we're hard-coding it to 3 because there's not really a nice formula for the roll
		int marks = (diceRolls[0] % 2 == 0 ? 1 : 0) + (diceRolls[1] % 2 == 0 ? 1 : 0) + (diceRolls[2] % 2 == 0 ? 1 : 0);
		int roll = marks == 3 ? 5 : marks;

		if (!urGC.IsGameOver) 
		{
			StartCoroutine(VisualDiceRoll(diceRolls, roll, playerTurn));
		}
		
		return roll;
	}

	private IEnumerator VisualDiceRoll(int[] diceRolls, int resultRoll, bool playerTurn) 
	{
		//Visually rotate the dice so they look like they're rolling
		//I TRIED to do it procedurally by adding to the transform.rotation, but it wouldn't work
		//First I tried Euler angles and kept having issues with them not being unique and with gimble lock
		//Then I tried Transform.Rotate, but sometimes it would cause massive framerate dips
		//So I had to give up and do animations instead

		for (int i = 0; i < diceModels.Length; i++) 
		{
			int suffix = Random.Range(1, 3);
			//1 is blank, 2 is mark
			string trigger = diceRolls[i] == 1 ? "RollBlank" : "RollMark";
			trigger += suffix.ToString();
			StartCoroutine(RollAndRotate(diceModels[i], trigger));
		}

		yield return new WaitForSeconds(diceSpinTime);

		diceResultText.text = resultRoll.ToString();

		if (playerTurn) 
		{
			if (!urGC.CanPlayerMove(true)) 
			{
				urGC.ShowAlertText("No Available Moves");
				StartCoroutine(urGC.WaitToSwitchTurn(false, skipTurnWaitTime));
			}
			
		}
		else 
		{
			if (!urGC.CanPlayerMove(false, false)) 
			{
				urGC.ShowAlertText("Opponent Has No Moves");
				StartCoroutine(urGC.WaitToSwitchTurn(true, skipTurnWaitTime));
			}
			else 
			{
				StartCoroutine(urGC.enemyAI.DoEnemyTurn());
			}
		}

	}
}


