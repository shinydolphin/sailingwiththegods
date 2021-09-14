using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UrPlayerPiece : UrPiece
{
	private bool selected = false;
	private int mask;

	private void Start() 
	{
		AssignVariables();
		mask = LayerMask.GetMask("GameSquare");
	}

	private void Update() 
	{
		if (selected) 
		{
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit, 300f, mask, QueryTriggerInteraction.Collide)) 
			{
				UrGameTile ugt = hit.collider.GetComponent<UrGameTile>();
				if (ugt != null && validMoves.Contains(ugt)) 
				{
					transform.position = ugt.transform.position;

					//We have to do all this so we don't accidentally try to send the piece to an earlier index, which would break a lot of stuff
					List<int> allPossibleIndices = new List<int>();
					for (int i = 0; i < urGC.playerBoardPositions.Count; i++) 
					{
						if (urGC.playerBoardPositions[i].Equals(ugt)) {
							allPossibleIndices.Add(i);
						}
					}

					//We're going to hard-code this as always only having 2 potential tiles, since that's the way the game is set up
					//Honestly, I probably should come up with a better way, but I can't think of it right now
					if (allPossibleIndices.Count > 1) 
					{
						if (boardIndex > allPossibleIndices[0]) 
						{
							potentialIndex = allPossibleIndices[1];
						}
						else 
						{
							potentialIndex = allPossibleIndices[0];
						}
					}
					else 
					{
						potentialIndex = allPossibleIndices[0];
					}
					
					ClearPossiblePath();
					ShowPossiblePath(urGC.playerBoardPositions, boardIndex, potentialIndex);
				}
				else 
				{
					//The only thing tagged GameSquare other than board squares is the plane that lets you drag off the board
					//We do this because it feels weird to mouse off your opponent's side of the board but have your piece snap back to your side
					//And of course, we check if the last position actually was off the board, otherwise you shouldn't be able to
					if (boardIndex == -1) 
					{
						transform.position = startPos;
						potentialIndex = -1;
					}
				}
			}
		}
	}

	private void OnMouseDown() 
	{
		if (urGC.AllowPlayerMove && enabled && highlight.activeSelf) 
		{
			selected = true;
			urGC.UnhighlightPieces();
			SpawnGhostInPlace();
			potentialIndex = boardIndex;
			validMoves = PopulateValidMovesList(urGC.playerBoardPositions, true);
			urGC.UnhighlightBoard();
		
			foreach (UrGameTile tile in validMoves) 
			{
				tile.ShowHighlight(true, true);
			}
		}
	}

	private void OnMouseUp() 
	{
		if (urGC.AllowPlayerMove && enabled) 
		{
			selected = false;
			DestroyGhost();
			urGC.UnhighlightBoard();
			ClearPossiblePath();

			//If you moved, check for flip or piece off, then pass the turn
			if (boardIndex != potentialIndex) 
			{
				urGC.PlayMoveSound();

				if (boardIndex != -1) {
					urGC.playerBoardPositions[boardIndex].ClearOccupied();
				}
				else {
					urGC.TriggerBark(true, urGC.MoveOnFlavor);
				}
				
				//The "bridge" you go back along starts at index 16 for both player and enemy, so I'm hard-coding it in
				//Again, this is a bad practice, but it shouldn't change so it's probably fine
				if (boardIndex < 16 && potentialIndex >= 16) 
				{
					FlipPiece();
					urGC.TriggerBark(true, urGC.FlipFlavor);
				}

				boardIndex = potentialIndex;
				if (urGC.playerBoardPositions[boardIndex].OppositeOccupyingPiece(true)) 
				{
					urGC.playerBoardPositions[boardIndex].RemoveCurrentFromBoard();
					urGC.TriggerBark(true, urGC.CaptureFlavor);
				}

				//We check this now because we won't need to do any more processing if you're moving off the board
				//We especially don't want to set the end space to occupied!
				if (boardIndex == urGC.playerBoardPositions.Count - 1) 
				{
					urGC.TriggerBark(true, urGC.MoveOffFlavor, true);
					urGC.PointScored(true, this);
				}
				else {
					urGC.playerBoardPositions[boardIndex].SetOccupied(this);
				}
				
				//If it's a rosette, "switch" to player turn
				if (urGC.playerBoardPositions[boardIndex].isRosette) 
				{
					urGC.ShowAlertText("Roll Again");
					urGC.TriggerBark(true, urGC.RosetteFlavor);
					urGC.SwitchTurn(true);
				}
				else 
				{
					StartCoroutine(urGC.WaitToSwitchTurn(false, .75f));
				}

				//No clue why I would need to check this, but I got an error once out of nowhere!
				if (validMoves != null) 
				{
					validMoves.Clear();
				}
			}
			//If you didn't move, the highlights need to come back
			else 
			{
				urGC.CanPlayerMove(true);
			}
		}
	}	
}
