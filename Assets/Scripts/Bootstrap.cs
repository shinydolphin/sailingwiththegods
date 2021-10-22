using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrap : MonoBehaviour
{
	[SerializeField] string _baseScene = null;
	[SerializeField] string[] _additiveScenes = null;

	[SerializeField] Canvas _loadingScreen;

    IEnumerator Start()
    {
		DontDestroyOnLoad(gameObject);

		// base scene has to be loaded and activated first or unity won't load additive scenes over it
		// this is safe because our World scene has no dependencies on external code itself, though other things depend on *it*
		yield return SceneManager.LoadSceneAsync(_baseScene, LoadSceneMode.Single);

		Debug.Log("Base scene " + _baseScene + " loaded and activated.");

		var sceneLoads = new List<AsyncOperation>();
		foreach (var scene in _additiveScenes)
        {
			sceneLoads.Add(LoadWithoutActivation(scene));
        }

		Debug.Log("Waiting on scene loads: " + sceneLoads.Count);

		// wait for all scenes to load before activating
		while (sceneLoads.Any(load => load.progress < 0.9f)) {
			yield return new WaitForEndOfFrame();
		}

		Debug.Log("All additive scenes preloaded...");

		// activate all at once
		foreach(var load in sceneLoads) {
			load.allowSceneActivation = true;
		}
		while(sceneLoads.Any(loads => !loads.isDone)) {
			yield return new WaitForEndOfFrame();
		}

		Debug.Log("All additive scenes activated.");

		_loadingScreen.gameObject.SetActive(false);

		Debug.Log("Bootstrap completed.");
	}

	AsyncOperation LoadWithoutActivation(string scene) {
		Debug.Log("Loading additive " + scene);
		var baseLoad = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
		baseLoad.allowSceneActivation = false;
		return baseLoad;
	}
}
