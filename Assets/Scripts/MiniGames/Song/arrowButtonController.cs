using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ArrowButtonController : MonoBehaviour
{

    //so that arrow keys work when a key input is pressed
    public KeyCode keyToPress;
	//public Animator am;
    private Button _button;
	//public Color good, bad, perfect;
	public Button arrow;
	public Image pic;
	public Color pressed_color;
	Color defalt_color;
	private bool inrange = false;
	// Start is called before the first frame update
	void Start()
    {
        _button = GetComponent<Button>();
		pic = gameObject.GetComponent<Image>();
		defalt_color = pic.color; ;
		
	}
	public void flash() 
	{
	//	am.SetTrigger("flash");
	}
    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(keyToPress))
        {
			if (inrange) { pic.color = pressed_color; }
			//changes button color when key is pressed
            FadeToColor(_button.colors.pressedColor);
			//ClickButton
			_button.onClick.Invoke();
           // Debug.Log("button pressed");
        }
        else if (Input.GetKeyUp(keyToPress))
        {
            FadeToColor(_button.colors.normalColor);
			pic.color = defalt_color;
		}
    }

	private void OnTriggerExit2D(Collider2D other) {
		if (other.gameObject.tag == "Arrow") 
			{ inrange = false; }
	}
	private void OnTriggerEnter2D(Collider2D other) {
		if (other.gameObject.tag=="Arrow") { inrange = true; Debug.Log("turn green"); }
	}
	//To change button color 
	public void FadeToColor(Color color)
	{
    Graphic graphic = GetComponent<Graphic>();
    graphic.CrossFadeColor(color, _button.colors.fadeDuration, true, true);
	graphic.CrossFadeColor(color, arrow.colors.fadeDuration, true, true);
	}
}
