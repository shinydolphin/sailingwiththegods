using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UrAIController : MonoBehaviour
{
	public List<UrPiece> enemyPieces;

	private UrGameController urGC;
	private int currentRoll;

	private void Start() 
	{
		urGC = GetComponent<UrGameController>();
	}

	public void EnemyTurn() 
	{
		if (!urGC.IsGameOver) 
		{
			currentRoll = urGC.GetDiceRoll();
		}
	}

	public void DoEnemyTurn() 
	{
		//Chooses what value the dice roll

		//int[] i = { 0, 1, 4, 5 };
		//int sel = Random.Range(0, 4);
		//int ediceValue = i[sel];
		//dvText.text = "" + ediceValue;

		//If the enemy can move a piece onto the board, do it

		if (currentRoll != 0) 
		{
			List<UrPiece> movablePieces = new List<UrPiece>();
			for (int i = 0; i < enemyPieces.Count; i++) {
				if (enemyPieces[i].PopulateValidMovesList(urGC.enemyBoardPositions).Count != 0) 
				{
					movablePieces.Add(enemyPieces[i]);
				}
			}
			bool redoTurn = false;
			if (movablePieces.Count > 0) 
			{
				UrPiece pieceToMove = movablePieces.RandomElement();
				//If we're moving it onto the board, it's only got one potential move. Otherwise, it has two - its current space and the next one
				int validMovePos = pieceToMove.BoardIndex == -1 ? 0 : 1;
				UrGameTile nextTile = pieceToMove.PopulateValidMovesList(urGC.enemyBoardPositions)[validMovePos];
				pieceToMove.transform.position = nextTile.transform.position;
				if (pieceToMove.BoardIndex != -1) {
					urGC.enemyBoardPositions[pieceToMove.BoardIndex].Occupied = false;
				}
				nextTile.Occupied = true;
				//Since we know that this space + currentRoll is a valid move, we don't have to check it again
				pieceToMove.BoardIndex += currentRoll;
				if (nextTile.isRosette) {
					urGC.ShowAlertText("Opponent Rolls Again");
					redoTurn = true;
				}
			}

			StartCoroutine(urGC.WaitToSwitchTurn(!redoTurn, 2.5f));
		}

			//if (ediceValue == 1 && enemyCountersOnBoard == 0) {
			//	eCounters[0].PlaceOnBoard(enemyBoardPositions[0], false, true, false);
			//	enemyCountersOnBoard++;
			//}
			//else if (ediceValue == 5 && enemyCountersOnBoard == 0) {
			//	eCounters[0].PlaceOnBoard(enemyBoardPositions[4], false, true, false);
			//	enemyCountersOnBoard++;
			//}
			//else if (ediceValue == 1 && enemyCountersOnBoard > 0 && !IsEnemySpaceOccupied(enemyBoardPositions[0])) {
			//	eCounters[enemyCountersOnBoard].PlaceOnBoard(enemyBoardPositions[0], false, true, false);
			//	enemyCountersOnBoard++;
			//}
			//else if (ediceValue == 5 && enemyCountersOnBoard > 0 && !IsEnemySpaceOccupied(enemyBoardPositions[4])) {
			//	eCounters[enemyCountersOnBoard].PlaceOnBoard(enemyBoardPositions[4], false, true, false);
			//	enemyCountersOnBoard++;
			//}

		//If the enemy can't move a piece onto the board, but does have some pieces on the board already

		//else if (enemyCountersOnBoard > 0) 
		//{
		//	//If the enemy rolled a 1
		//	if (ediceValue == 1) 
		//	{
		//		foreach (UrCounter c in eCounters) 
		//		{
		//			//Check if the piece can be moved
		//			if (c.onBoard && CanEnemyMove(ediceValue, c)) 
		//			{
		//				Debug.Log("i get here");
		//				int index = enemyBoardPositions.IndexOf(c.currentTile);
		//				//Check if the potential space is unoccupied and not off the end
		//				if (!IsEnemySpaceOccupied(enemyBoardPositions[index + 1]) && ((index + 1) < 13)) 
		//				{
		//					//Move this piece
		//					c.PlaceOnBoard(enemyBoardPositions[index + 1], false, true, false);
		//					//If there's already a player piece there, remove it from the board
		//					if (IsSpaceOccupied(playerBoardPositions[index + 1])) 
		//					{
		//						Debug.Log("haha rekt");
		//						IsSpaceOccupiedCounter(playerBoardPositions[index + 1]).onBoard = false;
		//						IsSpaceOccupiedCounter(playerBoardPositions[index + 1]).transform.position = IsSpaceOccupiedCounter(playerBoardPositions[index + 1]).initPosit;
		//						IsSpaceOccupiedCounter(playerBoardPositions[index + 1]).currentTile = null;
		//						break;
		//					}
		//				}
		//				//Check if the potential space is unoccupied and is off the board
		//				else if (!IsEnemySpaceOccupied(enemyBoardPositions[index + 1]) && ((index + 1) >= 13)) 
		//				{
		//					//Score a point
		//					if (index + 1 == 19) 
		//					{
		//						PointScored(false);
		//					}
		//					//Still move the piece and check for the player (unnecessary)
		//					c.PlaceOnBoard(enemyBoardPositions[index + 1], true, true, false);
		//					if (IsSpaceOccupied(playerBoardPositions[index + 1])) 
		//					{
		//						Debug.Log("haha rekt");
		//						IsSpaceOccupiedCounter(playerBoardPositions[index + 1]).onBoard = false;
		//						IsSpaceOccupiedCounter(playerBoardPositions[index + 1]).transform.position = IsSpaceOccupiedCounter(playerBoardPositions[index + 1]).initPosit;
		//						IsSpaceOccupiedCounter(playerBoardPositions[index + 1]).currentTile = null;
		//						break;
		//					}

		//				}

		//			}

		//		}
		//	}

			//The EXACT SAME CODE, but not just using ediceValue instead of a hard-coded number
				//else if (ediceValue == 4) 
				//{
				//	foreach (UrCounter c in eCounters) 
				//	{
				//		if (c.onBoard && CanEnemyMove(ediceValue, c)) 
				//		{
				//			Debug.Log("i get here");
				//			int index = enemyBoardPositions.IndexOf(c.currentTile);
				//			if (!IsEnemySpaceOccupied(enemyBoardPositions[index + 4]) && ((index + 4) < 13)) 
				//			{
				//				c.PlaceOnBoard(enemyBoardPositions[index + 4], false, true, false);
				//				if (IsSpaceOccupied(playerBoardPositions[index + 4])) {
				//					Debug.Log("haha rekt");
				//					IsSpaceOccupiedCounter(playerBoardPositions[index + 4]).onBoard = false;
				//					IsSpaceOccupiedCounter(playerBoardPositions[index + 4]).transform.position = IsSpaceOccupiedCounter(playerBoardPositions[index + 4]).initPosit;
				//					IsSpaceOccupiedCounter(playerBoardPositions[index + 4]).currentTile = null;
				//					break;
				//				}
				//			}
				//			else if (!IsEnemySpaceOccupied(enemyBoardPositions[index + 4]) && ((index + 4) >= 13)) 
				//			{
				//				if (index + 4 == 19) {
				//					PointScored(false);
				//				}
				//				c.PlaceOnBoard(enemyBoardPositions[index + 4], true, true, false);
				//				if (IsSpaceOccupied(playerBoardPositions[index + 4])) 
				//				{
				//					Debug.Log("haha rekt");
				//					IsSpaceOccupiedCounter(playerBoardPositions[index + 4]).onBoard = false;
				//					IsSpaceOccupiedCounter(playerBoardPositions[index + 4]).transform.position = IsSpaceOccupiedCounter(playerBoardPositions[index + 4]).initPosit;
				//					IsSpaceOccupiedCounter(playerBoardPositions[index + 4]).currentTile = null;
				//					break;
				//				}

				//			}

				//		}
				//	}
				//}
				//else if (ediceValue == 5) 
				//{
				//	foreach (UrCounter c in eCounters) 
				//	{
				//		if (c.onBoard && CanEnemyMove(ediceValue, c)) 
				//		{
				//			Debug.Log("i get here");
				//			int index = enemyBoardPositions.IndexOf(c.currentTile);
				//			if (!IsEnemySpaceOccupied(enemyBoardPositions[index + 5]) && ((index + 5) < 13)) 
				//			{
				//				c.PlaceOnBoard(enemyBoardPositions[index + 5], false, true, false);
				//				if (IsSpaceOccupied(playerBoardPositions[index + 5]))
				//				{
				//					Debug.Log("haha rekt");
				//					IsSpaceOccupiedCounter(playerBoardPositions[index + 5]).onBoard = false;
				//					IsSpaceOccupiedCounter(playerBoardPositions[index + 5]).transform.position = IsSpaceOccupiedCounter(playerBoardPositions[index + 5]).initPosit;
				//					IsSpaceOccupiedCounter(playerBoardPositions[index + 5]).currentTile = null;
				//					break;
				//				}
				//			}
				//			else if (!IsEnemySpaceOccupied(enemyBoardPositions[index + 5]) && ((index + 5) >= 13)) 
				//			{
				//				if (index + 5 == 19) 
				//				{
				//					PointScored(false);
				//				}
				//				c.PlaceOnBoard(enemyBoardPositions[index + 5], true, true, false);
				//				if (IsSpaceOccupied(playerBoardPositions[index + 5])) 
				//				{
				//					Debug.Log("haha rekt");
				//					IsSpaceOccupiedCounter(playerBoardPositions[index + 5]).onBoard = false;
				//					IsSpaceOccupiedCounter(playerBoardPositions[index + 5]).transform.position = IsSpaceOccupiedCounter(playerBoardPositions[index + 5]).initPosit;
				//					IsSpaceOccupiedCounter(playerBoardPositions[index + 5]).currentTile = null;
				//					break;
				//				}

				//			}

				//		}
				//	}
				//}
		//}
		////rollDiceButton.SetActive(true);
	}

}
