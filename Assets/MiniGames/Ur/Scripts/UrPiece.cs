using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UrPiece : MonoBehaviour
{
	public GameObject highlight;
	public GameObject ghostPiece;
	public LineRenderer possiblePathLine;

	protected Vector3 startPos;

	protected bool selected = false;
	protected int mask;
	protected UrGameController urGC;
	protected GameObject spawnedGhost;

	protected List<UrGameTile> validMoves;
	protected int boardIndex = -1;
	protected int potentialIndex;
	protected Animator anim;

	private void Start() 
	{
		AssignVariables();
	}


	protected void AssignVariables() 
	{
		anim = GetComponent<Animator>();
		urGC = GameObject.FindWithTag("GameController").GetComponent<UrGameController>();
		startPos = transform.position;
		mask = LayerMask.GetMask("GameSquare");
		highlight.SetActive(false);
	}

	public List<UrGameTile> PopulateValidMovesList(List<UrGameTile> allowedPath) 
	{
		int roll = urGC.CurrentRoll;
		List<UrGameTile> possibleMoves = new List<UrGameTile>();

		//If this is off the board, it can only move if you have a 1 or 5 and then just to the start
		if (boardIndex == -1) 
		{
			if ((roll == 1 || roll == 5) && !allowedPath[0].Occupied) 
			{
				possibleMoves.Add(allowedPath[0]);
			}
		}
		//If this is on the board, just add the number
		//Note: We also add the original space so we can move back to it if we change our mind (which we could do anyway, but this way it lights up)
		else if (roll > 0) 
		{
			int nextSpace = boardIndex + roll;
			//If it's not off the end of the board, you're fine and normal
			if (nextSpace < allowedPath.Count - 1) 
			{
				if (!allowedPath[nextSpace].Occupied) 
				{
					possibleMoves.Add(allowedPath[boardIndex]);
					possibleMoves.Add(allowedPath[boardIndex + roll]);
				}
			}
			//If it's exactly to the end, you do special stuff
			//Don't need to check for occupied here, since this is off the board
			else if (boardIndex + roll == allowedPath.Count - 1) 
			{
				possibleMoves.Add(allowedPath[boardIndex]);
				possibleMoves.Add(allowedPath[boardIndex + roll]);
			}
			//Otherwise if it's off the end you can't move it
		}
		
		return possibleMoves;
	}

	public void ShowPossiblePath(List<UrGameTile> allowedPath, int start, int end) 
	{
		if (start != -1) {
			//We need to add 1 so we include both ending squares
			int length = end - start + 1;
			Vector3[] positions = new Vector3[length];

			for (int i = 0; i < positions.Length; i++) 
			{
				positions[i] = allowedPath[i + start].transform.position;
			}

			possiblePathLine.positionCount = length;
			possiblePathLine.SetPositions(positions);

			UrScrollTex scroll = possiblePathLine.GetComponent<UrScrollTex>();

			if (urGC.IsPlayerTurn) 
			{
				scroll.xSpeed = Mathf.Abs(scroll.xSpeed) * -1;
			}
			else 
			{
				scroll.xSpeed = Mathf.Abs(scroll.xSpeed);
			}

		}

	}

	public void ClearPossiblePath() 
	{
		possiblePathLine.positionCount = 0;
	}
	
	public void FlipPiece() 
	{
		anim.SetTrigger("Flip");
	}

	public void RemovePieceFromBoard() 
	{
		transform.position = startPos;
		boardIndex = -1;
		transform.rotation = Quaternion.identity;
	}

	public void ShowHighlight(bool toggle) 
	{
		highlight.SetActive(toggle);
	}

	public void SpawnGhostInPlace() 
	{
		spawnedGhost = Instantiate(ghostPiece, transform.position, transform.rotation);
	}

	public void DestroyGhost() 
	{
		Destroy(spawnedGhost);
		spawnedGhost = null;
	}

	public int BoardIndex {
		get {
			return boardIndex;
		}
		set {
			boardIndex = value;
		}
	}

}
