using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChargeModule : MonoBehaviour {
	public GameObject ObjectsContainer;
	public KMSelectable Selectable;
	public WireComponent WirePrefab;
	public PowerSourceComponent PowerSourcePrefab;
	public SwitchComponent SwitchPrefab;

	private CircuitState state = new CircuitState();
	private SwitchComponent[] _switches = new SwitchComponent[18];
	private List<WireComponent> wires = new List<WireComponent>();

	private void Start() {
		CreateWire(new Vector3(280, 0, 136), new Vector3(296, 0, 136), true); // battery => grid | statulight || discharge
		CreateWire(new Vector3(312, 0, 144), new Vector3(352, 0, 144), false); // battery => grid | statuslight
		CreateWire(new Vector3(352, 0, 144), new Vector3(352, 0, 152), false); // battery => grid | statuslight
		CreateWire(new Vector3(344, 0, 168), new Vector3(304, 0, 168), false); // battery => grid
		CreateWire(new Vector3(288, 0, 176), new Vector3(256, 0, 176), true); // battery | statuslight => grid
		CreateWire(new Vector3(248, 0, 192), new Vector3(176, 0, 192), true, (state) => state.powergridActive && state.powergridStart < 5); // grid input 1-4
		CreateWire(new Vector3(176, 0, 192), new Vector3(176, 0, 208), true, (state) => state.powergridActive && state.powergridStart < 5); // grid input 1-4
		CreateWire(new Vector3(264, 0, 192), new Vector3(264, 0, 208), false, (state) => state.powergridActive && state.powergridStart > 4); // grid input 5-8
		CreateWire(new Vector3(264, 0, 208), new Vector3(336, 0, 208), false, (state) => state.powergridActive && state.powergridStart > 4); // grid input 5-8
		CreateWire(new Vector3(168, 0, 224), new Vector3(136, 0, 224), false, (state) => state.powergridActive && state.powergridStart < 3); // grid input 1-2
		CreateWire(new Vector3(136, 0, 224), new Vector3(136, 0, 240), false, (state) => state.powergridActive && state.powergridStart < 3); // grid input 1-2
		CreateWire(new Vector3(184, 0, 224), new Vector3(216, 0, 224), true, (state) => ( // grid input 3-4
			state.powergridActive && state.powergridStart < 5 && state.powergridStart > 2
		));
		CreateWire(new Vector3(216, 0, 224), new Vector3(216, 0, 240), true, (state) => ( // grid input 3-4
			state.powergridActive && state.powergridStart < 5 && state.powergridStart > 2
		));
		CreateWire(new Vector3(328, 0, 224), new Vector3(296, 0, 224), false, (state) => ( // grid input 5-6
			state.powergridActive && state.powergridStart < 7 && state.powergridStart > 4
		));
		CreateWire(new Vector3(296, 0, 224), new Vector3(296, 0, 240), false, (state) => ( // grid input 5-6
			state.powergridActive && state.powergridStart < 7 && state.powergridStart > 4
		));
		CreateWire(new Vector3(344, 0, 224), new Vector3(376, 0, 224), false, (state) => state.powergridActive && state.powergridStart > 6); // grid input 7-8
		CreateWire(new Vector3(376, 0, 224), new Vector3(376, 0, 240), false, (state) => state.powergridActive && state.powergridStart > 6); // grid input 7-8
		for (int i = 0; i < 8; i++) {
			int iLock = i + 1;

			// grid input {i}
			float xOffset = 104 + i * 40;
			bool activeInput = i == 3;
			System.Func<CircuitState, bool> onInputChange = (state) => state.powergridStart == iLock && state.powergridEnd >= iLock;
			if (i % 2 == 1) CreateWire(new Vector3(xOffset, 0, 256), new Vector3(xOffset + 16, 0, 256), activeInput, onInputChange);
			else CreateWire(new Vector3(xOffset, 0, 256), new Vector3(xOffset + 24, 0, 256), activeInput, onInputChange);
			if (i == 0) {
				CreateWire(new Vector3(xOffset, 0, 256), new Vector3(xOffset, 0, 360), activeInput, onInputChange);
				CreateWire(new Vector3(xOffset, 0, 280), new Vector3(xOffset + 8, 0, 280), activeInput, onInputChange);
				CreateWire(new Vector3(xOffset, 0, 320), new Vector3(xOffset + 8, 0, 320), activeInput, onInputChange);
				CreateWire(new Vector3(xOffset, 0, 360), new Vector3(xOffset + 8, 0, 360), activeInput, onInputChange);
			} else {
				CreateWire(new Vector3(xOffset, 0, 256), new Vector3(xOffset, 0, 344), activeInput, onInputChange);
				CreateWire(new Vector3(xOffset + 16, 0, 256), new Vector3(xOffset + 16, 0, 272), activeInput, onInputChange);
				CreateWire(new Vector3(xOffset, 0, 344), new Vector3(xOffset + 16, 0, 344), activeInput, onInputChange);
				CreateWire(new Vector3(xOffset + 16, 0, 344), new Vector3(xOffset + 16, 0, 352), activeInput, onInputChange);
				CreateWire(new Vector3(xOffset, 0, 304), new Vector3(xOffset + 16, 0, 304), activeInput, onInputChange);
				CreateWire(new Vector3(xOffset + 16, 0, 304), new Vector3(xOffset + 16, 0, 312), activeInput, onInputChange);
			}

			// grid output {i}
			bool activeOutput = i == 4;
			System.Func<CircuitState, bool> onOutputChanged = (state) => state.powergridStart <= iLock && state.powergridEnd == iLock;
			if (i % 2 == 0) CreateWire(new Vector3(xOffset + 16, 0, 376), new Vector3(xOffset + 32, 0, 376), activeOutput, onOutputChanged);
			else CreateWire(new Vector3(xOffset + 32, 0, 376), new Vector3(xOffset + 8, 0, 376), activeOutput, onOutputChanged);
			if (i < 7) {
				CreateWire(new Vector3(xOffset + 16, 0, 288), new Vector3(xOffset + 16, 0, 296), activeOutput, onOutputChanged);
				CreateWire(new Vector3(xOffset + 16, 0, 296), new Vector3(xOffset + 32, 0, 296), activeOutput, onOutputChanged);
				CreateWire(new Vector3(xOffset + 32, 0, 296), new Vector3(xOffset + 32, 0, 376), activeOutput, onOutputChanged);
				CreateWire(new Vector3(xOffset + 16, 0, 328), new Vector3(xOffset + 16, 0, 336), activeOutput, onOutputChanged);
				CreateWire(new Vector3(xOffset + 16, 0, 336), new Vector3(xOffset + 32, 0, 336), activeOutput, onOutputChanged);
				CreateWire(new Vector3(xOffset + 16, 0, 368), new Vector3(xOffset + 16, 0, 376), activeOutput, onOutputChanged);
			} else {
				CreateWire(new Vector3(xOffset + 32, 0, 280), new Vector3(xOffset + 32, 0, 376), activeOutput, onOutputChanged);
				CreateWire(new Vector3(xOffset + 24, 0, 280), new Vector3(xOffset + 32, 0, 280), activeOutput, onOutputChanged);
				CreateWire(new Vector3(xOffset + 24, 0, 320), new Vector3(xOffset + 32, 0, 320), activeOutput, onOutputChanged);
				CreateWire(new Vector3(xOffset + 24, 0, 360), new Vector3(xOffset + 32, 0, 360), activeOutput, onOutputChanged);
			}
			if (i < 7) {

				// grid {i}-{i+1}
				bool activeWay = i == 3;
				System.Func<CircuitState, bool> onWayChanged = (state) => iLock >= state.powergridStart && state.powergridEnd > iLock;
				CreateWire(new Vector3(xOffset + 24, 0, 280), new Vector3(xOffset + 48, 0, 280), activeWay, onWayChanged);
				CreateWire(new Vector3(xOffset + 24, 0, 320), new Vector3(xOffset + 48, 0, 320), activeWay, onWayChanged);
				CreateWire(new Vector3(xOffset + 24, 0, 360), new Vector3(xOffset + 48, 0, 360), activeWay, onWayChanged);

			}

			for (int j = 0; j < 3; j++) CreatePowerSource(new Vector3(120 + i * 40, 0, 280 + j * 40), i == 3 || i == 4);
		}
		CreateWire(new Vector3(144, 0, 396), new Vector3(144, 0, 408), false, (state) => state.powergridActive && state.powergridEnd < 3); // grid output 1-2
		CreateWire(new Vector3(144, 0, 408), new Vector3(176, 0, 408), false, (state) => state.powergridActive && state.powergridEnd < 3); // grid output 1-2
		CreateWire(new Vector3(224, 0, 392), new Vector3(224, 0, 408), false, (state) => ( // grid output 3-4
			state.powergridActive && state.powergridEnd > 2 && state.powergridEnd < 5
		));
		CreateWire(new Vector3(224, 0, 408), new Vector3(192, 0, 408), false, (state) => ( // grid output 3-4
			state.powergridActive && state.powergridEnd > 2 && state.powergridEnd < 5
		));
		CreateWire(new Vector3(304, 0, 396), new Vector3(304, 0, 408), true, (state) => ( // grid output 5-6
			state.powergridActive && state.powergridEnd > 4 && state.powergridEnd < 7
		));
		CreateWire(new Vector3(304, 0, 408), new Vector3(336, 0, 408), true, (state) => ( // grid output 5-6
			state.powergridActive && state.powergridEnd > 4 && state.powergridEnd < 7
		));
		CreateWire(new Vector3(384, 0, 396), new Vector3(384, 0, 408), false, (state) => state.powergridActive && state.powergridEnd > 6); // grid output 7-8
		CreateWire(new Vector3(384, 0, 408), new Vector3(352, 0, 408), false, (state) => state.powergridActive && state.powergridEnd > 6); // grid output 7-8
		CreateWire(new Vector3(184, 0, 424), new Vector3(184, 0, 440), false, (state) => state.powergridActive && state.powergridEnd < 5); // grid output 1-4
		CreateWire(new Vector3(184, 0, 440), new Vector3(256, 0, 440), false, (state) => state.powergridActive && state.powergridEnd < 5); // grid output 1-4
		CreateWire(new Vector3(344, 0, 424), new Vector3(344, 0, 440), true, (state) => state.powergridActive && state.powergridEnd > 4); // grid output 5-8
		CreateWire(new Vector3(344, 0, 440), new Vector3(272, 0, 440), true, (state) => state.powergridActive && state.powergridEnd > 4); // grid output 5-8
		CreateWire(new Vector3(264, 0, 456), new Vector3(80, 0, 456), true); // grid => battery | statuslight
		CreateWire(new Vector3(80, 0, 456), new Vector3(80, 0, 136), true); // grid => battery | statuslight
		CreateWire(new Vector3(80, 0, 136), new Vector3(104, 0, 136), false); // grid => battery
		CreateWire(new Vector3(104, 0, 136), new Vector3(120, 0, 136), true); // grid => battery || discharge

		_switches[0] = CreateSwitch(new Vector3(296, 0, 136), 0, false, (s) => { }); // switch 1
		_switches[1] = CreateSwitch(new Vector3(352, 0, 152), 90, false, (s) => { }); // switch 2
		_switches[2] = CreateSwitch(new Vector3(288, 0, 176), 0, false, (s) => { }); // switch 3
		_switches[3] = CreateSwitch(new Vector3(400, 0, 176), 180, true, (s) => { }); // switch 4
		_switches[4] = CreateSwitch(new Vector3(256, 0, 176), 90, false, (s) => { // switch input 1-8
			if (s.state) {
				if (_switches[6].state) state.powergridStart = _switches[14].state ? 8 : 7;
				else state.powergridStart = _switches[13].state ? 6 : 5;
			} else {
				if (_switches[5].state) state.powergridStart = _switches[8].state ? 4 : 3;
				else state.powergridStart = _switches[7].state ? 2 : 1;
			}
		});
		_switches[5] = CreateSwitch(new Vector3(176, 0, 208), 90, true, (s) => { // switch input 1-4
			if (state.powergridStart > 4) return;
			if (s.state) state.powergridStart = _switches[8].state ? 4 : 3;
			else state.powergridStart = _switches[7].state ? 2 : 1;
		});
		_switches[6] = CreateSwitch(new Vector3(336, 0, 208), 90, Random.Range(0, 2) == 0, (s) => { // switch input 5-8
			if (state.powergridStart < 5) return;
			if (s.state) state.powergridStart = _switches[10].state ? 8 : 7;
			else state.powergridStart = _switches[9].state ? 6 : 5;
		});
		_switches[7] = CreateSwitch(new Vector3(136, 0, 240), 90, Random.Range(0, 2) == 0, (s) => { // switch input 1-2
			if (state.powergridStart > 2) return;
			state.powergridStart = s.state ? 2 : 1;
		});
		_switches[8] = CreateSwitch(new Vector3(216, 0, 240), 90, true, (s) => { // switch input 3-4
			if (state.powergridStart > 4 || state.powergridStart < 3) return;
			state.powergridStart = s.state ? 4 : 3;
		});
		_switches[9] = CreateSwitch(new Vector3(296, 0, 240), 90, Random.Range(0, 2) == 0, (s) => { // switch input 5-6
			if (state.powergridStart > 6 || state.powergridStart < 5) return;
			state.powergridStart = s.state ? 6 : 5;
		});
		_switches[10] = CreateSwitch(new Vector3(376, 0, 240), 90, Random.Range(0, 2) == 0, (s) => { // switch input 7-8
			if (state.powergridStart < 7) return;
			state.powergridStart = s.state ? 8 : 7;
		});
		_switches[11] = CreateSwitch(new Vector3(144, 0, 392), 270, Random.Range(0, 2) == 0, (s) => { // switch output 1-2
			if (state.powergridEnd > 2) return;
			state.powergridEnd = s.state ? 1 : 2;
		});
		_switches[12] = CreateSwitch(new Vector3(224, 0, 392), 270, Random.Range(0, 2) == 0, (s) => { // switch output 3-4
			if (state.powergridEnd < 3 || state.powergridEnd > 4) return;
			state.powergridEnd = s.state ? 3 : 4;
		});
		_switches[13] = CreateSwitch(new Vector3(304, 0, 392), 270, true, (s) => { // switch output 5-6
			if (state.powergridEnd < 5 || state.powergridEnd > 6) return;
			state.powergridEnd = s.state ? 5 : 6;
		});
		_switches[14] = CreateSwitch(new Vector3(384, 0, 392), 270, Random.Range(0, 2) == 0, (s) => { // switch output 7-8
			if (state.powergridEnd < 7) return;
			state.powergridEnd = s.state ? 7 : 8;
		});
		_switches[15] = CreateSwitch(new Vector3(184, 0, 424), 270, Random.Range(0, 2) == 0, (s) => { // switch output 1-4
			if (state.powergridEnd > 4) return;
			if (s.state) state.powergridEnd = _switches[11].state ? 1 : 2;
			else state.powergridEnd = _switches[12].state ? 3 : 4;
		});
		_switches[16] = CreateSwitch(new Vector3(344, 0, 424), 270, true, (s) => { // switch output 5-8
			if (state.powergridEnd < 5) return;
			if (s.state) state.powergridEnd = _switches[13].state ? 5 : 6;
			else state.powergridEnd = _switches[14].state ? 7 : 8;
		});
		_switches[17] = CreateSwitch(new Vector3(264, 0, 456), 270, false, (s) => { // switch output 1-8
			if (s.state) {
				if (_switches[15].state) state.powergridEnd = _switches[11].state ? 1 : 2;
				else state.powergridEnd = _switches[12].state ? 3 : 4;
			} else {
				if (_switches[16].state) state.powergridEnd = _switches[13].state ? 5 : 6;
				else state.powergridEnd = _switches[14].state ? 7 : 8;
			}
		});

		Selectable.Children = _switches.Select((s) => s.Selectable).ToArray();
		Selectable.UpdateChildren();
	}

	private WireComponent CreateWire(Vector3 from, Vector3 to, bool defaultActive, System.Func<CircuitState, bool> onChange = null) {
		WireComponent result = Instantiate(WirePrefab);
		result.transform.parent = ObjectsContainer.transform;
		from.z = 512 - from.z;
		to.z = 512 - to.z;
		result.position = new KeyValuePair<Vector3, Vector3>(from, to);
		result.active = defaultActive;
		result.OnChange = onChange;
		wires.Add(result);
		return result;
	}

	private PowerSourceComponent CreatePowerSource(Vector3 pos, bool active = false) {
		PowerSourceComponent result = Instantiate(PowerSourcePrefab);
		result.transform.parent = ObjectsContainer.transform;
		pos.z = 512 - pos.z;
		result.transform.localPosition = pos;
		result.transform.localScale = Vector3.one;
		result.transform.localRotation = Quaternion.identity;
		result.color = new KeyValuePair<Color, Color>(Random.ColorHSV(), Random.ColorHSV());
		result.active = active;
		return result;
	}

	private SwitchComponent CreateSwitch(Vector3 pos, float rotation, bool defaultState, System.Action<SwitchComponent> onChanged) {
		SwitchComponent result = Instantiate(SwitchPrefab);
		result.transform.parent = ObjectsContainer.transform;
		pos.z = 512 - pos.z;
		result.transform.localPosition = pos;
		result.transform.localScale = Vector3.one;
		result.transform.localRotation = Quaternion.Euler(0f, rotation, 0f);
		result.state = defaultState;
		result.Selectable.Parent = Selectable;
		result.Selectable.OnInteract = () => {
			result.state = !result.state;
			onChanged(result);
			foreach (WireComponent wire in wires) wire.UpdateActivity(state);
			return false;
		};
		return result;
	}
}
