//Paul Reichling
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PetteiaGameController : MonoBehaviour
{
	public Vector2Int rewardAmts;

	[Header("Game Pieces")]
	public List<PetteiaMovePiece> playerPieces;
	public PetteiaEnemyAI enemyAI;

	public AudioSource moveSound;

	[Header("Board Positions")]
	public PetteiaColliderMover[] squaresRow0 = new PetteiaColliderMover[8];
	public PetteiaColliderMover[] squaresRow1 = new PetteiaColliderMover[8];
	public PetteiaColliderMover[] squaresRow2 = new PetteiaColliderMover[8];
	public PetteiaColliderMover[] squaresRow3 = new PetteiaColliderMover[8];
	public PetteiaColliderMover[] squaresRow4 = new PetteiaColliderMover[8];
	public PetteiaColliderMover[] squaresRow5 = new PetteiaColliderMover[8];
	public PetteiaColliderMover[] squaresRow6 = new PetteiaColliderMover[8];
	public PetteiaColliderMover[] squaresRow7 = new PetteiaColliderMover[8];
	public int[,] positions = new int[8, 8];

	[Header("UI")]
	//public GameObject menuCanvas;
	//public GameObject endCanvas;
	//public Text waterText;
	//public Text foodText;
	public MiniGameInfoScreen mgScreen;
	public TavernaMiniGameDialog gameBarks;
	public Sprite gameIcon;
	public float barkChance = 0.25f;

	[Header("Text")]
	public string introText;
	[TextArea(2, 30)]
	public string instructions;
	public string winText;
	public string loseText;


	[TextArea(1, 8)]
	public string debugPiecePositions;


	private bool yourTurn;
	private int currentPiece;
	private string moveDir;
	private Vector2 oldPos, curPos;
	private Vector2 curPosArray, oldPosArray;
	private bool updateOld;
	private bool gameOver = false;

	//private Transform currentT;
	//public MovePiece mp;

	//Some variables are public for debugging and being able to be viewed in the inspector 
	// Start is called before the first frame update
	void Start() {
		//menuCanvas.SetActive(false);
		//endCanvas.SetActive(false);
		mgScreen.gameObject.SetActive(true);
		string text = introText + "\n\n" + instructions + "\n\n" + "flavor";
		mgScreen.DisplayText("Petteia", "Taverna game", text, gameIcon, MiniGameInfoScreen.MiniGame.TavernaStart);
		enemyAI = GetComponent<PetteiaEnemyAI>();
		moveDir = "";
		InitalStateSetup();
		updateOld = true;
		yourTurn = true;

		for (int i = 0; i < 8; i++) {
			BoardSquares[0, i] = squaresRow0[i];
			BoardSquares[1, i] = squaresRow1[i];
			BoardSquares[2, i] = squaresRow2[i];
			BoardSquares[3, i] = squaresRow3[i];
			BoardSquares[4, i] = squaresRow4[i];
			BoardSquares[5, i] = squaresRow5[i];
			BoardSquares[6, i] = squaresRow6[i];
			BoardSquares[7, i] = squaresRow7[i];
		}

		HighlightPlayerPieces(true);
		enemyAI.ToggleEnemyHighlight(false);
	}

	private void Update() {
		//if (Input.GetKeyDown(KeyCode.Alpha1)) {
		//	Debug.Log($"Winning with all 8, expect {rewardAmts.y}");
		//	enemyAI.pieces.Clear();
		//	CheckGameOver();
		//}
		//if (Input.GetKeyDown(KeyCode.Alpha2)) {
		//	Debug.Log($"Winning with half, expect {Mathf.CeilToInt((rewardAmts.y - rewardAmts.x) / 2f + rewardAmts.x)}");
		//	playerPieces.RemoveRange(0, 3);
		//	enemyAI.pieces.Clear();
		//	CheckGameOver();
		//}
		//if (Input.GetKeyDown(KeyCode.Alpha3)) {
		//	Debug.Log($"Winning with minimum 2, expect {rewardAmts.x}");
		//	playerPieces.RemoveRange(0, 6);
		//	enemyAI.pieces.Clear();
		//	CheckGameOver();
		//}
		//if (Input.GetKeyDown(KeyCode.Alpha4)) {
		//	playerPieces.Clear();
		//	CheckGameOver();
		//}
	}

	public void PauseMinigame() 
	{
		mgScreen.gameObject.SetActive(true);
		Time.timeScale = 0;
		mgScreen.DisplayText("Petteia", "Taverna game", instructions, gameIcon, MiniGameInfoScreen.MiniGame.TavernaPause);
	}

	public void UnpauseMinigame() {
		mgScreen.gameObject.SetActive(false);
		Time.timeScale = 1;
	}

	public void ExitMinigame() 
	{
		TavernaController.BackToTavernaMenu();
	}

	public void RestartMinigame() 
	{
		TavernaController.ReloadTavernaGame("Petteia");
	}

	//TODO: no more reloading, we'll have to just reset everything's position and state
	//so delete all pieces and respawn them in their base positions?
	//and if needed mark given squares as occupied
	//and show the ui again
	private IEnumerator ReloadMinigame() {
		Scene current = SceneManager.GetActiveScene();
		string name = current.name;
		yield return SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
		yield return SceneManager.UnloadSceneAsync(current);
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(name));
	}

	public void SwitchTurn() 
	{
		PrintBoard();
		//Debug.Log("Switching turn");
		if (yourTurn) {
			CheckCapture();
			yourTurn = false;
			CheckGameOver();
			HighlightPlayerPieces(false);
			enemyAI.ToggleEnemyHighlight(true);
			curPosArray = PosToArray((int)curPos.x, (int)curPos.y);
			oldPosArray = PosToArray((int)oldPos.x, (int)oldPos.y);
			updateOld = true;

			enemyAI.StartEnemyTurn();
		}
		else {
			CheckCapture();
			yourTurn = true;
			CheckGameOver();
			HighlightPlayerPieces(true);
			enemyAI.ToggleEnemyHighlight(false);
			oldPosArray = Vector2.up;
			curPosArray = Vector2.up;
			oldPos = Vector2.up;
			curPos = Vector2.up;
			//PetteiaMovePiece.showHighlight = true;
		}
	}

	public void HighlightPlayerPieces(bool toggle) 
	{
		foreach (PetteiaMovePiece p in playerPieces) {
			p.ToggleHighlight(toggle);
		}
	}

	public void PlayMoveSound() 
	{
		moveSound.pitch = Random.Range(0.7f, 1.1f);
		moveSound.Play();
	}

	void InitalStateSetup() {
		for (int i = 0; i < 8; i++) {

			positions[0, i] = 1;

			positions[7, i] = 2;

		}
	}

	string SetPiecePosition() {
		bool collide = false;
		//See if piece is off board - interviewer said my code was hard to read so I made it shorter
		if ((int)curPosArray.x > 7 || (int)curPosArray.y > 7 || (int)curPosArray.x < 0 || (int)curPosArray.y < 0) { collide = true; }


		//Checking each direction to see if the piece tries to move through another piece, which is not allowed
		if (collide == false) {
			if (moveDir == "down") {
				for (int i = (int)curPosArray.x; i > (int)oldPosArray.x; i--) {
					if (positions[i, (int)curPosArray.y] != 0) {
						collide = true;
					}
				}
			}
			if (moveDir == "up") {
				for (int i = (int)curPosArray.x; i < (int)oldPosArray.x; i++) {
					if (positions[i, (int)curPosArray.y] != 0) {
						collide = true;
					}
				}
			}

			if (moveDir == "right") {
				for (int i = (int)curPosArray.y; i > (int)oldPosArray.y; i--) {
					if (positions[(int)curPosArray.x, i] != 0) {
						collide = true;
					}
				}
			}
			if (moveDir == "left") {
				for (int i = (int)curPosArray.y; i < (int)oldPosArray.y; i++) {
					if (positions[(int)curPosArray.x, i] != 0) {
						collide = true;
					}
				}
			}
		}

		if (collide && !updateOld) {
			//MoveBack(currentT);

			//yourTurn = false;
			//Debug.Log("MOVEBACK TRIGGERED");
			moveSound.pitch = Random.Range(0.7f, 1.1f);
			moveSound.Play();
			return "mb";
		}
		else {
			try {
				//Transform inital, final;
				//send in original pos and new pos 
				positions[(int)curPosArray.x, (int)curPosArray.y] = currentPiece;
				positions[(int)oldPosArray.x, (int)oldPosArray.y] = 0;
			}
			catch {

			}
			if (oldPosArray != curPosArray) {
				return "m";
			} else {
				return "mb";
			}

		}
	}

	public void CheckCapture() {
		for (int y = 0; y < 8; y++) {

			for (int x = 0; x < 8; x++) {
				
				if (x != 0 && x != 7) {
					
					if (positions[x, y] == 1
						&& positions[x + 1, y] == 2
						&& positions[x - 1, y] == 2) 
					{
						Debug.Log("Enemy captured by player vertically");
						DoCapturePiece(x, y);
						//Debug.Log(x.ToString() + y.ToString());
					}
					
					
					if (positions[x, y] == 2
						&& positions[x + 1, y] == 1
						&& positions[x - 1, y] == 1) 
					{
						Debug.Log("Player captured by enemy vertically");
						DoCapturePiece(x, y);
						//Debug.Log(x.ToString() + y.ToString());
					}
					
				}
				if (y != 0 && y != 7) {
					
					if (positions[x, y] == 1
						&& positions[x, y + 1] == 2
						&& positions[x, y - 1] == 2) 
					{
						Debug.Log("Enemy captured by player horizontally");
						DoCapturePiece(x, y);
						//Debug.Log(x.ToString() + y.ToString());
					}
					
					
					if (positions[x, y] == 2
						&& positions[x, y + 1] == 1
						&& positions[x, y - 1] == 1) 
					{
						Debug.Log("Player captured by enemy horizontally");
						DoCapturePiece(x, y);
						//Debug.Log(x.ToString() + y.ToString());
					}
				}


			}
		}
		//CheckGameOver();
		#region old code
		//Older function with semantics errors

		//for (int y = 0; y < 8; y++) {

		//	for (int x = 1; x < 7; x++) {

		//		if (positions[0, x] == 1 && positions[0, x - 1] == 2 && positions[0, x + 1] == 2) {
		//			//Debug.Log("CAPTUREo");
		//			StartCoroutine(CapturePiece(x, 0));
		//			//Debug.Log(x.ToString() + y.ToString());
		//		}
		//		if (positions[0, x] == 2 && positions[0, x - 1] == 1 && positions[0, x + 1] == 1) {
		//			//Debug.Log("CAPTUREo");
		//			StartCoroutine(CapturePiece(x, 0));
		//			//Debug.Log(x.ToString() + y.ToString());
		//		}
		//		if (y != 0 && y != 7) {
		//			////Debug.Log(positions[i - 1, j] + "," + positions[j, i] + "," + positions[i + 1, j]);
		//			if (positions[x, y] == 1 && positions[x - 1, y] == 2 && positions[x + 1, y] == 2) {
		//				//Debug.Log("CAPTURE");
		//				StartCoroutine(CapturePiece(x, y));
		//				//Debug.Log(x.ToString() + y.ToString());
		//			}
		//			if (positions[x, y] == 2 && positions[x - 1, y] == 1 && positions[x + 1, y] == 1) {
		//				//Debug.Log("CAPTURE");
		//				StartCoroutine(CapturePiece(x, y));
		//				//Debug.Log(x.ToString() + y.ToString());

		//			}

		//		//if (j != 0 || j != 7) {
		//			if (positions[x, y] == 1 && positions[x, y - 1] == 2 && positions[x, y + 1] == 2) {
		//				//Debug.Log("CAPTURE");
		//				StartCoroutine(CapturePiece(x, y));
		//				//Debug.Log(x.ToString() + y.ToString());

		//			}
		//			if (positions[x, y] == 2 && positions[x, y - 1] == 1 && positions[x, y + 1] == 1) {
		//				//Debug.Log("CAPTURE");
		//				StartCoroutine(CapturePiece(x, y));
		//				//Debug.Log(x.ToString() + y.ToString());

		//			}
		//		} else {
		//			if (positions[x, y] == 1 && positions[x - 1, y] == 2 && positions[x + 1, y] == 2) {
		//				//Debug.Log("CAPTURE");
		//				StartCoroutine(CapturePiece(x, y));
		//				//Debug.Log(x.ToString() + y.ToString());
		//			}
		//			if (positions[x, y] == 2 && positions[x - 1, y] == 1 && positions[x + 1, y] == 1) {
		//				//Debug.Log("CAPTURE");
		//				StartCoroutine(CapturePiece(x, y));
		//				//Debug.Log(x.ToString() + y.ToString());

		//			}
		//		}
		//	}
		//}
		#endregion
	}

	public void CheckGameOver() {
		//Debug.Log($"Players: {playerPieces.Count} | Enemies: {enemyAI.pieces.Count}");
		if (enemyAI.pieces.Count <= 1) {
			mgScreen.gameObject.SetActive(true);

			//Minimum pieces left to win is 2, maximum is all 8
			//So we need to map [rewardAmt.x rewardAmt.y] to [2, 8]
			float oldRange = 6f;
			float newRange = rewardAmts.y - rewardAmts.x;			
			int reward = Mathf.CeilToInt(((playerPieces.Count - 2) * (newRange * 1.0f) / oldRange) + rewardAmts.x);

			string text = winText + "\n\n" + $"For your victory, you win {reward} food and water!";
			Globals.GameVars.playerShipVariables.ship.AddToFoodAndWater(reward);
			mgScreen.DisplayText("Petteia Victory", "Taverna Game", text, gameIcon, MiniGameInfoScreen.MiniGame.TavernaEnd);
			gameOver = true;
		}

		if (playerPieces.Count <= 1) {
			mgScreen.gameObject.SetActive(true);
			string text = loseText + "\n\n" + "Although you have lost this round, you can always find a willing opponent to try again!";
			mgScreen.DisplayText("Petteia Loss", "Taverna Game", text, gameIcon, MiniGameInfoScreen.MiniGame.TavernaEnd);
			gameOver = true;
		}
	}

	//private void CapturePiece(int i, int j) {
	//	StartCoroutine(DoCapturePiece(i, j));
	//}

	private void DoCapturePiece(int i, int j) 
	{
		BoardSquares[i, j].DestroyPiece();
		int enemyDone = 0;
		enemyDone = enemyAI.CheckPieces();
		int tries = 0;
		while (enemyDone != 1 && tries < 1000) {
			Debug.Log("wait...");
		}
		if (tries >= 200) {
			Debug.Log("Waited too long for the enemy to check its pieces");
		}
		//Debug.Log("Is it broken yet?");
		//Debug.Log("Checking if it's broken yet....");
		//Debug.Log("One last broken check...");
		//yield return null;
		//Debug.Log("After yield return null");

		//CheckGameOver();
		//colliders[i, j].GetComponent<PetteiaColliderMover>().destroy = true;
		//colliders[i, j].SetActive(true);
		//Collider needs time to check for collisions
		//yield return new WaitForSeconds(0.2f);
		//colliders[i, j].SetActive(false);

		if (Random.Range(0f, 1f) < barkChance) {
			//player captures enemy
			if (positions[i, j] == 1) {
				gameBarks.DisplayInsult();
			}
			//enemy captures player
			else if (positions[i, j] == 2) {
				gameBarks.DisplayBragging();
			}
		}

		positions[i, j] = 0;
		PrintBoard();
	}

	//IEnumerator GetPiece(int i, int j) {
	//	positions[i, j] = 0;
	//	colliders[i, j].GetComponent<colliderMover>().destroy = false;
	//	colliders[i, j].SetActive(true);
		

	//	yield return new WaitForSeconds(0.2f);
	//	g = colliders[i, j].GetComponent<colliderMover>().go.transform;
	//	colliders[i, j].SetActive(false);
	//	MovePiece(g, "down", gpos, g.tag);
	//	yourTurn = true;


	//}

	public void PrintBoard() {
		string s = "  ";
		for (int i = 0; i < 8; i++) {
			s += "\n\n";
			for (int j = 0; j < 8; j++) {
				s += "  ";
				switch (positions[i, j]) {
					case 0:
						s += "-";
						break;
					case 1:
						s += "X";
						break;
					case 2:
						s += "O";
						break;
				}
				//s += positions[i, j];
			}
		}

		//Debug.Log(s);
		debugPiecePositions = s;
	}

	public void MovePiece(Vector2Int oldPos, Vector2Int newPos, string tag) 
	{
		positions[oldPos.x, oldPos.y] = 0;
		positions[newPos.x, newPos.y] = tag == "PetteiaW" ? 2 : 1;
		currentPiece = tag == "PetteiaW" ? 2 : 1;
	}

	//public Vector3 MovePiece(Transform piece, string dir, Vector3 startPos, string tag) {

	//	//PrintBoard();

	//	moveDir = dir;
	//	if (updateOld == true) {
	//		currentT = piece;
	//		oldPos = new Vector2(startPos.z, startPos.x);

	//		updateOld = false;
	//	}
	//	if (dir == "up") {
	//		piece.position = startPos + Vector3.forward * 6.25f;
	//		//SetPiecePosition();
	//		curPos = new Vector2(piece.position.z, piece.position.x);

	//		if (tag == "PetteiaW") {
	//			currentPiece = 2;
	//		}
	//		else {
	//			currentPiece = 1;
	//		}
	//		return startPos + Vector3.forward * 6.25f;
	//	}
	//	if (dir == "down") {
	//		piece.position = startPos + Vector3.forward * -6.25f;
	//		//SetPiecePosition();
	//		curPos = new Vector2(piece.position.z, piece.position.x);

	//		if (tag == "PetteiaW") {
	//			currentPiece = 2;
	//		}
	//		else {
	//			currentPiece = 1;
	//		}
	//		return startPos + Vector3.forward * -6.25f;
	//	}
	//	if (dir == "left") {
	//		piece.position = startPos + Vector3.right * -6.25f;
	//		//SetPiecePosition();
	//		curPos = new Vector2(piece.position.z, piece.position.x);

	//		if (tag == "PetteiaW") {
	//			currentPiece = 2;
	//		}
	//		else {
	//			currentPiece = 1;
	//		}
	//		return startPos + Vector3.right * -6.25f;
	//	}
	//	if (dir == "right") {
	//		piece.position = startPos + Vector3.right * 6.25f;
	//		//SetPiecePosition();
	//		curPos = new Vector2(piece.position.z, piece.position.x);

	//		if (tag == "PetteiaW") {
	//			currentPiece = 2;
	//		}
	//		else {
	//			currentPiece = 1;
	//		}
	//		return startPos + Vector3.right * 6.25f;
	//	}

	//	else {
	//		//Debug.Log("SOMETHING HORRIBLE HAS HAPPENED \n Just kidding. Probably a typo(use right/left/up/down only");
	//		return startPos;

	//	}

	//}


	public Vector2 PosToArray(int y, int x) {
		return new Vector2(Mathf.Round(((y + 3.25f) / -6.25f) - 0), Mathf.Round(((x - 3) / 6.25f)) - 0);
		//converts the real world cordinates of the pieces to the value of the array that stores where the pieces are

	}

	void MoveBack(Transform g) {
		g.position = new Vector3(oldPos.y, 1, oldPos.x);
	}

    public PetteiaColliderMover[,] BoardSquares { get; } = new PetteiaColliderMover[8, 8];

	public bool GameOver 
	{
		get { return gameOver; }
	}

	public bool YourTurn {
		get { return yourTurn; }
	}
}
