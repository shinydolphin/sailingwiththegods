using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using TMPro;

public class time_Line_controll : MonoBehaviour
{
	
	private bool paused = false;
	private PlayableDirector TL;
	public CanvasGroup desply;
	public CanvasGroup buttons;
	public float fade_speed = 3;
	public TextMeshProUGUI diolag;
	private bool play_tutorial;
	static GameObject objectref;
	// Start is called before the first frame update
	void Start()
    {
		TL = gameObject.GetComponent<PlayableDirector>();
		diolag.text = "That is a fine story. but you seem rather young\n tell me have you braved theser waters before?";
		//static ref for thee play funtion
		if (objectref == null) { objectref = this.gameObject; }
		objectref.SetActive(false);

	}

	// Update is called once per frame
	public static void play() 
	{
		objectref.SetActive(true);
	}
	void Update() {
		if (TL.playableGraph.IsValid()){
			if (paused) {
			desply.interactable = true;
			desply.alpha += fade_speed * Time.deltaTime;
			buttons.interactable = true;
			buttons.alpha += fade_speed/6 * Time.deltaTime;
			}
			else if (!paused) {
				//desply.interactable = false;
				//desply.alpha -= fade_speed * Time.deltaTime;
				buttons.interactable = false;
				buttons.alpha -= fade_speed * Time.deltaTime;
			}
		}
	}

	public void pause() 
	{
		if (TL.playableGraph.IsValid()) 
		{
			if (!paused) {
				TL.playableGraph.GetRootPlayable(0).SetSpeed(0);
				paused = true;
			}
			else {
				TL.playableGraph.GetRootPlayable(0).SetSpeed(1);
				paused = false;
			}
		}
			
	}
	public void set_option(bool x) 
	{
		if (x == true) 
			{ diolag.text = "Ah then I will leave you to it then"; }
		else 
		{
			diolag.text = "then allow me to give you some advice";
		}
		play_tutorial = x;
	}
	public void startgame() 
	{
		if (play_tutorial) 
		{
			//add code to start tutorial here
		}
		objectref.SetActive(false);
		//turn off the intro object ad all its componitnts
	}
}
