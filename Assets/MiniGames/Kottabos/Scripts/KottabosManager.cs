using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
//ISSUE: PowerIndiacator when activated stops the Menu buttons from being pressed
public class KottabosManager : MonoBehaviour {
	GameSession Session => Globals.Game.Session;

	public MiniGameInfoScreen mgscreen;
	public Sprite gameIcon = null;

	public GameObject playerPos;
	private Rigidbody playerRb;
	private Vector3 playerStartPos;

	//Used to reset the targets on kottabos stand
	public GameObject randomPlacement;
	public Transform[] childPos;

	//Used to reset the top target on kottabos stand because of rigidbody attachment
	private Vector3 topTargetStartPos;
	private Quaternion topTargetStartRot;
	private KottabosThrow tr;

	private static int score = 0;
	[SerializeField]
	private static int tries = 6;

	public bool ContinueRound { get; set; } = false;
	public bool Scored { get; set; } = false;
	public bool IsHit { get; set; } = false;

	/// <summary>
	/// Reset Kotaboas varibles after a game is played
	/// </summary>
	private void KottabosReset() {
		score = 0;
		tries = 5;

		ContinueRound = false;
		Scored = false;
		IsHit = false;

		Time.timeScale = 1;
	}

	// Start is called before the first frame update
	void Start() {		

		mgscreen.DisplayText("Kottabos", "Wine throwing game", "Try and hit targets with the fig droplet. Power, controlled by the E and Q Keys, will determine how far your figs will go. The pointer between the axis will determine the direction of the fig, controlled by the ASWD Keys, moving it in your desired direction. You will be given 5 tries to estimate the trajectory to hit the targets. If you succeed in hitting a target in the bowl or on the Kottabos Stand, you will be allowed to play another round with the C key. The top target on the Kottabos stand will give the most points. Targets will be randomly placed on each hit.", gameIcon, MiniGameInfoScreen.MiniGame.TavernaStart);

		playerStartPos = playerPos.transform.position;
		playerRb = playerPos.GetComponent<Rigidbody>();

		topTargetStartPos = childPos[5].position;
		topTargetStartRot = childPos[5].rotation;

		tr = playerPos.GetComponent<KottabosThrow>();
	}

	// Update is called once per frame
	void Update() {

		KottabosPauseAndUnPause();

		if (ContinueRound) {
			Debug.Log("C or B");
			Debug.Log(score);
			//Debug.Log(tries);
			if (Input.GetKeyDown(KeyCode.C)) {
				tr.animate.SetBool("isFlinged", false);
				gameObject.GetComponent<KottabosArmController>().ArmReset();
				playerPos.SetActive(true);
				//Reset
				ResetRound();
				//Debug.Log("reset");
				ContinueRound = false;
			}
			else if (Input.GetKeyDown(KeyCode.B) || tries == 0) {
				//Start = 15;
				//tries = 0;
				mgscreen.gameObject.SetActive(true);

				if (score >= 15) {
					//Here's your reward end game
					//KottabosReset();
					Session.AdjustPlayerClout(15 * score, false);
					Globals.Game.Session.playerShipVariables.ship.currency += Random.Range(5, 7);
					mgscreen.DisplayText("Perfect", "Perfection absolute – desired but dangerous!", "You have reached it, but now beware\n Lest Envy drive the god of War\n To take aim at you as you have at these cups!", null, MiniGameInfoScreen.MiniGame.TavernaEnd);
				}
				else if (score >= 10) {
					//KottabosReset();
					Session.AdjustPlayerClout(Random.Range(10, 14) * score, false);
					Globals.Game.Session.playerShipVariables.ship.currency += Random.Range(3,4);
					mgscreen.DisplayText("Great", "Zeus himself could not have thrown better!", "Your hand was neither too stiff , nor too crooked; a master of the javelin, a god of the sling, a hero of missiles must you be on  the battlefield!", null, MiniGameInfoScreen.MiniGame.TavernaEnd);
				}
				else if (score >= 5) {
					//KottabosReset();
					Session.AdjustPlayerClout(Random.Range(5, 9) * score, false);
					mgscreen.DisplayText("Good", "A winner in this game is a winner in love!", "A Sophokles says, The golden-colored drop of Aphrodite descends on all the houses! (Athenaeus Deipnosophistae 668)", null, MiniGameInfoScreen.MiniGame.TavernaEnd);
				}
				else 
				{
					//KottabosReset();
					mgscreen.DisplayText("You Lose", "You have lost!", "Your clout is like a tiny mouse who must hide from the cat, the silvery fish who flee from great whales, or warriors who run away on skinny legs from ravaging birds of prey.", null, MiniGameInfoScreen.MiniGame.TavernaEnd);
				}
			}
		}
	}

	public void SCORE_PER_HIT() {
		score += 1;
	}
	public void SCORE_PER_HIT(int num) {
		score += num;
	}

	/// <summary>
	/// Currently ends game when you hit c after 5 misses
	/// </summary>
	public void SubtractTries() {
		tries -= 1;
	}

	private void ResetBallPosition() {
		playerPos.transform.position = playerStartPos;
		playerPos.transform.rotation = Quaternion.Euler(Vector3.zero);

		playerRb.useGravity = false;
		playerRb.velocity = Vector3.zero;
		playerRb.angularVelocity = Vector3.zero;

		for (int i = 0; i < playerPos.GetComponent<Transform>().childCount; i++) {
			playerPos.transform.GetChild(i).gameObject.SetActive(true);
		}
		tr.Launch = false;
	}

	private void ResetTargetPosition() {
		randomPlacement.GetComponent<KottabosRandomPlacement>().PlaceRandomPosition();
		//Stop velocity angular velocity
		childPos[5].localPosition = topTargetStartPos;
		childPos[5].rotation = topTargetStartRot;
		childPos[5].GetComponent<Rigidbody>().velocity = Vector3.zero;
		childPos[5].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

		IsHit = false;
	}

	private void ResetRound() {
		//ResetBallPosition
		ResetBallPosition();
		//ResetTargetPosition
		if (IsHit) {
			ResetTargetPosition();
		}
		SubtractTries();
	}

	public void LeaveKotaboas() {
		KottabosReset();
		TavernaController.BackToTavernaMenu();
	}

	public void RestartKottabos()	{
		KottabosReset();
		TavernaController.ReloadTavernaGame("Kottabos");
	}

	public void KottabosPauseMenu() {
		mgscreen.gameObject.SetActive(true);
		Time.timeScale = 0;
		mgscreen.DisplayText("Kottabos", "Taverna Game", "Kottabos is paused, here's where the controls will go", null, MiniGameInfoScreen.MiniGame.TavernaPause);
	}

	public void KottabosUnPauseMenu() {
		mgscreen.gameObject.SetActive(false);
		Time.timeScale = 1;
		mgscreen.CloseDialog();
	}

	private void KottabosPauseAndUnPause() {
		if (Input.GetKeyDown(KeyCode.Escape)) {
			if (Time.timeScale == 1) {
				KottabosPauseMenu();
			}
			else {
				KottabosUnPauseMenu();
			}
		}
	}
}
