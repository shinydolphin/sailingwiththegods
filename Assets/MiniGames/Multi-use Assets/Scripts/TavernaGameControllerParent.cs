using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TavernaGameControllerParent : MonoBehaviour
{
	public AudioSource moveSound;

	[Header("Main UI")]
	public MiniGameInfoScreen mgScreen;
	public Sprite gameIcon;
	public TavernaMiniGameDialog playerBarks;
	public TavernaEnemyDialog enemyBarks;
	[Range(0f, 1f)]
	public float barkChance = 0.75f;

	[Header("Text")]
	[TextArea(2, 6)]
	public string introText;
	[TextArea(2, 6)]
	public string instructions;
	[TextArea(2, 6)]
	public string history;
	[TextArea(2, 6)]
	public string winText;
	[TextArea(2, 6)]
	public string loseText;

	public virtual void PauseMinigame() {
		mgScreen.gameObject.SetActive(true);
		Time.timeScale = 0;
	}

	public void UnpauseMinigame() {
		mgScreen.gameObject.SetActive(false);
		Time.timeScale = 1;
	}

	public void ExitMinigame() {
		TavernaController.BackToTavernaMenu();
	}

	public virtual void RestartMinigame() 
	{
		//Restart minigame
	}


	/// <summary>
	/// Plays the movement sound at a random pitch
	/// </summary>
	public void PlayMoveSound() {
		moveSound.pitch = Random.Range(0.7f, 1.1f);
		moveSound.Play();
	}
}
