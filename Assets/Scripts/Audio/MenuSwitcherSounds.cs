using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuSwitcherSounds : MonoBehaviour
{
	[SerializeField] private SoundsForMenus sounds = null;

	//THIS ENTIRE CLASS IS TO START AND STOP SOUNDS FROM PLAYING WHEN CERTIAN PANELS ARE OPEN

	//These are being called by various buttons

	public void PlaySound(string name) {
		sounds.StopAllSounds();
		sounds.PlaySound(name);
	}

	//public void PlayDashboardSound() {
	//	Debug.Log("Playing Dashboard BGM");
	//	sounds.StopAllSounds();
	//	sounds.PlaySound("Dashboard");
	//}

	//public void StopDashboardSound() {
	//	sounds.StopSound("Dashboard");
	//}

	//public void PlayAgora() {
	//	Debug.Log("Playing Agora BGM");
	//	sounds.StopAllSounds();
	//	sounds.PlaySound("Agora");
	//}

	//public void StopAgora() {
	//	sounds.StopSound("Agora");
	//}

	//public void PlayTaverna() {
	//	Debug.Log("Playing Taverna BGM");
	//	sounds.StopAllSounds();
	//	sounds.PlaySound("Taverna");
	//}

	//public void StopTaverna() {
	//	sounds.StopSound("Taverna");
	//}
}


