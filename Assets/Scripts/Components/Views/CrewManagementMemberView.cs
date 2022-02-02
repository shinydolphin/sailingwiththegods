using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.UI;

public class CrewManagementMemberViewModel : Model
{
	Database Database => Globals.Database;

	GameSession Session { get; set; }

	public CrewMember Member { get; private set; }
	
	public Sprite Portrait { get; private set; }
	public string Name => Member.name;
	public string City => Database.GetSettlementFromID(Member.originCity).name;
	public string Job => Database.GetJobClassEquivalency(Member.typeOfCrew);
	public string BackgroundInfo => Member.backgroundInfo;

	public string Role => "<#000000>" + Job + "</color>" + "\n" + Skills;

	public bool IsInCrew => Session.playerShipVariables.ship.crewRoster.Contains(Member);
	public string Skills => IsInCrew ? Member.changeOnFire.ToString() : Member.changeOnHire.ToString();
	
	public string NumConnectionsStr => CitiesInNetwork == null ? 
		"" : 
		CitiesInNetwork.Count() + " Connections";

	public readonly ICollectionModel<CityViewModel> CitiesInNetwork;

	//TODO Temporary solution--need to add a clout check modifier
	public int CostToHire => Member.clout * 2;

	private Action<CrewManagementMemberViewModel> _OnClick;
	public Action<CrewManagementMemberViewModel> OnClick { get => _OnClick; set { _OnClick = value; Notify(); } }


	public CrewManagementMemberViewModel(GameSession session, CrewMember member, Action<CrewManagementMemberViewModel> onClick, Action<CityViewModel> onClickCity) {
		Member = member;
		OnClick = onClick;
		Portrait = member.PortraitSprite();
		Session = session;
		
		// don't bother building the network list if we're being created for a view that doesn't need it
		// sort by port name so you can easily look up a port in the list
		if(onClickCity != null) {
			CitiesInNetwork = ValueModel.Wrap(new ObservableCollection<CityViewModel>(
				Session.Network.GetCrewMemberNetwork(Member)
					.Select(s => new CityViewModel(session, s, onClickCity))
					.OrderBy(c => c.PortName)
			));
		}
	}
}

public class CrewManagementMemberView : ViewBehaviour<CrewManagementMemberViewModel>
{
	UISystem UI => Globals.UI;

	[SerializeField] ButtonView InfoButton = null;
	[SerializeField] ButtonView ActionButton = null;
	[SerializeField] ImageView Portrait = null;
	[SerializeField] StringView Name = null;
	[SerializeField] StringView City = null;
	[SerializeField] StringView Skills = null;
	[SerializeField] StringView Cost = null;
	[SerializeField] StringView CitiesContributed = null;

	public override void Bind(CrewManagementMemberViewModel model) {
		base.Bind(model);

		InfoButton?.Bind(ValueModel.New(new ButtonViewModel {
			OnClick = () => UI.Show<InfoScreen, InfoScreenModel>(new InfoScreenModel {
				Icon = model.Portrait,
				Title = model.Name,
				Subtitle = model.Job,
				Message = model.BackgroundInfo
			})
		}));

		ActionButton?.Bind(ValueModel.New(new ButtonViewModel {
			Label = model.IsInCrew ? "Fire" : "Hire",
			OnClick = () => model.OnClick?.Invoke(Model)
		}));

		Name?.Bind(new BoundModel<string>(Model, nameof(Model.Name)));
		City?.Bind(new BoundModel<string>(Model, nameof(Model.City)));
		Skills?.Bind(new BoundModel<string>(Model, nameof(Model.Role)));
		Cost?.Bind(new BoundModel<int>(Model, nameof(Model.CostToHire)).AsString());
		Portrait?.Bind(new BoundModel<Sprite>(Model, nameof(Model.Portrait)));
		CitiesContributed?.Bind(new BoundModel<string>(Model, nameof(Model.NumConnectionsStr)));
	}
}
