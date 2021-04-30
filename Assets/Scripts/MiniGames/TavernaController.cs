using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

// used for tavern minigame menu
public class TavernaController : MonoBehaviour
{
	public AudioListener tavernaListener;

	private Scene thisScene;

	private void Awake() {
		thisScene = SceneManager.GetSceneByName("TavernaMenu");
	}

	public void StartPetteia() {
		StartCoroutine(LoadTavernaGame("Petteia"));
	}

	public void StartUr() {
		StartCoroutine(LoadTavernaGame("Ur"));
	}

	public void StartSong() {
		StartCoroutine(LoadTavernaGame("SongCompMainMenu"));
	}
	
	public void StartKottaboas() {
		StartCoroutine(LoadTavernaGame("Kottaboas_Game"));
	}

	public void StartTavernaConvo() {
		LeaveTavernaScene();
		Globals.UI.Show<DialogScreen>().StartDialog("Start_Taverna", "taverna");
	}

	private void Update() {
		if (SceneManager.GetActiveScene().Equals(thisScene) && !tavernaListener.enabled) 
		{
			ToggleTavernaObjects(true);
		}
	}

	// can't use static methods in unityevent inspector
	public void TavernaMenu() {
		BackToTavernaMenu();
	}

	/// <summary>
	/// Returns to the main taverna scene
	/// </summary>
	public static void BackToTavernaMenu() {

		TavernaController controller = GetTavernaControllerInstance();
		
		if(controller != null) {
			controller.GetComponentInParent<Canvas>().enabled = true;
			controller.StartCoroutine(UnloadTavernaGame());
		}
	}

	/// <summary>
	/// Unloads and reloads the given scene
	/// </summary>
	/// <param name="sceneName"></param>
	public static void ReloadTavernaGame(string sceneName) 
	{
		TavernaController controller = GetTavernaControllerInstance();

		if (controller != null) {
			controller.StartCoroutine(TavernaReload(sceneName, controller));
		}
		else {
			Debug.Log("Couldn't find the controller");
		}
	}

	// Moved to its own function so it can be used multiple places and then when we refactor it we only change it the once
	// TODO: This needs refactoring. This script is sometimes added on minigames too and used to unload the game, but we're disabling it in the original scene and need to turn it back on...
	// we can find the real one by looking for the one with a canvas above it
	private static TavernaController GetTavernaControllerInstance() {
		return GameObject.FindObjectsOfType<TavernaController>().FirstOrDefault(d => d.GetComponentInParent<Canvas>() != null);
	}

	/// <summary>
	/// Handles the actual reloading of the given scene
	/// </summary>
	/// <param name="sceneName"></param>
	/// <param name="c"></param>
	/// <returns></returns>
	private static IEnumerator TavernaReload(string sceneName, TavernaController c) 
	{
		//Unloads and then sets this as the active scene
		yield return SceneManager.UnloadSceneAsync(sceneName);
		SceneManager.SetActiveScene(SceneManager.GetSceneByName("TavernaMenu"));
		//If you reloaded from a pause menu, your timescale is 0, so it needs to get set back to 1
		Time.timeScale = 1;
		//Loads in the same scene again, then sets it active
		yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
		yield return null;
		SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
		//Makes sure the taverna's audioListener is off
		c.ToggleTavernaObjects(false);
	}

	static IEnumerator UnloadTavernaGame() {
		yield return SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
		Time.timeScale = 1;
		Scene scene = SceneManager.GetSceneByName("TavernaMenu");
		SceneManager.SetActiveScene(scene);
	}

	public void LeaveTavernaScene() {
		Globals.MiniGames.Exit();
	}

	// load individual minigame scenes on top of the main menu, leaving main menu open
	IEnumerator LoadTavernaGame(string sceneName) {
		yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
		ToggleTavernaObjects(false);
		Scene scene = SceneManager.GetSceneByName(sceneName);
		SceneManager.SetActiveScene(scene);

		GetComponentInParent<Canvas>().enabled = false;
	}

	private void ToggleTavernaObjects(bool toggle) {
		Debug.Log($"Toggling {name}: {toggle}");
		tavernaListener.enabled = toggle;
	}

	
}
