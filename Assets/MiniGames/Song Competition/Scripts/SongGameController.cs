using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class SongGameController : MonoBehaviour
{
	public static bool party_song=false, War_song=false, Sorrow_Song=false, Wisdom_Song=true;
	public Text victory, failure;
	private AudioSource[] musicList;
	public AudioSource[] party_list, War_list, Sorrow_list, Wisdom_list;
	private int songChoiceNum;
	public Animator am;
    public static bool startPlaying;
	public CanvasGroup win, lose;
	//End Game Manager
	public static float targetScore;
    public float gameEndTimerValue;
    private float gameEndTimerHolder;
    public static bool endGameState;

    public ArrowController arrowController;
	private List<string> lose_message = new List<string>();
	private List<string> lose_message_badly = new List<string>();
	private List<string> did_not_win_message = new List<string>();
	private List<string> Win_message = new List<string>();
	bool setmessage;
	//score holders
	public static float currentScore;
	public  static int score_check;
	public int scorePerNote = 100;
	

	public static int currentMultiplier;
    public int multiplierTracker;
    public int[] multiplierThresholds;
	
	public static float Soere_speed {
		get {
			//josh's notes I made this to act as a multiplyer for ArrowController.cs line 35 
			// it was meet to make it harder as you got more points 
			// I never got it quite right so I have this as a place holder for now
			// feel free to change it

			float x = (currentScore / targetScore) *2;
			return 1 + x;

		}
	}
	//text boxes
	public Text scoreText;
    public Text multiText;

    //Made static so that there is only one instance of mattsGameManager at a time
    public static SongGameController instance;
	public static void set_song(int x) 
	{
		//quick check to see what song to play
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
	void set_end_message() 
	{
		//reading the messages from the files
		string myFile = Resources.Load<TextAsset>("Song\\You Lose Badly").text;
		lose_message_badly = myFile.Split('\n').ToList();

		 myFile = Resources.Load<TextAsset>("Song\\You Lose").text;
		lose_message = myFile.Split('\n').ToList();
		myFile = Resources.Load<TextAsset>("Song\\You Win by almost nothing").text;
		did_not_win_message = myFile.Split('\n').ToList();
		myFile = Resources.Load<TextAsset>("Song\\You Win").text;
		Win_message = myFile.Split('\n').ToList();

		
		
	}
	private void Awake() {
		setmessage = false;
		if (party_song) {
			musicList = party_list;
		}
		else if (War_song) {
			musicList = War_list;
		}
		else if (Sorrow_Song) {
			musicList = Sorrow_list;
		}
		else if (Wisdom_Song) {
			musicList = Wisdom_list;
		}
		currentScore = 0;
		am.SetBool("open", false);
		set_end_message();
	}
	// Start is called before the first frame update
	void Start()
    {
		//not sure if this is used now
        instance = this;
        
		
		//josh's notes set the ending target (may not be used after the last cchanges)
		targetScore = 100*LyricsController.word_limit;


        //Resetting score at Start
        scoreText.text = "Score: 0";
        multiText.text = "Multiplier: 1x";
        currentMultiplier = 1;
        songChoiceNum = Random.Range(0, musicList.Length);

        endGameState = false;
        startPlaying = false;
    }

	void lose_game(float c) 
	{
		if (!setmessage) 
			{
			if (c > .6f) { failure.text = lose_message[Random.Range(0, lose_message.Count - 1)]; }
			else { failure.text = lose_message_badly[Random.Range(0, lose_message_badly.Count - 1)]; }
		}
		//failure
		
		int x = Random.Range(1, 4);
		endGameState = true;
		lose.interactable = true;
		//lose.alpha += Time.deltaTime; cant use here
		setmessage = true;
	}

	void win_game(float score) {
		int clout = 0;
		if (!setmessage) 
			{
			if (score > .60f) {
				clout = (int)(15 * score / 100);
				victory.text = Win_message[Random.Range(0, Win_message.Count - 1)] + " (clout +" + clout + ")";
				
			}
			else {
				clout = 0; //no grade as the claint put it
				victory.text = did_not_win_message[Random.Range(0, did_not_win_message.Count - 1)]+" (clout +"+clout+")";
				
			}
		}
		
		
		endGameState = true;		
		win.interactable = true;
		win.alpha += Time.deltaTime;
		if (!endGameState && Globals.World != null) { Globals.Game.Session.AdjustPlayerClout(clout, false); }
		setmessage = true;
	}
	// Update is called once per frame
	void Update()
    {
		float score_precent = (currentScore / targetScore) * 100;
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
				am.SetBool("open",true);
			}
        }

        if (startPlaying)
        {
            gameEndTimerHolder += Time.deltaTime;
        }

		//End Game Manger
		if (LyricsController.word_check()) {//check for win condisions

			//Debug.Log("score check1 " + score_precent);
			//Debug.Log("score check2 " + score_precent/100);

			win_game(score_precent);

		}
		else  if (!endGameState && gameEndTimerHolder >= gameEndTimerValue) {//check for lose condisions
			lose_game(score_precent);
			Debug.Log("time up");
			lose.alpha = 1;
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
		score_check += scorePerNote * currentMultiplier;//ref to add the wards in LyricsController.cs
		scoreText.text = "Score: " + currentScore;
    }

    //what happens when we miss a note
    public void NoteMissed()
    {
        //Debug.Log("Missed Note");

        currentMultiplier = 1;
        multiplierTracker = 0;
		currentScore -= scorePerNote * currentMultiplier;
		if (currentScore < 0) { currentScore = 0; }
		multiText.text = "Multiplier: x:" + currentMultiplier;
		scoreText.text = "Score: " + currentScore;
	}

   // public static void ChangeOppacityOfLyrics (Text lyricsText)
	//{
      //  Color lyricsColor = lyricsText.color;  //  sets color to object
        //lyricsColor.a = targetScore % currentScore; // changes the color of alpha
		//Debug.Log(lyricsColor.a);
   // }
}
