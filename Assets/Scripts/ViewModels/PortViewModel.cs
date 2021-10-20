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

	public PortViewModel(bool townAccess = true) : base(Globals.Game.Session.currentSettlement, null){
		CrewManagement = new CrewManagementViewModel(City);
		allowTownAccess = townAccess;
	}

	public void GoToTown() {
		UI.Hide<PortScreen>();
		UI.Show<TownScreen, TradeViewModel>(World.MasterGUISystem.Trade);
	}
}
