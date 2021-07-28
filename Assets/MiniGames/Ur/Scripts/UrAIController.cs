using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UrAIController : MonoBehaviour
{
	public UrGameController gc;

	public void EnemyTurn() {
		gc.RollDice();
		StartCoroutine(gc.WaitToSwitchTurn(true, 3.75f));
	}
}
