using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SongGameController : MonoBehaviour
{
	public static bool party_song=false, War_song=false, Sorrow_Song=false, Wisdom_Song=true;

	private AudioSource[] musicList;
	public AudioSource[] party_list, War_list, Sorrow_list, Wisdom_list;
	private int songChoiceNum;
	
    public static bool startPlaying;
	public CanvasGroup win, lose;
	//End Game Manager
	public static float targetScore;
    public float gameEndTimerValue;
    private float gameEndTimerHolder;
    public static bool endGameState;

    public ArrowController arrowController;

    //score holders
    public static float currentScore;
	public  static int score_check;
	public int scorePerNote = 100;

    public static int currentMultiplier;
    public int multiplierTracker;
    public int[] multiplierThresholds;
	
	public static float Soere_speed {
		get {
			//play with this formula to adhust arrow speed based on scrore
			float x = (currentScore / targetScore) ;
			//Debug.Log("X speed " + x);
			if (currentMultiplier > 2) { return 1 + x + (float)(currentMultiplier / 2f); }
			else { return 1 + x; }
			
		}
	}
	//text boxes
	public Text scoreText;
    public Text multiText;
   // public Text lyricsText;
	private Color lyricsColor;

    //Made static so that there is only one instance of mattsGameManager at a time
    public static SongGameController instance;
	public void reset() 
	{
		//Awake();
		//Start();
	}
	public static void set_song(int x) 
	{
		party_song = false;
		War_song = false;
		Sorrow_Song = false; 
		Wisdom_Song = false;
		switch (x) 
		{
			case 1: party_song = true; break;
			case 2: War_song = true; break;
			case 3: Sorrow_Song = true; break;
			case 4: Wisdom_Song = true; break;
		}
	}
	private void Awake() {
		if (party_song) { musicList = party_list; }
		else if (War_song) { musicList = War_list; }
		else if (Sorrow_Song) { musicList = Sorrow_list; }
		else if (Wisdom_Song) { musicList = Wisdom_list; }
		currentScore = 0;
	}
	// Start is called before the first frame update
	void Start()
    {
        instance = this;
        //lyricsColor = lyricsText.color;  //  sets color to object
        lyricsColor.a = 0.0f; // makes the color transparent
        targetScore = 100*LyricsController.word_limit;


        //Resetting score at Start
        scoreText.text = "Score: 0";
        multiText.text = "Multiplier: 1x";
        currentMultiplier = 1;
        songChoiceNum = Random.Range(0, musicList.Length);

        endGameState = false;
        startPlaying = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Debug.Log(gameEndTimerHolder);
        if (!startPlaying)
        {
            if (Input.anyKeyDown)
            {
                gameEndTimerHolder = 0;
                startPlaying = true;
                arrowController.hasStarted = true;
                musicList[songChoiceNum].Play();
				gameEndTimerValue = musicList[songChoiceNum].clip.length;

			}
        }

        if (startPlaying)
        {
            gameEndTimerHolder += Time.deltaTime;
        }

        //End Game Manger
        if (gameEndTimerHolder >= gameEndTimerValue && !(currentScore > targetScore))
        {
            endGameState = true;
			lose.interactable = true;
			lose.alpha += Time.deltaTime;
		}
        if (currentScore >= targetScore)
        {
			//if (!endGameState) 
			//	{ Globals.GameVars.AdjustPlayerClout(5, false); }
            endGameState = true;
			win.interactable = true;
			win.alpha += Time.deltaTime;
			
		}

        //ending game 
        if (endGameState)
        {
			musicList[songChoiceNum].volume -= .4f * Time.deltaTime;
			if (musicList[songChoiceNum].volume <= 0) 
				{ musicList[songChoiceNum].Stop(); }
			
        }
    }

    //what happens when we hit a note
    public void NoteHit()
    {
        //Debug.Log("Hit On Time");

        if (currentMultiplier - 1 < multiplierThresholds.Length)
        {
            multiplierTracker++;

            //increases multiplier at each threshold 
            //then resets tracker to 0 so we can get to next threshold
            if (multiplierThresholds[currentMultiplier - 1] <= multiplierTracker)
            {
                multiplierTracker = 0;
                currentMultiplier++;
            }
        }

        multiText.text = "Multiplier: x:" + currentMultiplier;

        currentScore += scorePerNote * currentMultiplier;
		score_check = scorePerNote * currentMultiplier;//only line I added here
		scoreText.text = "Score: " + currentScore;
    }

    //what happens when we miss a note
    public void NoteMissed()
    {
        //Debug.Log("Missed Note");

        currentMultiplier = 1;
        multiplierTracker = 0;

        multiText.text = "Multiplier: x:" + currentMultiplier;
    }

   // public static void ChangeOppacityOfLyrics (Text lyricsText)
	//{
      //  Color lyricsColor = lyricsText.color;  //  sets color to object
        //lyricsColor.a = targetScore % currentScore; // changes the color of alpha
		//Debug.Log(lyricsColor.a);
   // }
}
