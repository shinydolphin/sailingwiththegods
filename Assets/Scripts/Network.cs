using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Network
{
	const int INDEPENDENT = 0;

	Ship Ship => Session.playerShipVariables.ship;

	GameSession Session => Globals.Session;
	Database Database => Globals.Database;

	public Network() {
	}

	public bool CheckForNetworkMatchBetweenTwoSettlements(int cityA, int cityB) {
		Settlement cityAObj = Database.GetSettlementFromID(cityA);
		Settlement cityBObj = Database.GetSettlementFromID(cityB);
		foreach (int cityA_ID in cityAObj.networks) {
			foreach (int cityB_ID in cityBObj.networks) {
				if (cityA_ID == cityB_ID && cityA_ID != INDEPENDENT) {
					return true;
				}
			}
		}
		return false;
	}

	public IEnumerable<Settlement> GetCitiesFromNetwork(int netId) => Database.settlement_masterList.Where(s => s.networks.Contains(netId));

	public IEnumerable<Settlement> MyImmediateNetwork => Ship.networks
		.Where(netId => netId != INDEPENDENT)
		.SelectMany(netId => GetCitiesFromNetwork(netId))
		.Concat(new[] { Database.GetSettlementFromID(Ship.originSettlement) });

	public IEnumerable<Settlement> GetCrewMemberNetwork(CrewMember crew) =>
		Database.GetSettlementFromID(crew.originCity).networks
			.Where(netId => netId != INDEPENDENT)
			.SelectMany(netId => GetCitiesFromNetwork(netId))
			.Concat(new[] { Database.GetSettlementFromID(crew.originCity) });

	public IEnumerable<Settlement> MyCompleteNetwork => Ship.crewRoster
		.SelectMany(crew => GetCrewMemberNetwork(crew))
		.Concat(MyImmediateNetwork);

	public IEnumerable<CrewMember> CrewMembersWithNetwork(Settlement settlement, bool includeJason = false) {
		var list = Ship.crewRoster.Where(crew => GetCrewMemberNetwork(crew).Contains(settlement));
		if (includeJason && GetCrewMemberNetwork(Session.Crew.Jason).Contains(settlement)) {
			list = list.Concat(new[] { Session.Crew.Jason });
		}
		return list;
	}

	public CrewMember CrewMemberWithNetwork(Settlement settlement) => CrewMembersWithNetwork(settlement).FirstOrDefault();

	public bool CheckIfCityIDIsPartOfNetwork(int cityID) {
		var settlement = Database.GetSettlementFromID(cityID);
		return settlement != null && MyCompleteNetwork.Contains(settlement);
	}
}
