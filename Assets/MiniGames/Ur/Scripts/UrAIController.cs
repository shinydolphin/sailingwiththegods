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
		//Chooses what value the dice roll

		//int[] i = { 0, 1, 4, 5 };
		//int sel = Random.Range(0, 4);
		//int ediceValue = i[sel];
		//dvText.text = "" + ediceValue;

		//If the enemy can move a piece onto the board, do it

		if (currentRoll != 0) 
		{
			//Picks out what pieces are valid - they're already highlighted by the dice roller
			List<UrPiece> movablePieces = new List<UrPiece>();
			for (int i = 0; i < enemyPieces.Count; i++) {
				if (enemyPieces[i].PopulateValidMovesList(urGC.enemyBoardPositions).Count != 0) 
				{
					movablePieces.Add(enemyPieces[i]);
				}
			}

			yield return new WaitForSeconds(midTurnPause);

			bool redoTurn = false;
			if (movablePieces.Count > 0) 
			{
				//Pick what piece to move
				UrPiece pieceToMove = ChoosePieceToMove(movablePieces);
				foreach (UrPiece p in movablePieces) 
				{
					if (!p.Equals(pieceToMove)) 
					{
						p.ShowHighlight(false);
					}
				}
				List<UrGameTile> validMoves = pieceToMove.PopulateValidMovesList(urGC.enemyBoardPositions);
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
				pieceToMove.ClearPossiblePath();
				urGC.UnhighlightBoard();
				pieceToMove.DestroyGhost();

				if (pieceToMove.BoardIndex < 16 && pieceToMove.BoardIndex + currentRoll >= 16) 
				{
					pieceToMove.FlipPiece();
				}

				if (pieceToMove.BoardIndex != -1) 
				{
					urGC.enemyBoardPositions[pieceToMove.BoardIndex].Occupied = false;
					pieceToMove.BoardIndex += currentRoll;
				}
				else 
				{
					//If they're at at the beginning, we don't want to set their position to 4 if they rolled a 5
					pieceToMove.BoardIndex = 0;
				}

				nextTile.Occupied = true;

				if (nextTile.isRosette) 
				{
					urGC.ShowAlertText("Opponent Rolls Again");
					redoTurn = true;
				}
			}

			StartCoroutine(urGC.WaitToSwitchTurn(!redoTurn, midTurnPause));
		}
	}

	private UrPiece ChoosePieceToMove(List<UrPiece> movablePieceList) 
	{
		return movablePieceList.RandomElement();
	}

}
