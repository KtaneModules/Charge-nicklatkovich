using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class ChargePuzzle {
	public const int WIDTH = 8;
	public const int HEIGHT = 3;

	public struct Info {
		public readonly KMBombInfo bomb;
		public readonly int startingTimeInMinutes;
		public Info(KMBombInfo bomb, int startingTimeInMinutes) {
			this.bomb = bomb;
			this.startingTimeInMinutes = startingTimeInMinutes;
		}
	}

	public struct PowerGroup {
		public readonly int power;
		public readonly KeyValuePair<Color, Color> trueColor;
		public readonly KeyValuePair<Color, Color> falseColor;
		public readonly System.Func<Info, bool> condition;
		public PowerGroup(int power, Color trueColor1, Color trueColor2, Color falseColor1, Color falseColor2, System.Func<Info, bool> condition) {
			this.power = power;
			this.trueColor = new KeyValuePair<Color, Color>(trueColor1, trueColor2);
			this.falseColor = new KeyValuePair<Color, Color>(falseColor1, falseColor2);
			this.condition = condition;
		}
	}

	public static readonly PowerGroup[] groups = new PowerGroup[] {
		new PowerGroup(5, Color.black, Color.yellow, Color.magenta, Color.red, (info) => info.bomb.GetTwoFactorCodes().Any((code) => code % 2 == 0)),
		new PowerGroup(11, Color.magenta, Color.blue, Color.red, Color.white, (info) => info.bomb.GetIndicators().Count() > 1),
		new PowerGroup(13, Color.magenta, Color.yellow, Color.magenta, Color.white, (info) => info.startingTimeInMinutes < 59),
		new PowerGroup(7, Color.yellow, Color.red, Color.red, Color.blue, (info) => info.bomb.GetSerialNumberNumbers().Count() == 3),
		new PowerGroup(3, Color.white, Color.yellow, Color.red, Color.black, (info) => info.bomb.GetSolvedModuleIDs().Count() > 13),
		new PowerGroup(17, Color.blue, Color.yellow, Color.blue, Color.black, (info) => info.bomb.IsPortPresent(Port.StereoRCA)),
		new PowerGroup(2, Color.magenta, Color.black, Color.blue, Color.white, (info) => info.bomb.GetStrikes() == 0),
	};

	public struct PowerSourceProps {
		public readonly bool active;
		public readonly int groupIndex;
		public PowerSourceProps(bool active, int groupIndex) {
			this.active = active;
			this.groupIndex = groupIndex;
		}
	}

	public int requiredChargeLevel = 0;
	public PowerSourceProps[][] powergrid;
	public HashSet<KeyValuePair<int, int>> allowedConnections;
	public HashSet<KeyValuePair<int, int>> solutionExample;

	private bool[] _validities = new bool[groups.Length];

	public ChargePuzzle() {
		powergrid = new PowerSourceProps[WIDTH][];
		for (int i = 0; i < WIDTH; i++) {
			powergrid[i] = new PowerSourceProps[HEIGHT];
			for (int j = 0; j < HEIGHT; j++) powergrid[i][j] = new PowerSourceProps(Random.Range(0, 2) == 0, Random.Range(0, groups.Length));
		}
		UpdateAllowedConnections();
		while (allowedConnections.Count < 6) {
			KeyValuePair<int, int> cellToActivate = new KeyValuePair<int, int>(0, 0);
			int inactivesCount = 0;
			for (int i = 0; i < WIDTH; i++) {
				for (int j = 0; j < HEIGHT; j++) {
					if (!powergrid[i][j].active) {
						inactivesCount += 1;
						if (Random.Range(0, inactivesCount) == 0) cellToActivate = new KeyValuePair<int, int>(i, j);
					}
				}
			}
			powergrid[cellToActivate.Key][cellToActivate.Value] = new PowerSourceProps(true, powergrid[cellToActivate.Key][cellToActivate.Value].groupIndex);
			UpdateAllowedConnections();
		}
		int solutionConnectionsCount = Random.Range(3, Mathf.Min(6, allowedConnections.Count - 3));
		HashSet<KeyValuePair<int, int>> possibleSolutionConnections = new HashSet<KeyValuePair<int, int>>(allowedConnections);
		solutionExample = new HashSet<KeyValuePair<int, int>>();
		while (solutionExample.Count < solutionConnectionsCount) {
			KeyValuePair<int, int> connection = possibleSolutionConnections.PickRandom();
			possibleSolutionConnections.Remove(connection);
			solutionExample.Add(connection);
			requiredChargeLevel += GetConnectionPower(connection);
		}
	}

	public int GetConnectionPower(KeyValuePair<int, int> connection) {
		int result = 0;
		for (int i = connection.Key; i <= connection.Value; i++) {
			for (int j = 0; j < HEIGHT; j++) {
				PowerSourceProps cell = powergrid[i][j];
				if (!cell.active) continue;
				result += groups[cell.groupIndex].power;
			}
		}
		return result;
	}

	public KeyValuePair<Color, Color> GetColor(int i, int j) {
		PowerSourceProps cell = powergrid[i][j];
		PowerGroup group = groups[cell.groupIndex];
		return _validities[cell.groupIndex] == cell.active ? group.trueColor : group.falseColor;
	}

	public void UpdateValidities(Info info) {
		for (int i = 0; i < groups.Length; i++) _validities[i] = groups[i].condition(info);
	}

	public void UpdateDynamicValidities(Info info) {
		foreach (int i in new[] { 0, 4, 6 }) _validities[i] = groups[i].condition(info);
	}

	private void UpdateAllowedConnections() {
		allowedConnections = new HashSet<KeyValuePair<int, int>>();
		for (int i = 0; i < 8; i++) {
			for (int j = i; j < 8; j++) {
				bool hasActiveCells = false;
				for (int y = 0; y < HEIGHT; y++) {
					int activeCellsCount = 0;
					for (int x = i; x <= j; x++) {
						if (powergrid[x][y].active) {
							hasActiveCells = true;
							activeCellsCount += 1;
						}
					}
					if (activeCellsCount > 0 && activeCellsCount % 2 == 0) {
						hasActiveCells = false;
						break;
					}
				}
				if (hasActiveCells) allowedConnections.Add(new KeyValuePair<int, int>(i, j));
			}
		}
	}
}
