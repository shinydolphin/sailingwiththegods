using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class PortViewModel : CityViewModel
{
	public readonly CrewManagementViewModel CrewManagement;

	public bool allowTownAccess;

	public PortViewModel(GameSession session, Notifications notifications, bool townAccess = true) : base(session, Globals.Game.Session.currentSettlement, null){
		CrewManagement = new CrewManagementViewModel(session, notifications, City);
		allowTownAccess = townAccess;
	}

	public void GoToTown() {
		UI.Hide<PortScreen>();
		UI.Show<TownScreen, TradeViewModel>(masterGUISystem.Trade);
	}
}
