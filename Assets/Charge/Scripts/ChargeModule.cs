using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class ChargeModule : MonoBehaviour {
	private static int moduleIdCounter = 1;

	public GameObject ObjectsContainer;
	public GameObject StatusLightParent;
	public Material StatusLightLitMaterial;
	public AudioClip[] SwitchSounds;
	public KMSelectable Selectable;
	public KMBombModule Module;
	public KMBombInfo BombInfo;
	public KMAudio Audio;
	public WireComponent WirePrefab;
	public PowerSourceComponent PowerSourcePrefab;
	public SwitchComponent SwitchPrefab;
	public ResistorComponent ResistorPrefab;
	public BatteryComponent Battery;
	public LockComponent Lock;

	public readonly string TwitchHelpMessage = new string[] {
		"\"!{0} 14;3;2;4\" - press switches",
		"See manual to get the id of each switch",
		"If the locking is enabled, the module will wait for its end before using the next switch",
		"This command is cancelable",
	}.Join(". ");

	public bool TwitchShouldCancelCommand;

	private bool lightScaleSet = false;
	private bool solved = false;
	private bool activated = false;
	private float lockingStartsAt;
	private int chargeFrom;
	private int chargeTo;
	private int moduleId;
	private int startingTimeInMinutes;
	private Material defaultStatusLightMaterial;
	private CircuitState state = new CircuitState();
	private ChargePuzzle puzzle;
	private SwitchComponent[] _switches = new SwitchComponent[18];
	private PowerSourceComponent[][] _powerSources = new PowerSourceComponent[ChargePuzzle.WIDTH][];
	private List<WireComponent> wires = new List<WireComponent>();
	private List<ResistorComponent> resistors = new List<ResistorComponent>();
	private Light StatusLightSource;
	private Renderer StatusLightOffRenderer;
	private HashSet<KeyValuePair<int, int>> usedConnections = new HashSet<KeyValuePair<int, int>>();

	private void Start() {
		moduleId = moduleIdCounter++;
		puzzle = new ChargePuzzle();
		Debug.LogFormat("[Charge #{0}] Power sources:", moduleId);
		for (int j = 0; j < ChargePuzzle.HEIGHT; j++) {
			Debug.LogFormat("[Charge #{0}] Row {1}: [{2}]", moduleId, j + 1, puzzle.powergrid.Select((column) => column[j]).Select((cell) => (
				(cell.active ? ChargePuzzle.groups[cell.groupIndex].power : 0).ToString() + " W"
			)).Join(", "));
		}
		Debug.LogFormat("[Charge #{0}] Required energy level: {1} J", moduleId, puzzle.requiredChargeLevel);
		Debug.LogFormat("[Charge #{0}] Solution example: [{1}]", moduleId, puzzle.solutionExample.Select((conn) => (
			string.Format("{0}-{1}", conn.Key + 1, conn.Value + 1)
		)).Join("; "));
		for (int i = 0; i < ChargePuzzle.WIDTH; i++) _powerSources[i] = new PowerSourceComponent[ChargePuzzle.HEIGHT];
		StatusLightSource = StatusLightParent.transform.GetChild(0).GetComponent<Light>();
		Transform statusLightCollection = Enumerable.Range(0, StatusLightParent.transform.childCount).Select((i) => StatusLightParent.transform.GetChild(i)).First((c) => (
			c.name.ToLower().StartsWith("statuslight")
		));
		StatusLightOffRenderer = statusLightCollection.GetChild(Application.isEditor ? 2 : 0).GetComponent<Renderer>();
		StatusLightSource.transform.parent = StatusLightOffRenderer.transform;
		defaultStatusLightMaterial = StatusLightOffRenderer.material;
		StatusLightOffRenderer.material = StatusLightLitMaterial;
		CreateWire(new Vector3(280, 0, 136), new Vector3(296, 0, 136), (state) => { // battery => grid | statulight || discharge
			if (state.discharge && Battery.charge > 0) return true;
			if (state.crossState == CircuitState.CrossState.CHARGE && state.powergridActive) return true;
			return state.crossState == CircuitState.CrossState.SUBMIT;
		});
		CreateWire(new Vector3(312, 0, 144), new Vector3(352, 0, 144), (state) => ( // battery => grid | statuslight
			(state.crossState == CircuitState.CrossState.CHARGE && state.powergridActive) || state.crossState == CircuitState.CrossState.SUBMIT
		));
		CreateWire(new Vector3(352, 0, 144), new Vector3(352, 0, 152), (state) => ( // battery => grid | statuslight
			(state.crossState == CircuitState.CrossState.CHARGE && state.powergridActive) || state.crossState == CircuitState.CrossState.SUBMIT
		));
		CreateWire(new Vector3(344, 0, 168), new Vector3(304, 0, 168), (state) => ( // battery => grid
			state.crossState == CircuitState.CrossState.CHARGE && state.powergridActive
		));
		CreateWire(new Vector3(288, 0, 176), new Vector3(256, 0, 176), (state) => state.powergridActive); // battery | statuslight => grid
		CreateWire(new Vector3(248, 0, 192), new Vector3(176, 0, 192), (state) => state.powergridActive && state.powergridStart < 5); // grid input 1-4
		CreateWire(new Vector3(176, 0, 192), new Vector3(176, 0, 208), (state) => state.powergridActive && state.powergridStart < 5); // grid input 1-4
		CreateWire(new Vector3(264, 0, 192), new Vector3(264, 0, 208), (state) => state.powergridActive && state.powergridStart > 4); // grid input 5-8
		CreateWire(new Vector3(264, 0, 208), new Vector3(336, 0, 208), (state) => state.powergridActive && state.powergridStart > 4); // grid input 5-8
		CreateWire(new Vector3(168, 0, 224), new Vector3(136, 0, 224), (state) => state.powergridActive && state.powergridStart < 3); // grid input 1-2
		CreateWire(new Vector3(136, 0, 224), new Vector3(136, 0, 240), (state) => state.powergridActive && state.powergridStart < 3); // grid input 1-2
		CreateWire(new Vector3(184, 0, 224), new Vector3(216, 0, 224), (state) => ( // grid input 3-4
			state.powergridActive && state.powergridStart < 5 && state.powergridStart > 2
		));
		CreateWire(new Vector3(216, 0, 224), new Vector3(216, 0, 240), (state) => ( // grid input 3-4
			state.powergridActive && state.powergridStart < 5 && state.powergridStart > 2
		));
		CreateWire(new Vector3(328, 0, 224), new Vector3(296, 0, 224), (state) => ( // grid input 5-6
			state.powergridActive && state.powergridStart < 7 && state.powergridStart > 4
		));
		CreateWire(new Vector3(296, 0, 224), new Vector3(296, 0, 240), (state) => ( // grid input 5-6
			state.powergridActive && state.powergridStart < 7 && state.powergridStart > 4
		));
		CreateWire(new Vector3(344, 0, 224), new Vector3(376, 0, 224), (state) => state.powergridActive && state.powergridStart > 6); // grid input 7-8
		CreateWire(new Vector3(376, 0, 224), new Vector3(376, 0, 240), (state) => state.powergridActive && state.powergridStart > 6); // grid input 7-8
		for (int i = 0; i < 8; i++) {
			int iLock = i + 1;

			// grid input {i}
			float xOffset = 104 + i * 40;
			System.Func<CircuitState, bool> onInputChange = (state) => state.powergridActive && state.powergridStart == iLock && state.powergridEnd >= iLock;
			if (i % 2 == 1) CreateWire(new Vector3(xOffset, 0, 256), new Vector3(xOffset + 16, 0, 256), onInputChange);
			else CreateWire(new Vector3(xOffset, 0, 256), new Vector3(xOffset + 24, 0, 256), onInputChange);
			if (i == 0) {
				CreateWire(new Vector3(xOffset, 0, 256), new Vector3(xOffset, 0, 360), onInputChange);
				CreateWire(new Vector3(xOffset, 0, 280), new Vector3(xOffset + 8, 0, 280), onInputChange);
				CreateWire(new Vector3(xOffset, 0, 320), new Vector3(xOffset + 8, 0, 320), onInputChange);
				CreateWire(new Vector3(xOffset, 0, 360), new Vector3(xOffset + 8, 0, 360), onInputChange);
			} else {
				CreateWire(new Vector3(xOffset, 0, 256), new Vector3(xOffset, 0, 344), onInputChange);
				CreateWire(new Vector3(xOffset + 16, 0, 256), new Vector3(xOffset + 16, 0, 272), onInputChange);
				CreateWire(new Vector3(xOffset, 0, 344), new Vector3(xOffset + 16, 0, 344), onInputChange);
				CreateWire(new Vector3(xOffset + 16, 0, 344), new Vector3(xOffset + 16, 0, 352), onInputChange);
				CreateWire(new Vector3(xOffset, 0, 304), new Vector3(xOffset + 16, 0, 304), onInputChange);
				CreateWire(new Vector3(xOffset + 16, 0, 304), new Vector3(xOffset + 16, 0, 312), onInputChange);
			}

			// grid output {i}
			System.Func<CircuitState, bool> onOutputChanged = (state) => state.powergridActive && state.powergridStart <= iLock && state.powergridEnd == iLock;
			if (i % 2 == 0) CreateWire(new Vector3(xOffset + 16, 0, 376), new Vector3(xOffset + 32, 0, 376), onOutputChanged);
			else CreateWire(new Vector3(xOffset + 32, 0, 376), new Vector3(xOffset + 8, 0, 376), onOutputChanged);
			if (i < 7) {
				CreateWire(new Vector3(xOffset + 16, 0, 288), new Vector3(xOffset + 16, 0, 296), onOutputChanged);
				CreateWire(new Vector3(xOffset + 16, 0, 296), new Vector3(xOffset + 32, 0, 296), onOutputChanged);
				CreateWire(new Vector3(xOffset + 32, 0, 296), new Vector3(xOffset + 32, 0, 376), onOutputChanged);
				CreateWire(new Vector3(xOffset + 16, 0, 328), new Vector3(xOffset + 16, 0, 336), onOutputChanged);
				CreateWire(new Vector3(xOffset + 16, 0, 336), new Vector3(xOffset + 32, 0, 336), onOutputChanged);
				CreateWire(new Vector3(xOffset + 16, 0, 368), new Vector3(xOffset + 16, 0, 376), onOutputChanged);
			} else {
				CreateWire(new Vector3(xOffset + 32, 0, 280), new Vector3(xOffset + 32, 0, 376), onOutputChanged);
				CreateWire(new Vector3(xOffset + 24, 0, 280), new Vector3(xOffset + 32, 0, 280), onOutputChanged);
				CreateWire(new Vector3(xOffset + 24, 0, 320), new Vector3(xOffset + 32, 0, 320), onOutputChanged);
				CreateWire(new Vector3(xOffset + 24, 0, 360), new Vector3(xOffset + 32, 0, 360), onOutputChanged);
			}
			if (i < 7) {

				// grid {i}-{i+1}
				System.Func<CircuitState, bool> onWayChanged = (state) => state.powergridActive && iLock >= state.powergridStart && state.powergridEnd > iLock;
				CreateWire(new Vector3(xOffset + 24, 0, 280), new Vector3(xOffset + 48, 0, 280), onWayChanged);
				CreateWire(new Vector3(xOffset + 24, 0, 320), new Vector3(xOffset + 48, 0, 320), onWayChanged);
				CreateWire(new Vector3(xOffset + 24, 0, 360), new Vector3(xOffset + 48, 0, 360), onWayChanged);

			}

			for (int j = 0; j < 3; j++) _powerSources[i][j] = CreatePowerSource(new Vector3(120 + i * 40, 0, 280 + j * 40));
		}
		CreateWire(new Vector3(144, 0, 396), new Vector3(144, 0, 408), (state) => state.powergridActive && state.powergridEnd < 3); // grid output 1-2
		CreateWire(new Vector3(144, 0, 408), new Vector3(176, 0, 408), (state) => state.powergridActive && state.powergridEnd < 3); // grid output 1-2
		CreateWire(new Vector3(224, 0, 392), new Vector3(224, 0, 408), (state) => ( // grid output 3-4
			state.powergridActive && state.powergridEnd > 2 && state.powergridEnd < 5
		));
		CreateWire(new Vector3(224, 0, 408), new Vector3(192, 0, 408), (state) => ( // grid output 3-4
			state.powergridActive && state.powergridEnd > 2 && state.powergridEnd < 5
		));
		CreateWire(new Vector3(304, 0, 396), new Vector3(304, 0, 408), (state) => ( // grid output 5-6
			state.powergridActive && state.powergridEnd > 4 && state.powergridEnd < 7
		));
		CreateWire(new Vector3(304, 0, 408), new Vector3(336, 0, 408), (state) => ( // grid output 5-6
			state.powergridActive && state.powergridEnd > 4 && state.powergridEnd < 7
		));
		CreateWire(new Vector3(384, 0, 396), new Vector3(384, 0, 408), (state) => state.powergridActive && state.powergridEnd > 6); // grid output 7-8
		CreateWire(new Vector3(384, 0, 408), new Vector3(352, 0, 408), (state) => state.powergridActive && state.powergridEnd > 6); // grid output 7-8
		CreateWire(new Vector3(184, 0, 424), new Vector3(184, 0, 440), (state) => state.powergridActive && state.powergridEnd < 5); // grid output 1-4
		CreateWire(new Vector3(184, 0, 440), new Vector3(256, 0, 440), (state) => state.powergridActive && state.powergridEnd < 5); // grid output 1-4
		CreateWire(new Vector3(344, 0, 424), new Vector3(344, 0, 440), (state) => state.powergridActive && state.powergridEnd > 4); // grid output 5-8
		CreateWire(new Vector3(344, 0, 440), new Vector3(272, 0, 440), (state) => state.powergridActive && state.powergridEnd > 4); // grid output 5-8
		CreateWire(new Vector3(264, 0, 456), new Vector3(80, 0, 456), (state) => state.powergridActive); // grid => battery | statuslight
		CreateWire(new Vector3(80, 0, 456), new Vector3(80, 0, 136), (state) => state.powergridActive); // grid => battery | statuslight
		CreateWire(new Vector3(80, 0, 136), new Vector3(104, 0, 136), (state) => ( // grid => battery || battery => statuslight
			(state.powergridActive && state.crossState == CircuitState.CrossState.CHARGE) || state.crossState == CircuitState.CrossState.SUBMIT
		));
		CreateWire(new Vector3(104, 0, 136), new Vector3(120, 0, 136), (state) => { // grid => battery || discharge || battery => statuslight
			if (state.powergridActive && state.crossState == CircuitState.CrossState.CHARGE) return true;
			if (state.discharge && Battery.charge > 0) return true;
			return state.crossState == CircuitState.CrossState.SUBMIT;
		});
		CreateWire(new Vector3(80, 0, 136), new Vector3(80, 0, 56), (state) => ( // grid | battery => statuslight
			state.crossState == CircuitState.CrossState.SUBMIT || (state.crossState == CircuitState.CrossState.STATUS && state.powergridActive)
		));
		CreateWire(new Vector3(80, 0, 56), new Vector3(344, 0, 56), (state) => ( // grid | battery => statuslight
			state.crossState == CircuitState.CrossState.SUBMIT || (state.crossState == CircuitState.CrossState.STATUS && state.powergridActive)
		));
		CreateWire(new Vector3(344, 0, 56), new Vector3(344, 0, 64), (state) => ( // grid | battery => statuslight
			state.crossState == CircuitState.CrossState.SUBMIT || (state.crossState == CircuitState.CrossState.STATUS && state.powergridActive)
		));
		CreateWire(new Vector3(344, 0, 64), new Vector3(448, 0, 64), (state) => ( // grid | battery => statuslight
			state.crossState == CircuitState.CrossState.SUBMIT || (state.crossState == CircuitState.CrossState.STATUS && state.powergridActive)
		));
		CreateWire(new Vector3(304, 0, 184), new Vector3(384, 0, 184), (state) => ( // statuslight => grid
			state.crossState == CircuitState.CrossState.STATUS && state.powergridActive
		));
		CreateWire(new Vector3(360, 0, 168), new Vector3(384, 0, 168), (state) => state.crossState == CircuitState.CrossState.SUBMIT); // battery => statuslight
		CreateWire(new Vector3(400, 0, 176), new Vector3(448, 0, 176), (state) => ( // statuslight => grid | battery
			state.crossState == CircuitState.CrossState.SUBMIT || (state.crossState == CircuitState.CrossState.STATUS && state.powergridActive)
		));
		CreateWire(new Vector3(448, 0, 176), new Vector3(448, 0, 64), (state) => ( // statuslight => grid | battery
			state.crossState == CircuitState.CrossState.SUBMIT || (state.crossState == CircuitState.CrossState.STATUS && state.powergridActive)
		));
		CreateWire(new Vector3(312, 0, 128), new Vector3(312, 0, 72), (state) => state.discharge && Battery.charge > 0); // discharge
		CreateWire(new Vector3(312, 0, 72), new Vector3(136, 0, 72), (state) => state.discharge && Battery.charge > 0); // discharge
		for (int i = 0; i < 9; i++) {
			float x = 136 + i * 16;
			CreateWire(new Vector3(x, 0, 72), new Vector3(x, 0, 104), (state) => state.discharge && Battery.charge > 0); // discharge
			ResistorComponent resistor = Instantiate(ResistorPrefab);
			resistor.transform.parent = ObjectsContainer.transform;
			resistor.transform.localPosition = new Vector3(x, 0, 424);
			resistor.transform.localScale = Vector3.one;
			resistor.transform.localRotation = Quaternion.identity;
			resistor.active = true;
			resistors.Add(resistor);
		}
		CreateWire(new Vector3(264, 0, 104), new Vector3(104, 0, 104), (state) => state.discharge && Battery.charge > 0); // discharge
		CreateWire(new Vector3(104, 0, 104), new Vector3(104, 0, 136), (state) => state.discharge && Battery.charge > 0); // discharge

		_switches[0] = CreateSwitch(new Vector3(296, 0, 136), 0, false);
		_switches[1] = CreateSwitch(new Vector3(352, 0, 152), 90, false);
		_switches[2] = CreateSwitch(new Vector3(288, 0, 176), 0, false);
		_switches[3] = CreateSwitch(new Vector3(400, 0, 176), 180, true);
		_switches[4] = CreateSwitch(new Vector3(256, 0, 176), 90, false); // switch input 1-8
		_switches[5] = CreateSwitch(new Vector3(176, 0, 208), 90, true); // switch input 1-4
		_switches[6] = CreateSwitch(new Vector3(336, 0, 208), 90, Random.Range(0, 2) == 0); // switch input 5-8
		_switches[7] = CreateSwitch(new Vector3(136, 0, 240), 90, Random.Range(0, 2) == 0); // switch input 1-2
		_switches[8] = CreateSwitch(new Vector3(216, 0, 240), 90, true); // switch input 3-4
		_switches[9] = CreateSwitch(new Vector3(296, 0, 240), 90, Random.Range(0, 2) == 0); // switch input 5-6
		_switches[10] = CreateSwitch(new Vector3(376, 0, 240), 90, Random.Range(0, 2) == 0); // switch input 7-8
		_switches[11] = CreateSwitch(new Vector3(144, 0, 392), 270, Random.Range(0, 2) == 0); // switch output 1-2
		_switches[12] = CreateSwitch(new Vector3(224, 0, 392), 270, Random.Range(0, 2) == 0); // switch output 3-4
		_switches[13] = CreateSwitch(new Vector3(304, 0, 392), 270, true); // switch output 5-6
		_switches[14] = CreateSwitch(new Vector3(384, 0, 392), 270, Random.Range(0, 2) == 0); // switch output 7-8
		_switches[15] = CreateSwitch(new Vector3(184, 0, 424), 270, Random.Range(0, 2) == 0); // switch output 1-4
		_switches[16] = CreateSwitch(new Vector3(344, 0, 424), 270, true); // switch output 5-8
		_switches[17] = CreateSwitch(new Vector3(264, 0, 456), 270, false); // switch output 1-8

		Selectable.Children = _switches.Select((s) => s.Selectable).ToArray();
		Selectable.UpdateChildren();
		UpdateSwitches();
		Module.OnActivate += Activate;
	}

	private void Activate() {
		startingTimeInMinutes = Mathf.FloorToInt(BombInfo.GetTime() / 60);
		ChargePuzzle.Info info = new ChargePuzzle.Info(BombInfo, startingTimeInMinutes);
		puzzle.UpdateValidities(info);
		for (int i = 0; i < ChargePuzzle.WIDTH; i++) for (int j = 0; j < ChargePuzzle.HEIGHT; j++) _powerSources[i][j].color = puzzle.GetColor(i, j);
		UpdateSwitches();
		Battery.required = puzzle.requiredChargeLevel;
		Lock.active = false;
		activated = true;
	}

	private void Update() {
		if (!lightScaleSet) {
			Vector3 scale = transform.lossyScale;
			StatusLightSource.range = 0.05f * Mathf.Max(scale.x, scale.y, scale.z);
			lightScaleSet = true;
		}
		ChargePuzzle.Info info = new ChargePuzzle.Info(BombInfo, startingTimeInMinutes);
		puzzle.UpdateDynamicValidities(info);
		for (int i = 0; i < ChargePuzzle.WIDTH; i++) for (int j = 0; j < ChargePuzzle.HEIGHT; j++) _powerSources[i][j].color = puzzle.GetColor(i, j);
		if (solved || !activated) return;
		if (Lock.active) {
			if (Time.time >= lockingStartsAt + 1f) {
				Lock.active = false;
				_switches[state.discharge ? 0 : 2].state = false;
				UpdateSwitches(true);
				Battery.charge = chargeTo;
			} else Battery.charge = Mathf.FloorToInt(chargeFrom + (chargeTo - chargeFrom) * (Time.time - lockingStartsAt));
		}
	}

	public IEnumerator ProcessTwitchCommand(string command) {
		if (solved || !activated) yield break;
		command = command.Trim().ToLower();
		if (!Regex.IsMatch(command, @"^(([1-9]|1[0-8]) *(;|$) *)+$")) yield break;
		command = command.Split(' ').Where((s) => s.Length > 0).Join("");
		if (command.EndsWith(";")) command = command.Take(command.Length - 1).Join("");
		int[] indices = command.Split(';').Select((s) => int.Parse(s)).ToArray();
		yield return null;
		int i = 0;
		while (i < indices.Length) {
			if (TwitchShouldCancelCommand) {
				yield return "cancelled";
				yield break;
			}
			if (!Lock.active) {
				int index = indices[i];
				yield return new[] { _switches[index - 1].Selectable };
				i += 1;
			}
			yield return null;
		}
	}

	private IEnumerator TwitchHandleForcedSolve() {
		yield return null;
		while (Lock.active) yield return new WaitForSeconds(.1f);
		if (Battery.charge > 0) {
			_switches[0].state = !_switches[0].state;
			UpdateSwitches(true);
			while (Lock.active) yield return new WaitForSeconds(.1f);
		}
		foreach (KeyValuePair<int, int> conn in puzzle.solutionExample) {
			List<int> _switchesToPress = new List<int>();
			if (_switches[4].state != conn.Key > 3) _switchesToPress.Add(4);
			int level2switch = 5 + conn.Key / 4;
			if (_switches[level2switch].state != conn.Key % 4 > 1) _switchesToPress.Add(level2switch);
			int level3switch = 7 + conn.Key / 2;
			if (_switches[level3switch].state != conn.Key % 2 > 0) _switchesToPress.Add(level3switch);
			if (_switches[17].state != conn.Value < 4) _switchesToPress.Add(17);
			int level5switch = 15 + conn.Value / 4;
			if (_switches[level5switch].state != conn.Value % 4 < 2) _switchesToPress.Add(level5switch);
			int level4switch = 11 + conn.Value / 2;
			if (_switches[level4switch].state != conn.Value % 2 < 1) _switchesToPress.Add(level4switch);
			if (_switches[1].state) _switchesToPress.Add(1);
			if (!_switches[2].state) _switchesToPress.Add(2);
			foreach (int _switchIndex in _switchesToPress) {
				_switches[_switchIndex].state = !_switches[_switchIndex].state;
				UpdateSwitches(true);
				yield return new WaitForSeconds(.1f);
			}
			while (Lock.active) yield return new WaitForSeconds(.1f);
		}
		if (!_switches[1].state) {
			_switches[1].state = !_switches[1].state;
			UpdateSwitches(true);
			yield return new WaitForSeconds(.1f);
		}
		if (_switches[3].state) {
			_switches[3].state = !_switches[3].state;
			UpdateSwitches(true);
			yield return new WaitForSeconds(.1f);
		}
		while (Lock.active) yield return new WaitForSeconds(.1f);
	}

	private WireComponent CreateWire(Vector3 from, Vector3 to, System.Func<CircuitState, bool> onChange = null) {
		WireComponent result = Instantiate(WirePrefab);
		result.transform.parent = ObjectsContainer.transform;
		from.z = 512 - from.z;
		to.z = 512 - to.z;
		result.position = new KeyValuePair<Vector3, Vector3>(from, to);
		result.active = true;
		result.OnChange = onChange;
		wires.Add(result);
		return result;
	}

	private PowerSourceComponent CreatePowerSource(Vector3 pos) {
		PowerSourceComponent result = Instantiate(PowerSourcePrefab);
		result.transform.parent = ObjectsContainer.transform;
		pos.z = 512 - pos.z;
		result.transform.localPosition = pos;
		result.transform.localScale = Vector3.one;
		result.transform.localRotation = Quaternion.identity;
		result.color = new KeyValuePair<Color, Color>(Random.ColorHSV(), Random.ColorHSV());
		result.active = true;
		return result;
	}

	private SwitchComponent CreateSwitch(Vector3 pos, float rotation, bool defaultState) {
		SwitchComponent result = Instantiate(SwitchPrefab);
		result.transform.parent = ObjectsContainer.transform;
		pos.z = 512 - pos.z;
		result.transform.localPosition = pos;
		result.transform.localScale = Vector3.one;
		result.transform.localRotation = Quaternion.Euler(0f, rotation, 0f);
		result.state = defaultState;
		result.Selectable.Parent = Selectable;
		result.Selectable.OnInteract = () => {
			if (Lock.active) return false;
			result.state = !result.state;
			UpdateSwitches(true);
			return false;
		};
		return result;
	}

	private void UpdateCrossSwitches() {
		state.crossState = CircuitState.CrossState.NONE;
		if (_switches[0].state) state.discharge = true;
		else {
			state.discharge = false;
			if (!_switches[1].state && _switches[2].state) {
				state.crossState = CircuitState.CrossState.CHARGE;
				return;
			}
			if (_switches[1].state && !_switches[3].state) {
				state.crossState = CircuitState.CrossState.SUBMIT;
				return;
			}
		}
		if (_switches[3].state && !_switches[2].state) state.crossState = CircuitState.CrossState.STATUS;
	}

	private void UpdateSwitches(bool playSounds = false) {
		UpdateCrossSwitches();
		if (_switches[4].state) {
			if (_switches[6].state) state.powergridStart = _switches[10].state ? 8 : 7;
			else state.powergridStart = _switches[9].state ? 6 : 5;
		} else {
			if (_switches[5].state) state.powergridStart = _switches[8].state ? 4 : 3;
			else state.powergridStart = _switches[7].state ? 2 : 1;
		}
		if (_switches[17].state) {
			if (_switches[15].state) state.powergridEnd = _switches[11].state ? 1 : 2;
			else state.powergridEnd = _switches[12].state ? 3 : 4;
		} else {
			if (_switches[16].state) state.powergridEnd = _switches[13].state ? 5 : 6;
			else state.powergridEnd = _switches[14].state ? 7 : 8;
		}
		state.UpdateActivity(puzzle);
		if (state.crossState == CircuitState.CrossState.CHARGE) {
			KeyValuePair<int, int> conn = new KeyValuePair<int, int>(state.powergridStart - 1, state.powergridEnd - 1);
			if (usedConnections.Contains(conn) || !puzzle.allowedConnections.Contains(conn)) {
				Debug.LogFormat("[Charge #{0}] Using forbidden connection {1}-{2}. Strike!", moduleId, state.powergridStart, state.powergridEnd);
				_switches[2].state = false;
				UpdateSwitches();
				Module.HandleStrike();
				return;
			}
			usedConnections.Add(conn);
			Lock.active = true;
			chargeFrom = Battery.charge;
			int power = puzzle.GetConnectionPower(conn);
			chargeTo = chargeFrom + power;
			Debug.LogFormat("[Charge #{0}] Using connection {1}-{2} ({3} W). Battery level = {4} J", moduleId, state.powergridStart, state.powergridEnd, power, chargeTo);
			lockingStartsAt = Time.time;
		} else if (state.crossState == CircuitState.CrossState.SUBMIT) {
			if (Battery.charge == Battery.required) {
				Debug.LogFormat("[Charge #{0}] Module solved", moduleId);
				Module.HandlePass();
				solved = true;
				Lock.active = true;
				Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, StatusLightSource.transform);
			} else {
				Debug.LogFormat("[Charge #{0}] Submitted {1} J. Expected {2} J. Strike!", moduleId, Battery.charge, Battery.required);
				_switches[3].state = true;
				UpdateSwitches();
				Module.HandleStrike();
				return;
			}
		} else if (state.discharge) {
			Debug.LogFormat("[Charge #{0}] Battery discharged", moduleId);
			usedConnections = new HashSet<KeyValuePair<int, int>>();
			Lock.active = true;
			chargeFrom = Battery.charge;
			chargeTo = 0;
			lockingStartsAt = Time.time;
		}
		if (playSounds) Audio.PlaySoundAtTransform(SwitchSounds.PickRandom().name, transform);
		foreach (WireComponent wire in wires) wire.UpdateActivity(state);
		Battery.active = state.crossState == CircuitState.CrossState.SUBMIT || state.crossState == CircuitState.CrossState.CHARGE || state.discharge;
		for (int i = 0; i < ChargePuzzle.WIDTH; i++) {
			for (int j = 0; j < ChargePuzzle.HEIGHT; j++) {
				_powerSources[i][j].active = state.powergridActive && i + 1 >= state.powergridStart && i < state.powergridEnd;
			}
		}
		bool light = state.powergridActive && state.crossState == CircuitState.CrossState.STATUS;
		StatusLightOffRenderer.material = light ? StatusLightLitMaterial : defaultStatusLightMaterial;
		StatusLightSource.gameObject.SetActive(light);
		foreach (ResistorComponent resistor in resistors) resistor.active = state.discharge && Battery.charge > 0;
	}
}
