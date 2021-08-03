using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UrCounter : MonoBehaviour
{
	public bool goingHome = false;
	public bool enemyTile = false;
	public UrGameTile currentTile;
	public GameObject ikTarget;
	public float increment;
	public bool onTheMove = false;
	public UrArmIKHandler armIK;
	public UrGameController UR;
	public bool pointScored = false;
	public GameObject highlight;
	public GameObject ghostPiece;

	private Vector3 startPos;
	private bool onBoard = false;

	private bool selected = false;
	private int mask;
	private UrGameController ugc;
	private GameObject spawnedGhost;
	
	private List<UrGameTile> validMoves;
	public int boardIndex = -1;
	private int potentialIndex;

	private void Start() {
		ugc = GameObject.FindWithTag("GameController").GetComponent<UrGameController>();
		startPos = transform.position;
		mask = LayerMask.GetMask("GameSquare");
		highlight.SetActive(false);
	}

	private void Update() 
	{
		if (selected) 
		{
			RaycastHit hit;
			Ray ray = ugc.cam.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit, 300f, mask, QueryTriggerInteraction.Collide)) 
			{
				UrGameTile ugt = hit.collider.GetComponent<UrGameTile>();
				if (ugt != null && validMoves.Contains(ugt)) 
				{
					transform.position = ugt.transform.position;
					potentialIndex = ugc.playerBoardPositions.IndexOf(ugt);
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


		//if (onTheMove && GetComponent<MeshRenderer>().enabled == false) {
		//	if ((Vector3.Distance(ikTarget.transform.position, currentTile.transform.position) > 0.001f))
		//		ikTarget.transform.position = Vector3.MoveTowards(ikTarget.transform.position, currentTile.transform.position, (increment * Time.deltaTime));
		//	else {
		//		armIK.gameObject.GetComponent<Animator>().SetTrigger("MovementIsDone");
		//		transform.position = currentTile.transform.position;
		//		//GetComponent<MeshRenderer>().enabled = true; transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
		//		onBoard = true;
		//		onTheMove = false;
		//		if(!enemyTile) {
		//			StartCoroutine(EnemyTurnDelay());
		//		}
		//		//GetComponent<MeshRenderer>().enabled = true; transform.GetChild(0).GetComponent<MeshRenderer>().enabled = true;
		//	}
		//}
	}

	public void PlaceOnBoard(UrGameTile tile, bool flip, bool enemyT, bool ps) {
		//GetComponent<MeshRenderer>().enabled = false;
		//transform.GetChild(0).GetComponent<MeshRenderer>().enabled = false;
		pointScored = ps;
		goingHome = flip;
		enemyTile = enemyT;
		armIK.counterOnTheMove = this;
		currentTile = tile;
		onTheMove = true;
		armIK.gameObject.GetComponent<Animator>().SetTrigger("BeginMovement");
		
		ikTarget.transform.position = transform.position;
		
	}

	public void TileMT() {
		GetComponent<MeshRenderer>().enabled = !GetComponent<MeshRenderer>().enabled;
		transform.GetChild(0).GetComponent<MeshRenderer>().enabled = !transform.GetChild(0).GetComponent<MeshRenderer>().enabled;
		if(goingHome) {
			transform.rotation = Quaternion.Euler(0, 0, 0);
		}
	}

	IEnumerator EnemyTurnDelay() {
		yield return new WaitForSeconds(2.0f);
		if(pointScored) {
			gameObject.SetActive(false);
		}
		UR.EnemyTurn();
	}

	private void OnMouseDown() 
	{
		if (ugc.AllowPlayerMove && enabled && highlight.activeSelf) 
		{
			selected = true;
			ugc.UnhighlightPlayerPieces();
			spawnedGhost = Instantiate(ghostPiece, transform.position, transform.rotation);
			potentialIndex = boardIndex;
			validMoves = PopulateValidMovesList();
			ugc.UnhighlightBoard();
			foreach (UrGameTile tile in validMoves) 
			{
				tile.ShowHighlight(true);
			}
		}
	}

	private void OnMouseUp() 
	{
		if (ugc.AllowPlayerMove && enabled) 
		{
			selected = false;
			Destroy(spawnedGhost);
			ugc.UnhighlightBoard();

			//If you moved, pass the turn
			if (spawnedGhost != null && boardIndex != potentialIndex) 
			{
				if (boardIndex != -1) {
					ugc.playerBoardPositions[boardIndex].Occupied = false;
				}
				boardIndex = potentialIndex;
				ugc.playerBoardPositions[boardIndex].Occupied = true;

				//If it's a rosette, "switch" to player turn
				if (ugc.playerBoardPositions[boardIndex].isRosette) {
					ugc.SwitchTurn(true);
				}
				else {
					StartCoroutine(ugc.WaitToSwitchTurn(false, 1.5f));
				}
				validMoves.Clear();
			}
			//If you didn't move, the highlights need to come back
			else 
			{
				ugc.CanPlayerMove();
			}
		}
	}

	public List<UrGameTile> PopulateValidMovesList() 
	{
		int roll = ugc.CurrentRoll;
		List<UrGameTile> possibleMoves = new List<UrGameTile>();

		//If this is off the board, it can only move if you have a 1 or 5 and then just to the start
		if (boardIndex == -1) 
		{
			if ((roll == 1 || roll == 5) && !ugc.playerBoardPositions[0].Occupied) {
				possibleMoves.Add(ugc.playerBoardPositions[0]);
			}
		}
		//If this is on the board, just add the number
		else if(roll > 0)
		{
			int nextSpace = boardIndex + roll;
			//If it's not off the end of the board, you're fine and normal
			if (nextSpace < ugc.playerBoardPositions.Count - 1) 
			{
				if (!ugc.playerBoardPositions[nextSpace].Occupied) 
				{
					possibleMoves.Add(ugc.playerBoardPositions[boardIndex]);
					possibleMoves.Add(ugc.playerBoardPositions[boardIndex + roll]);
				}
			}
			//If it's exactly to the end, you do special stuff
			//Don't need to check for occupied here, since this is off the board
			else if (boardIndex + roll == ugc.playerBoardPositions.Count - 1) 
			{
				possibleMoves.Add(ugc.playerBoardPositions[boardIndex]);
				possibleMoves.Add(ugc.playerBoardPositions[boardIndex + roll]);
				Debug.Log("Piece off the end!");
			}
			//Otherwise if it's off the end you can't move it
		}
		
		return possibleMoves;
	}

	public void FlipPiece() {
		transform.Rotate(Vector3.left * -180f);
	}

	public void RemovePieceFromBoard() 
	{
		transform.position = startPos;
		boardIndex = -1;
		transform.rotation = Quaternion.identity;
	}

	public void ShowHighlight(bool toggle) {
		highlight.SetActive(toggle);
	}
}
