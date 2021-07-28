using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UrAIController : MonoBehaviour
{
	public UrGameController gc;

	private int currentRoll;

	public void EnemyTurn() {
		gc.RollDice();
		StartCoroutine(gc.WaitToSwitchTurn(true, 3.75f));
	}

	private IEnumerator DoEnemyTurn() {
		yield return new WaitForSeconds(1.25f);
		currentRoll = gc.GetDiceRoll();
		StartCoroutine(gc.WaitToSwitchTurn(true, 3f));
	}
}
