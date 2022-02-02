using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MiniGames : MonoBehaviour
{
	World World => Globals.World;

	Scene? Scene;

	private void Awake() {
		Globals.Register(this);
	}

	/// <summary>
	/// True if any minigame is active, whether it's an additive scene or a child game object
	/// </summary>
	public bool IsMiniGameActive { get; private set; }
	public bool IsMiniGameSceneActive { get; private set; }

	/// <summary>
	/// Start a minigame that is in a separate scene which will be additively loaded on top of the current scene.
	/// The scene should have its own Camera since the main camera will be disabled.
	/// Remember to add the scene to BuildSettings
	/// Calling Exit will unload the additive scene.
	/// </summary>
	public void EnterScene(string additiveSceneName) {
		EnterInternal(disableCamera: true);

		IsMiniGameSceneActive = true;
		RenderSettings.fog = false;
		SetMainSceneObjectsEnabled(false);

		SceneManager.LoadScene(additiveSceneName, LoadSceneMode.Additive);
		Scene = SceneManager.GetSceneByName(additiveSceneName);
	}

	/// <summary>
	/// Start a minigame that is in a prefab. It's instantiated on Enter to ensure it starts fresh every time, as a child of this object.
	/// The prefab can act as the origin of the mini-game coordinate space so the mini-game can live anywhere in the world.
	/// The scene should have its own Camera since the main camera will be disabled.
	/// Calling Exit will destroy the child object.
	/// </summary>
	public void Enter(string prefabName){
		EnterInternal(disableCamera: false);
		Instantiate(Resources.Load<GameObject>(prefabName)).transform.SetParent(transform);
	}

	/// <summary>
	/// End any currently active minigame, whether it's an additive scene or a child game object 
	/// </summary>
	public void Exit() {

		// shut off all minigames
		for (var i = 0; i < transform.childCount; i++) {
			Destroy(transform.GetChild(i).gameObject, .1f);
		}

		StartCoroutine(ExitInternal());
	}

	/// <summary>
	/// Reloads the given minigame scene
	/// </summary>
	/// <param name="additiveSceneName"></param>
	public void ReloadScene(string additiveSceneName) 
	{
		StartCoroutine(Reload(additiveSceneName));
	}

	void EnterInternal(bool disableCamera) {
		CutsceneMode.Enter();
		IsMiniGameActive = true;

		World.camera_Mapview.SetActive(false);

		if(disableCamera) {
			World.FPVCamera.SetActive(false);
		}
	}

	IEnumerator ExitInternal() {

		// unload all additive minigame scenes. don't leave cutscene mode until its done to avoid weirdness
		if (Scene.HasValue) {
			yield return SceneManager.UnloadSceneAsync(Scene.Value);
			Scene = null;
		}

		CutsceneMode.Exit();
		IsMiniGameActive = false;

		if(IsMiniGameSceneActive) {
			RenderSettings.fog = true;
			SetMainSceneObjectsEnabled(true);
			IsMiniGameSceneActive = false;
		}

		World.camera_Mapview.SetActive(true);
		World.FPVCamera.SetActive(true);

	}

	/// <summary>
	/// Unloads and reloads the given scene
	/// </summary>
	/// <param name="additiveSceneName"></param>
	/// <returns></returns>
	IEnumerator Reload(string additiveSceneName) 
	{
		yield return SceneManager.UnloadSceneAsync(additiveSceneName);

		//Often you'll be reloading from a pause menu where timeScale = 0 so we need to fix that
		Time.timeScale = 1;

		yield return SceneManager.LoadSceneAsync(additiveSceneName, LoadSceneMode.Additive);
		yield return null;
		Scene = SceneManager.GetSceneByName(additiveSceneName);
		SceneManager.SetActiveScene(Scene.Value);
	}

	// scene minigames usually live at the origin, so this disables things that get in the way of the additively loaded minigames
	void SetMainSceneObjectsEnabled(bool enabled) {
		World.crewBeacon.IsTemporarilyHidden = !enabled;
		World.navigatorBeacon.IsTemporarilyHidden = !enabled;
		//Globals.World.terrain.GetComponent<Terrain>().drawHeightmap = enabled;
		World.terrain.SetActive(enabled);
		GameObject mainLight = GameObject.FindGameObjectWithTag("main_light_source");
		if (mainLight != null) {
			mainLight.GetComponent<Light>().enabled = enabled;
		}
	}
}
