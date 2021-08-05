using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UrAIController : MonoBehaviour
{
	public UrGameController gc;

	private int currentRoll;

	public void EnemyTurn() 
	{
		if (!gc.IsGameOver) {
			currentRoll = gc.GetDiceRoll();
			StartCoroutine(gc.WaitToSwitchTurn(true, 3.5f));
		}

	}

}
