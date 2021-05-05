using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// used for the tavern singing minigaame. TODO: Rename
public class SongMenuController : MonoBehaviour
{
	public GameObject game_state,main_state;
	//as a part of condincing this to one seen set the main menue and the song menue to there own game objects
	// on start the main state is true.
    public void PlayWisdomSong()
    {
		SongGameController.set_song(4);
		game_state.SetActive(true);
		main_state.SetActive(false);

	}

    public void PlayMilitarySong()
    {
		SongGameController.set_song(2);
		game_state.SetActive(true);
		main_state.SetActive(false);
	}

    public void PlayMournfulSong()
    {
		SongGameController.set_song(3);
		game_state.SetActive(true);
		main_state.SetActive(false);
	}

    public void PlayPartySong()
    {
		SongGameController.set_song(1);
		game_state.SetActive(true);
		main_state.SetActive(false);
	}

	public void MainMenuScene()
    {
		//use to relod the seen
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
	}

	public void BackToTavern() {
		TavernaController.BackToTavernaMenu();
	}

	// set the menu active before unloading the song scene since that will cancel the coroutine
	// TODO: Needs a refactor so that the context doesn't get lost on unloading the song (same issue with tavern menu)
	// (josh) I did not tuch this (I over looked it)
	IEnumerator UnloadSong() {
		var songScene = SceneManager.GetActiveScene();
		var songMenuScene = SceneManager.GetSceneByName("SongCompMainMenu");
		SceneManager.SetActiveScene(songMenuScene);
		yield return SceneManager.UnloadSceneAsync(songScene);
	}

	IEnumerator LoadSong(string sceneName) {
		yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
		var scene = SceneManager.GetSceneByName(sceneName);
        SceneManager.SetActiveScene(scene);
    }
}
