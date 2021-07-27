using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.IO;
using System.Linq;

public class LyricsController : MonoBehaviour
{
	
	public GameObject changingLyrics;
	//public Text lyricsText;
	public TMP_Text lyricsText;
	private string lyricsColor= "<color=red>";//set the tag for tex coloer flash
	public Image text_mask;
	public Scrollbar SB;
	private string currentLyricsIndex;
	private static string[] active_words;
	private static int word_count = 0;
	private int score_check;
	string sceneName;
	List<string> wisdomyLyricsList = new List<string>();
	List<string> militaryLyricsList = new List<string>();
	List<string> mournfulLyricsList = new List<string>();
	List<string> partyLyricsList = new List<string>();
	private float color_delay=0;
	public static int word_limit {
		//I am trying to not have to rework every thing so 
		//LyricsController and song controller need to pass some static info
		// but this is a safer way to do it
		get { return active_words.Length; }
	}
	public void reset() {
		//Awake();
		//Start();
	}
	public static bool word_check() 
		{
		//Debug.Log("words(" + word_count + "/" + word_limit);
		return word_count == word_limit; }
	// Start is called before the first frame update
	void Awake()
    {
		//Debug.Log(Application.persistentDataPath);
		
		word_count = 0;
		lyricsText.text = "";
		// Create a temporary reference to the current scene.
		Scene currentScene = SceneManager.GetActiveScene();

        // Retrieve the name of this scene.
        sceneName = currentScene.name;

        Debug.Log(SceneManager.GetActiveScene().name);
		//StreamReader SR = new StreamReader( + "\\wisdomy.txt");
		string myFile = Resources.Load<TextAsset>("Song\\wisdomy").text;
		// Finding which lyrics to use
		
		wisdomyLyricsList = myFile.Split('\n').ToList();
		myFile = Resources.Load<TextAsset>("Song\\military").text;
		militaryLyricsList = myFile.Split('\n').ToList();
		myFile = Resources.Load<TextAsset>("Song\\mournful").text;
		mournfulLyricsList = myFile.Split('\n').ToList();
		myFile = Resources.Load<TextAsset>("Song\\party").text;
		partyLyricsList = myFile.Split('\n').ToList();

		//(josh)decide witch list of songs to load up
		if (SongGameController.party_song)
        {
            currentLyricsIndex = wisdomyLyricsList[Random.Range(0, wisdomyLyricsList.Count)];
        }
        if (SongGameController.Sorrow_Song)
        {
            currentLyricsIndex = mournfulLyricsList[Random.Range(0, mournfulLyricsList.Count)];
        }
        if (SongGameController.War_song)
        {
            currentLyricsIndex = militaryLyricsList[Random.Range(0, militaryLyricsList.Count)];
        }
        if (SongGameController.Wisdom_Song)
        {
            currentLyricsIndex = partyLyricsList[Random.Range(0, partyLyricsList.Count)];
        }

		Debug.Log(militaryLyricsList.Count + "war songs detected");

		changeLyrics();
    }

    // Update is called once per frame
    void Update()
    {
		if (!SongGameController.endGameState) { if (SB.value != 0) { SB.value = 0; } }
		 if (SB.size != 1) { SB.gameObject.SetActive(false); }
			else  { SB.gameObject.SetActive(false); } 
		//Color lyricsColor = lyricsText.color;  //  sets color to object
		//lyricsColor.a = SongGameController.currentScore / SongGameController.targetScore;// changes the color of alpha
		// lyricsText.color = lyricsColor;
		// Debug.Log(lyricsColor.a);
		// GameManager.ChangeOppacityOfLyrics(lyricsText);
		//text_mask.fillAmount = SongGameController.currentScore / SongGameController.targetScore; 
		if (SongGameController.score_check>=100 && word_count<active_words.Length) 
		{//josh's notes loop to add one word for every 100 points 
			for(int i = SongGameController.score_check; i > 0; i -= 100) 
			{  // add one word for every 100 points
				if (word_count == active_words.Length) { break; }//prevent going out of bounds
				//josh's notes add HTML tages to give the thext coloer 
				lyricsText.text += " "+ lyricsColor + active_words[word_count]+"</color>";
				//lyricsText.text += " " + active_words[word_count];
				word_count++;
				color_delay = 0;
				Debug.Log("teat line wards+ " + word_count);
			
			}
			SongGameController.score_check = 0;
		}
		if (color_delay > 0.3f) {
			//remove the tages to go back to base coloer;
			lyricsText.text=lyricsText.text.Replace(lyricsColor, "");
			lyricsText.text = lyricsText.text.Replace("</color>", "");
			color_delay = 0;
			//Debug.Log("color remove");
		}
		color_delay += Time.deltaTime;
		
	}

    public void changeLyrics()
    {
        //using a game object 
        //changingLyrics.GetComponent<Text>().text = currentLyricsIndex; 

        //set the array of words to fill in
       active_words = currentLyricsIndex.Split(' ');
		lyricsText.text = "";
	}
}
