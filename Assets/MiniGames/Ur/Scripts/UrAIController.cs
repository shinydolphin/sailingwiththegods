using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UrAIController : MonoBehaviour
{
	public List<UrPiece> enemyPieces;

	public float midTurnPause = 0.5f;
	public float endTurnPause = 1f;

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

	public IEnumerator DoEnemyTurn() 
	{
		if (currentRoll != 0) 
		{
			//Picks out what pieces are valid
			List<UrPiece> movablePieces = new List<UrPiece>();
			for (int i = 0; i < enemyPieces.Count; i++) {
				if (enemyPieces[i].PopulateValidMovesList(urGC.enemyBoardPositions, false).Count != 0) 
				{
					movablePieces.Add(enemyPieces[i]);
				}
			}

			bool redoTurn = false;
			if (movablePieces.Count > 0) 
			{
				//Pick what piece to move
				UrPiece pieceToMove = ChoosePieceToMove(movablePieces);
				pieceToMove.ShowHighlight(true);

				List<UrGameTile> validMoves = pieceToMove.PopulateValidMovesList(urGC.enemyBoardPositions, false);
				foreach (UrGameTile t in validMoves) 
				{
					t.ShowHighlight(true, false);
				}
				//If we're moving it onto the board, it's only got one potential move. Otherwise, it has two - its current space and the next one
				int validMovePos = pieceToMove.BoardIndex == -1 ? 0 : 1;
				UrGameTile nextTile = validMoves[validMovePos];

				yield return new WaitForSeconds(midTurnPause);

				//Visually show the move
				urGC.UnhighlightPieces();
				pieceToMove.SpawnGhostInPlace();
				//Since we know that this space + currentRoll is a valid move, we don't have to check it again
				pieceToMove.ShowPossiblePath(urGC.enemyBoardPositions, pieceToMove.BoardIndex, pieceToMove.BoardIndex + currentRoll);
				pieceToMove.transform.position = nextTile.transform.position;

				yield return new WaitForSeconds(midTurnPause);

				//Finalize the move

				urGC.PlayMoveSound();

				//Check for a capture
				if (nextTile.OppositeOccupyingPiece(false)) 
				{
					nextTile.RemoveCurrentFromBoard();
					urGC.TriggerBark(false, urGC.CaptureFlavor);
				}

				pieceToMove.ClearPossiblePath();
				urGC.UnhighlightBoard();
				pieceToMove.DestroyGhost();

				if (pieceToMove.BoardIndex < 16 && pieceToMove.BoardIndex + currentRoll >= 16) 
				{
					pieceToMove.FlipPiece();
					urGC.TriggerBark(false, urGC.FlipFlavor);
				}

				if (pieceToMove.BoardIndex != -1) 
				{
					urGC.enemyBoardPositions[pieceToMove.BoardIndex].ClearOccupied();
					pieceToMove.BoardIndex += currentRoll;
				}
				else 
				{
					//If they're at at the beginning, we don't want to set their position to 4 if they rolled a 5
					pieceToMove.BoardIndex = 0;
					urGC.TriggerBark(false, urGC.MoveOnFlavor);
				}

				//If you're moving off the board
				if (pieceToMove.BoardIndex == urGC.playerBoardPositions.Count - 1) 
				{
					urGC.TriggerBark(false, urGC.MoveOffFlavor, true);
					urGC.PointScored(false, pieceToMove);
				}
				else {
					nextTile.SetOccupied(pieceToMove);
				}
				
				if (nextTile.isRosette) 
				{
					urGC.ShowAlertText("Opponent Rolls Again");
					urGC.TriggerBark(false, urGC.RosetteFlavor);
					redoTurn = true;
				}
			}

			StartCoroutine(urGC.WaitToSwitchTurn(!redoTurn, endTurnPause));
		}
	}

	private UrPiece ChoosePieceToMove(List<UrPiece> movablePieceList) 
	{
		//If there's only one piece, just return that without wasting time doing the other processing
		if (movablePieceList.Count == 1) 
		{
			//Debug.Log("Only one piece can move, taking that move");
			return movablePieceList[0];
		}

		//I've chosen to do this with one list and multiple loops for a few reasons
		//The method stops as soon as it finds a piece to move
		//Four lists would be a lot of memory when one works just fine
		//Singular for-loops are pretty efficient, especially given none of these will ever run more than 3 times
		//I find this to be nicely readable and debug-able
		List<UrPiece> potentialPieces = new List<UrPiece>();
		
		//1. Move piece off the end of the board
		for (int i = 0; i < movablePieceList.Count; i++) 
		{
			if (movablePieceList[i].BoardIndex + currentRoll == urGC.enemyBoardPositions.Count - 1) 
			{
				potentialPieces.Add(movablePieceList[i]);
			}
		}
		if (potentialPieces.Count != 0) 
		{
			//Debug.Log("At least one piece can move off the board, taking that move");
			return potentialPieces.RandomElement();
		}

		//2. Capture one of the player's pieces
		for (int i = 0; i < movablePieceList.Count; i++) 
		{
			if (urGC.enemyBoardPositions[movablePieceList[i].BoardIndex + currentRoll].OppositeOccupyingPiece(false)) {
				potentialPieces.Add(movablePieceList[i]);
			}
		}
		if (potentialPieces.Count != 0) 
		{
			//Debug.Log("At least one piece can capture a player piece, taking that move");
			return potentialPieces.RandomElement();
		}

		//3. Land on a rosette and roll again
		for (int i = 0; i < movablePieceList.Count; i++) 
		{
			if (urGC.enemyBoardPositions[movablePieceList[i].BoardIndex + currentRoll].isRosette) {
				potentialPieces.Add(movablePieceList[i]);
			}
		}
		if (potentialPieces.Count != 0) 
		{
			//Debug.Log("At least one piece can land on a rosette, taking that move");
			return potentialPieces.RandomElement();
		}

		//4. Move a piece off of the board onto it
		for (int i = 0; i < movablePieceList.Count; i++) 
		{
			//If it's in this list, it can already be moved, so you just need to check if it's off the board
			if (movablePieceList[i].BoardIndex == -1) {
				potentialPieces.Add(movablePieceList[i]);
			}
		}
		if (potentialPieces.Count != 0) 
		{
			//Debug.Log("At least one piece can move onto the board, taking that move");
			return potentialPieces.RandomElement();
		}

		//If no piece can do any of those things, choose at random

		//Debug.Log("No priority moves available, moving a piece at random");
		return movablePieceList.RandomElement();
	}

}
