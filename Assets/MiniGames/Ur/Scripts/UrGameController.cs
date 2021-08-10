//David Herrod
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UrGameController : MonoBehaviour
{
	public GameObject playerPathImage;
	public GameObject enemyPathImage;
	public UrAIController enemyAI;

	public List<UrGameTile> playerBoardPositions;
	public List<UrGameTile> enemyBoardPositions;
	public List<UrCounter> playerPieces;
	public List<UrCounter> enemyPieces;
	public UrDiceRoller dice;
	//public Text dvText;
	public Text alertText;
	public float alertShowTime;
	public float alertFadeSpeed;
	//private int diceValue = 0;
	//public int countersOffBoard = 7;
	//public int countersOnBoard = 0;
	//public int enemyCountersOnBoard;
	public Camera cam;
	//public bool selectingObject = false;

	//public bool selectingBoardPosition = false;
	//public UrCounter selectedCounter;
	public Button rollDiceButton;
	//public Animator aiAnim;
	//private Animator playerArms;
	//public int playerScore = 0;
	//public int enemyScore = 0;
	//public Text playerScoreText;
	//public Text enemyScoreText;

	private bool isGameOver = false;
	private int currentRoll;
	private bool isPlayerTurn = true;
	private bool allowPlayerMove = false;

	private Color baseAlertColor;
	private Outline alertOutline;
	private Color baseOutlineColor;
	private Coroutine fadeCoroutine;
	
	public void Awake() {
		//playerArms = dice.playerAnimator;
		baseAlertColor = alertText.color;
		alertOutline = alertText.GetComponent<Outline>();
		baseOutlineColor = alertOutline.effectColor;
		alertText.text = "";
	}

	public void Update() 
	{
		#region Old Code
		//if(Input.GetKeyDown("p")) {
		//	EnemyTurn();
		//}
		//if(!isPlaying(playerArms, "RollDiceLoop")) { 
		//	if (selectingObject) {
		//		if (Input.GetMouseButtonDown(0) && diceValue > 0) {
		//			foreach (UrCounter c in counters) {
		//				c.GetComponent<CapsuleCollider>().enabled = true;
		//			}
		//			Ray ray;
		//			RaycastHit hit;
		//			ray = cam.ScreenPointToRay(Input.mousePosition);

		//			if (Physics.Raycast(ray, out hit, 50)) {
		//				if (hit.collider.GetComponent<UrCounter>() != null && hit.collider.tag == "PlayerTile") {
		//					//if(hit.)
		//					CounterSelected(hit.collider.GetComponent<UrCounter>());
		//				}
		//			}
		//		}

		//	}
		//	//Does this need to be in update or can it be called less frequently?
		//	if (selectingBoardPosition) {
		//		Debug.Log("Selecting Board Position");
		//		if (Input.GetMouseButtonDown(0)) {
		//			//foreach (Counter c in counters) {
		//			//	c.GetComponent<CapsuleCollider>().enabled = false;
		//			//}
		//			Ray ray;
		//			RaycastHit hit;
		//			ray = cam.ScreenPointToRay(Input.mousePosition);

		//			if (Physics.Raycast(ray, out hit, 50)) {
		//				if (hit.collider.tag == "GameBoard") {
		//					if (!selectedCounter.onBoard) {
		//						countersOffBoard--;
		//						countersOnBoard++;
		//					}
		//					Debug.Log("Gameboard Hit");
		//					CounterSelected(selectedCounter);
		//					if (boardPositions.IndexOf(hit.collider.transform.parent.GetComponent<UrGameTile>()) == 19) { PointScored(true); selectedCounter.PlaceOnBoard(hit.collider.transform.parent.GetComponent<UrGameTile>(), true, false, true); /*selectedCounter.enabled = false;*/ }
		//					else {
		//						if (boardPositions.IndexOf(hit.collider.transform.parent.GetComponent<UrGameTile>()) >= 13) { selectedCounter.PlaceOnBoard(hit.collider.transform.parent.GetComponent<UrGameTile>(), true, false, false); if (IsEnemySpaceOccupied(hit.collider.transform.parent.GetComponent<UrGameTile>())){ IsEnemySpaceOccupiedCounter(hit.collider.transform.parent.GetComponent<UrGameTile>()).transform.position = IsEnemySpaceOccupiedCounter(hit.collider.transform.parent.GetComponent<UrGameTile>()).initPosit; IsEnemySpaceOccupiedCounter(hit.collider.transform.parent.GetComponent<UrGameTile>()).currentTile = null; enemyCountersOnBoard--; } }
		//						else { selectedCounter.PlaceOnBoard(hit.collider.transform.parent.GetComponent<UrGameTile>(), false, false, false); }
		//					}
		//					selectingBoardPosition = false;
		//					diceValue = 0;
		//					//EnemyTurn();

		//				}
		//			}
		//		}
		//		//if(Input.GetMouseButtonDown(1)) {
		//		//	selectingBoardPosition = false;
		//		//	selectingObject = true;
		//		//	CounterSelected(selectedCounter);
		//		}
		//	}
		#endregion
	}

	public void UnhighlightBoard(UrGameTile ugt) 
	{
		foreach (UrGameTile tile in playerBoardPositions) {
			if (!tile.Equals(ugt)) {
				tile.ShowHighlight(false);
			}
		}
		foreach (UrGameTile tile in enemyBoardPositions) {
			if (!tile.Equals(ugt)) {
				tile.ShowHighlight(false);
			}
		}
	}

	public void UnhighlightBoard() 
	{
		foreach (UrGameTile tile in playerBoardPositions) 
		{
			tile.ShowHighlight(false);
		}

		foreach (UrGameTile tile in enemyBoardPositions) 
		{
			tile.ShowHighlight(false);
		}
	}

	public void UnhighlightPlayerPieces() 
	{
		foreach (UrCounter piece in playerPieces) 
		{
			piece.ShowHighlight(false);
		}
	}

	public int GetDiceRoll() {
		rollDiceButton.interactable = false;
		currentRoll = dice.RollDice(isPlayerTurn);
		if (isPlayerTurn) {
			StartCoroutine(WaitToSwitchTurn(false, 1.75f));
		}
		return currentRoll;
	}

	public void RollDice() {
		rollDiceButton.interactable = false;
		currentRoll = dice.RollDice(isPlayerTurn);
		if (isPlayerTurn) {
			allowPlayerMove = true;
		}
		rollDiceButton.interactable = false;
	}

	public void SwitchTurn(bool playerTurn) 
	{
		isPlayerTurn = playerTurn;
		allowPlayerMove = false;
		rollDiceButton.interactable = isPlayerTurn;
		playerPathImage.SetActive(isPlayerTurn);
		enemyPathImage.SetActive(!isPlayerTurn);
		UnhighlightBoard();
		if (!isPlayerTurn) {
			enemyAI.EnemyTurn();
		}
	}

	public IEnumerator WaitToSwitchTurn(bool playerTurn, float waitTime) 
	{
		yield return new WaitForSeconds(waitTime);
		SwitchTurn(playerTurn);
	}

	//public void CounterSelected(UrCounter c) {
	//	if (c.onBoard) {
	//		selectedCounter = c;
	//		int bIndex = playerBoardPositions.IndexOf(c.currentTile);
	//		//for (int i = 0; i < diceValue; i++) {
	//		//need to figure this out - what if bIndex + diceValue is out of range?
	//		if (bIndex + diceValue < playerBoardPositions.Count) {
	//			if (!IsSpaceOccupied(playerBoardPositions[bIndex + diceValue])) 
	//			{
	//				playerBoardPositions[bIndex + diceValue].ShowAvailable(true);
	//				selectingBoardPosition = !selectingBoardPosition;
	//			}
	//			else 
	//			{
	//				Debug.Log("This tile cannot move.");
	//			}
	//		}
	//		else 
	//		{
	//			Debug.Log("This tile cannot move.");
	//		}

	//		//bIndex++;
	//		//}

	//		//selectingObject = !selectingObject;
	//	}
	//	else {
	//		selectedCounter = c;
	//		if (diceValue == 1 && !IsSpaceOccupied(playerBoardPositions[0])) {
	//			playerBoardPositions[0].ShowAvailable(true);
	//			selectingBoardPosition = !selectingBoardPosition;
	//		}
	//		else if(diceValue == 5 && !IsSpaceOccupied(playerBoardPositions[4]) && !IsSpaceOccupied(playerBoardPositions[16])) { playerBoardPositions[4].ShowAvailable(true); selectingBoardPosition = !selectingBoardPosition; }
	//	}
	 
	//}

	//public bool IsSpaceOccupied(UrGameTile gt) 
	//{
	//	foreach(UrCounter c in playerPieces) 
	//	{
	//		if(playerBoardPositions.IndexOf(c.currentTile) == playerBoardPositions.IndexOf(gt)) 
	//		{
	//			Debug.Log("space is occupied");
	//			return true;
	//		}
	//	}
	//	Debug.Log("space is free");
	//	return false;
	//}

	//public UrCounter IsSpaceOccupiedCounter(UrGameTile gt) 
	//{
	//	foreach (UrCounter c in playerPieces) 
	//	{
	//		if (playerBoardPositions.IndexOf(c.currentTile) == playerBoardPositions.IndexOf(gt)) 
	//		{
	//			Debug.Log("space is occupied");
	//			return c;
	//		}
	//	}
	//	Debug.Log("space is free");
	//	return null;
	//}

	//public bool IsEnemySpaceOccupied(UrGameTile gt) {
	//	foreach (UrCounter c in enemyPieces) {
	//		if (enemyBoardPositions.IndexOf(c.currentTile) == enemyBoardPositions.IndexOf(gt)) { Debug.Log("space is occupied"); return true; }
	//	}
	//	Debug.Log("space is free");
	//	return false;
	//}

	//public UrCounter IsEnemySpaceOccupiedCounter(UrGameTile gt) {
	//	foreach (UrCounter c in enemyPieces) {
	//		if (enemyBoardPositions.IndexOf(c.currentTile) == enemyBoardPositions.IndexOf(gt)) { Debug.Log("space is occupied"); return c; }
	//	}
	//	Debug.Log("space is free");
	//	return null;
	//}

	public bool CanPlayerMove() 
	{
		int movable = 0;
		foreach(UrCounter c in playerPieces) 
		{
			if (c.PopulateValidMovesList().Count > 0) 
			{
				c.ShowHighlight(true);
				movable++;
			}
			//if(c.currentTile != null && (playerBoardPositions.IndexOf(c.currentTile)+ val) < 19) { return true; }
		}
		return movable > 0;
	}

	//public bool CanEnemyMove(int val, UrCounter c) {

	//	if (c.currentTile != null && (enemyBoardPositions.IndexOf(c.currentTile) + val) < 19) { return true; }
	//	else {
	//		return false;
	//	}
	//}

	public void SetDiceValue(int val) {
		////diceValue = dice.DiceResult(val);
		////rollDiceButton.SetActive(false);
		//Debug.Log(diceValue);
		//dvText.text = "" + diceValue;
		//if(diceValue == 0) { EnemyTurn(); }
		//if (diceValue == 4 && !CanPlayerMove()) { EnemyTurn(); }
		//if (countersOnBoard > 0) {
		//	selectingObject = true;
		//}
		//else {


		//	if (diceValue == 1 && countersOffBoard > 0 && countersOnBoard == 0) {
		//		playerPieces[countersOffBoard - 1].PlaceOnBoard(playerBoardPositions[0], false, false, false);
		//		countersOffBoard--;
		//		countersOnBoard++;
		//		//aiAnim.SetTrigger("Angry");
		//		//EnemyTurn();
		//	}
		//	else if (diceValue == 5 && countersOffBoard > 0 && countersOnBoard == 0) {
		//		playerPieces[countersOffBoard - 1].PlaceOnBoard(playerBoardPositions[4], false, false, false);
		//		countersOffBoard--;
		//		countersOnBoard++;
		//		//aiAnim.SetTrigger("Angry");
		//		//EnemyTurn();
		//	}
		//	else {
		//		EnemyTurn();
		//	}
		//}
		
	}

	public void ShowAlertText(string alert) 
	{
		StartCoroutine(DoShowAlertText(alertText, alertOutline, alert));
	}

	private IEnumerator DoShowAlertText(Text t, Outline o, string alert) 
	{
		//For some reason, just calling StopCoroutine(FadeText(t, o)) doesn't work, so we have to do it this way
		if (fadeCoroutine != null) 
		{
			StopCoroutine(fadeCoroutine);
			fadeCoroutine = null;
		}
		yield return null;
		t.color = baseAlertColor;
		o.effectColor = baseOutlineColor;
		alertText.text = alert;
		yield return null;
		fadeCoroutine = StartCoroutine(FadeText(t, o));
	}

	private IEnumerator FadeText(Text t, Outline o) 
	{
		yield return new WaitForSeconds(alertShowTime);
		Color clearColor = new Color(baseAlertColor.r, baseAlertColor.g, baseAlertColor.b, 0f);
		Color clearOutline = new Color(baseOutlineColor.r, baseOutlineColor.g, baseOutlineColor.b, 0f);

		for (float i = 0; i <= 1; i += Time.deltaTime * alertFadeSpeed) 
		{
			t.color = Color.Lerp(baseAlertColor, clearColor, i);
			o.effectColor = Color.Lerp(baseOutlineColor, clearOutline, i);
			yield return null;
		}

		alertText.text = "";
		alertText.color = baseAlertColor;
		o.effectColor = baseOutlineColor;
	}

	bool isPlaying(Animator anim, string stateName) {
		if (anim.GetCurrentAnimatorStateInfo(0).IsName(stateName))
			return true;
		else
			return false;
	}

	public void PointScored(bool player, UrCounter c) 
	{
		if (player) 
		{
			playerPieces.Remove(c);
			c.GetComponent<MeshRenderer>().enabled = false;
			Destroy(c.gameObject, 1f);
			Debug.Log("Player scored with a piece!");
			if (playerPieces.Count == 0) 
			{
				WinGame();
			}
		}
		else 
		{
			enemyPieces.Remove(c);
			Destroy(c.gameObject);
			Debug.Log("Enemy scored with a piece!");
			if (enemyPieces.Count == 0) 
			{
				LoseGame();
			}
		}
		//if(playerScore == 7) {
		//	Debug.Log("You win.");
		//}
		//if (enemyScore == 7) {
		//	Debug.Log("You lose.");
		//}
	}

	public void WinGame() 
	{
		isGameOver = true;
		rollDiceButton.interactable = false;
		allowPlayerMove = false;
		Debug.Log("Player wins!");
	}

	public void LoseGame() 
	{
		isGameOver = false;
		rollDiceButton.interactable = false;
		allowPlayerMove = false;
		Debug.Log("Player loses :(");
	}

	public void EnemyTurn() {
	//	int[] i = { 0, 1, 4, 5 };
	//	int sel = Random.Range(0, 4);
	//	int ediceValue = i[sel];
	//	dvText.text = "" + ediceValue;
	//	if(ediceValue == 1 && enemyCountersOnBoard == 0) {
	//		eCounters[0].PlaceOnBoard(enemyBoardPositions[0], false, true, false); enemyCountersOnBoard++;
	//	}
	//	else if(ediceValue == 5 && enemyCountersOnBoard == 0) {
	//			eCounters[0].PlaceOnBoard(enemyBoardPositions[4], false, true, false); enemyCountersOnBoard++;
	//		}
	//	else if (ediceValue == 1 && enemyCountersOnBoard > 0 && !IsEnemySpaceOccupied(enemyBoardPositions[0])) {
	//		eCounters[enemyCountersOnBoard].PlaceOnBoard(enemyBoardPositions[0], false, true, false); enemyCountersOnBoard++;
	//	}
	//	else if (ediceValue == 5 && enemyCountersOnBoard > 0 && !IsEnemySpaceOccupied(enemyBoardPositions[4])) {
	//		eCounters[enemyCountersOnBoard].PlaceOnBoard(enemyBoardPositions[4], false, true, false); enemyCountersOnBoard++;
	//	}
	//	else if(enemyCountersOnBoard > 0) {
	//		if(ediceValue == 1) {
	//			foreach(UrCounter c in eCounters) {
	//				if (c.onBoard && CanEnemyMove(ediceValue, c)) {
	//					Debug.Log("i get here");
	//					int index = enemyBoardPositions.IndexOf(c.currentTile);
	//					if (!IsEnemySpaceOccupied(enemyBoardPositions[index + 1]) && ((index + 1) < 13)) {
	//						c.PlaceOnBoard(enemyBoardPositions[index + 1], false, true, false);
	//						if (IsSpaceOccupied(playerBoardPositions[index + 1])) {
	//							Debug.Log("haha rekt"); IsSpaceOccupiedCounter(playerBoardPositions[index + 1]).onBoard = false; IsSpaceOccupiedCounter(playerBoardPositions[index + 1]).transform.position = IsSpaceOccupiedCounter(playerBoardPositions[index + 1]).initPosit; IsSpaceOccupiedCounter(playerBoardPositions[index + 1]).currentTile = null; break;
	//						}
	//					}
	//					else if (!IsEnemySpaceOccupied(enemyBoardPositions[index + 1]) && ((index + 1) >= 13)) {
	//						if(index + 1 == 19) { PointScored(false); }
	//						c.PlaceOnBoard(enemyBoardPositions[index + 1], true, true, false);
	//						if (IsSpaceOccupied(playerBoardPositions[index + 1])) {
	//							Debug.Log("haha rekt"); IsSpaceOccupiedCounter(playerBoardPositions[index + 1]).onBoard = false; IsSpaceOccupiedCounter(playerBoardPositions[index + 1]).transform.position = IsSpaceOccupiedCounter(playerBoardPositions[index + 1]).initPosit; IsSpaceOccupiedCounter(playerBoardPositions[index + 1]).currentTile = null; break;
	//						}

	//					}
						
	//				}
					
	//			}
	//		}
	//		else if (ediceValue == 4) {
	//			foreach (UrCounter c in eCounters) {
	//				if (c.onBoard && CanEnemyMove(ediceValue, c)) {
	//					Debug.Log("i get here");
	//					int index = enemyBoardPositions.IndexOf(c.currentTile);
	//					if (!IsEnemySpaceOccupied(enemyBoardPositions[index + 4]) && ((index + 4) < 13)) {
	//						c.PlaceOnBoard(enemyBoardPositions[index + 4], false, true, false);
	//						if (IsSpaceOccupied(playerBoardPositions[index + 4])) {
	//							Debug.Log("haha rekt"); IsSpaceOccupiedCounter(playerBoardPositions[index + 4]).onBoard = false; IsSpaceOccupiedCounter(playerBoardPositions[index + 4]).transform.position = IsSpaceOccupiedCounter(playerBoardPositions[index + 4]).initPosit; IsSpaceOccupiedCounter(playerBoardPositions[index + 4]).currentTile = null; break;
	//						}
	//					}
	//					else if (!IsEnemySpaceOccupied(enemyBoardPositions[index + 4]) && ((index + 4) >= 13)) {
	//						if (index + 4 == 19) { PointScored(false); }
	//						c.PlaceOnBoard(enemyBoardPositions[index + 4], true, true, false);
	//						if (IsSpaceOccupied(playerBoardPositions[index + 4])) {
	//							Debug.Log("haha rekt"); IsSpaceOccupiedCounter(playerBoardPositions[index + 4]).onBoard = false; IsSpaceOccupiedCounter(playerBoardPositions[index + 4]).transform.position = IsSpaceOccupiedCounter(playerBoardPositions[index + 4]).initPosit; IsSpaceOccupiedCounter(playerBoardPositions[index + 4]).currentTile = null; break;
	//						}

	//					}

	//				}
	//			}
	//		}
	//		else if (ediceValue == 5) {
	//			foreach (UrCounter c in eCounters) {
	//				if (c.onBoard && CanEnemyMove(ediceValue, c)) {
	//					Debug.Log("i get here");
	//					int index = enemyBoardPositions.IndexOf(c.currentTile);
	//					if (!IsEnemySpaceOccupied(enemyBoardPositions[index + 5]) && ((index + 5) < 13)) {
	//						c.PlaceOnBoard(enemyBoardPositions[index + 5], false, true, false);
	//						if (IsSpaceOccupied(playerBoardPositions[index + 5])) {
	//							Debug.Log("haha rekt"); IsSpaceOccupiedCounter(playerBoardPositions[index + 5]).onBoard = false; IsSpaceOccupiedCounter(playerBoardPositions[index + 5]).transform.position = IsSpaceOccupiedCounter(playerBoardPositions[index + 5]).initPosit; IsSpaceOccupiedCounter(playerBoardPositions[index + 5]).currentTile = null; break;
	//						}
	//					}
	//					else if (!IsEnemySpaceOccupied(enemyBoardPositions[index + 5]) && ((index + 5) >= 13)) {
	//						if (index + 5 == 19) { PointScored(false); }
	//						c.PlaceOnBoard(enemyBoardPositions[index + 5], true, true, false);
	//						if (IsSpaceOccupied(playerBoardPositions[index + 5])) {
	//							Debug.Log("haha rekt"); IsSpaceOccupiedCounter(playerBoardPositions[index + 5]).onBoard = false; IsSpaceOccupiedCounter(playerBoardPositions[index + 5]).transform.position = IsSpaceOccupiedCounter(playerBoardPositions[index + 5]).initPosit; IsSpaceOccupiedCounter(playerBoardPositions[index + 5]).currentTile = null; break;
	//						}

	//					}

	//				}
	//			}
	//		}
	//	}
	//	//rollDiceButton.SetActive(true);
	}

	public int CurrentRoll {
		get {
			return currentRoll;
		}
	}

	public bool IsPlayerTurn {
		get {
			return isPlayerTurn;
		}
	}

	public bool AllowPlayerMove {
		get {
			return allowPlayerMove;
		}
	}

	public bool IsGameOver {
		get {
			return isGameOver;
		}
	}
}
