using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Notifications
{
	Game Game => Globals.Game;
	UISystem UI => Globals.UI;

	// notifications
	public bool NotificationQueued { get; private set; }
	public string QueuedNotificationMessage { get; private set; }

	void ConsumeNotification() {
		NotificationQueued = false;
	}

	public void Pump() {
		//`````````````````````````````````````````````````````````````````
		//Check to see if we need to show any generic notifications ?
		if (NotificationQueued) {
			ShowNotification(QueuedNotificationMessage);
			Game.menuControlsLock = true;
			ConsumeNotification();
		}
	}

	void ShowNotification(string message) {

		// KD: I don't think the complexity of stacked notifications is needed. pretty sure it'd be okay to queue them up (stack them so you close one by one)
		// also changed to the new popup which i think looks better
		UI.Show<InfoScreen, InfoScreenModel>(new InfoScreenModel {
			Title = "Attention!",
			Message = message,
			OnClose = () => OnNotificationClose()
		});

	}

	public void OnNotificationClose() {
		if (!UI.IsShown<PortScreen>() && !UI.IsShown<CityView>()) {
			Game.menuControlsLock = false;
		}
	}



	public void ShowANotificationMessage(string message) {
		//First check if we have a primary message going already
		if (NotificationQueued) {
			//if we do then queue up a secondary message
			// KD: This secondary notif concept was never fully implemented so I'm just removing it for now. I think what this should really do is just pop up stacked modals on top of each other
			// and you can click through each one, but i'm holding on that for now. It just won't show the second notification (which preserves what the code was doing before since they never showed)
			//_showSecondaryNotification = true;
			//_secondaryNotificationMessage = message;
			//otherwise show a normal primary message
		}
		else {
			NotificationQueued = true;
			QueuedNotificationMessage = message;
		}
	}

}
