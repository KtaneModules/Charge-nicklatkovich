using System.Collections.Generic;
using UnityEngine;

public class ChargeModule : MonoBehaviour {
	public GameObject ObjectsContainer;
	public WireComponent WirePrefab;
	public PowerSourceComponent PowerSourcePrefab;
	public SwitchComponent SwitchPrefab;

	private void Start() {
		CreateWire(new Vector3(280, 0, 136), new Vector3(296, 0, 136), true); // battery => grid | statulight || discharge
		CreateWire(new Vector3(312, 0, 144), new Vector3(352, 0, 144), false); // battery => grid | statuslight
		CreateWire(new Vector3(352, 0, 144), new Vector3(352, 0, 152), false); // battery => grid | statuslight
		CreateWire(new Vector3(344, 0, 168), new Vector3(304, 0, 168), false); // battery => grid
		CreateWire(new Vector3(288, 0, 176), new Vector3(256, 0, 176), true); // battery | statuslight => grid
		CreateWire(new Vector3(248, 0, 192), new Vector3(176, 0, 192), true); // grid input 1-4
		CreateWire(new Vector3(176, 0, 192), new Vector3(176, 0, 208), true); // grid input 1-4
		CreateWire(new Vector3(264, 0, 192), new Vector3(264, 0, 208), false); // grid input 5-8
		CreateWire(new Vector3(264, 0, 208), new Vector3(336, 0, 208), false); // grid input 5-8
		CreateWire(new Vector3(168, 0, 224), new Vector3(136, 0, 224), false); // grid input 1-2
		CreateWire(new Vector3(136, 0, 224), new Vector3(136, 0, 240), false); // grid input 1-2
		CreateWire(new Vector3(184, 0, 224), new Vector3(216, 0, 224), true); // grid input 3-4
		CreateWire(new Vector3(216, 0, 224), new Vector3(216, 0, 240), true); // grid input 3-4
		CreateWire(new Vector3(328, 0, 224), new Vector3(296, 0, 224), false); // grid input 5-6
		CreateWire(new Vector3(296, 0, 224), new Vector3(296, 0, 240), false); // grid input 5-6
		CreateWire(new Vector3(344, 0, 224), new Vector3(376, 0, 224), false); // grid input 7-8
		CreateWire(new Vector3(376, 0, 224), new Vector3(376, 0, 240), false); // grid input 7-8
		for (int i = 0; i < 8; i++) {

			// grid input {i}
			float xOffset = 104 + i * 40;
			bool activeInput = i == 3;
			if (i % 2 == 1) CreateWire(new Vector3(xOffset, 0, 256), new Vector3(xOffset + 16, 0, 256), activeInput);
			else CreateWire(new Vector3(xOffset, 0, 256), new Vector3(xOffset + 24, 0, 256), activeInput);
			if (i == 0) {
				CreateWire(new Vector3(xOffset, 0, 256), new Vector3(xOffset, 0, 360), activeInput);
				CreateWire(new Vector3(xOffset, 0, 280), new Vector3(xOffset + 8, 0, 280), activeInput);
				CreateWire(new Vector3(xOffset, 0, 320), new Vector3(xOffset + 8, 0, 320), activeInput);
				CreateWire(new Vector3(xOffset, 0, 360), new Vector3(xOffset + 8, 0, 360), activeInput);
			} else {
				CreateWire(new Vector3(xOffset, 0, 256), new Vector3(xOffset, 0, 344), activeInput);
				CreateWire(new Vector3(xOffset + 16, 0, 256), new Vector3(xOffset + 16, 0, 272), activeInput);
				CreateWire(new Vector3(xOffset, 0, 344), new Vector3(xOffset + 16, 0, 344), activeInput);
				CreateWire(new Vector3(xOffset + 16, 0, 344), new Vector3(xOffset + 16, 0, 352), activeInput);
				CreateWire(new Vector3(xOffset, 0, 304), new Vector3(xOffset + 16, 0, 304), activeInput);
				CreateWire(new Vector3(xOffset + 16, 0, 304), new Vector3(xOffset + 16, 0, 312), activeInput);
			}

			// grid output {i}
			bool activeOutput = i == 4;
			if (i % 2 == 0) CreateWire(new Vector3(xOffset + 16, 0, 376), new Vector3(xOffset + 32, 0, 376), activeOutput);
			else CreateWire(new Vector3(xOffset + 32, 0, 376), new Vector3(xOffset + 8, 0, 376), activeOutput);
			if (i < 7) {
				CreateWire(new Vector3(xOffset + 16, 0, 288), new Vector3(xOffset + 16, 0, 296), activeOutput);
				CreateWire(new Vector3(xOffset + 16, 0, 296), new Vector3(xOffset + 32, 0, 296), activeOutput);
				CreateWire(new Vector3(xOffset + 32, 0, 296), new Vector3(xOffset + 32, 0, 376), activeOutput);
				CreateWire(new Vector3(xOffset + 16, 0, 328), new Vector3(xOffset + 16, 0, 336), activeOutput);
				CreateWire(new Vector3(xOffset + 16, 0, 336), new Vector3(xOffset + 32, 0, 336), activeOutput);
				CreateWire(new Vector3(xOffset + 16, 0, 368), new Vector3(xOffset + 16, 0, 376), activeOutput);
			} else {
				CreateWire(new Vector3(xOffset + 32, 0, 280), new Vector3(xOffset + 32, 0, 376), activeOutput);
				CreateWire(new Vector3(xOffset + 24, 0, 280), new Vector3(xOffset + 32, 0, 280), activeOutput);
				CreateWire(new Vector3(xOffset + 24, 0, 320), new Vector3(xOffset + 32, 0, 320), activeOutput);
				CreateWire(new Vector3(xOffset + 24, 0, 360), new Vector3(xOffset + 32, 0, 360), activeOutput);
			}
			if (i < 7) {

				// grid {i}-{i+1}
				bool activeWay = i == 3;
				CreateWire(new Vector3(xOffset + 24, 0, 280), new Vector3(xOffset + 48, 0, 280), activeWay);
				CreateWire(new Vector3(xOffset + 24, 0, 320), new Vector3(xOffset + 48, 0, 320), activeWay);
				CreateWire(new Vector3(xOffset + 24, 0, 360), new Vector3(xOffset + 48, 0, 360), activeWay);

			}

			for (int j = 0; j < 3; j++) CreatePowerSource(new Vector3(120 + i * 40, 0, 280 + j * 40), i == 3 || i == 4);
		}
		CreateWire(new Vector3(144, 0, 396), new Vector3(144, 0, 408), false); // grid output 1-2
		CreateWire(new Vector3(144, 0, 408), new Vector3(176, 0, 408), false); // grid output 1-2
		CreateWire(new Vector3(224, 0, 392), new Vector3(224, 0, 408), false); // grid output 3-4
		CreateWire(new Vector3(224, 0, 408), new Vector3(192, 0, 408), false); // grid output 3-4
		CreateWire(new Vector3(304, 0, 396), new Vector3(304, 0, 408), true); // grid output 5-6
		CreateWire(new Vector3(304, 0, 408), new Vector3(336, 0, 408), true); // grid output 5-6
		CreateWire(new Vector3(384, 0, 396), new Vector3(384, 0, 408), false); // grid output 7-8
		CreateWire(new Vector3(384, 0, 408), new Vector3(352, 0, 408), false); // grid output 7-8
		CreateWire(new Vector3(184, 0, 424), new Vector3(184, 0, 440), false); // grid output 1-4
		CreateWire(new Vector3(184, 0, 440), new Vector3(256, 0, 440), false); // grid output 1-4
		CreateWire(new Vector3(344, 0, 424), new Vector3(344, 0, 440), true); // grid output 5-8
		CreateWire(new Vector3(344, 0, 440), new Vector3(272, 0, 440), true); // grid output 5-8
		CreateWire(new Vector3(264, 0, 456), new Vector3(80, 0, 456), true); // grid => battery | statuslight
		CreateWire(new Vector3(80, 0, 456), new Vector3(80, 0, 136), true); // grid => battery | statuslight
		CreateWire(new Vector3(80, 0, 136), new Vector3(104, 0, 136), false); // grid => battery
		CreateWire(new Vector3(104, 0, 136), new Vector3(120, 0, 136), true); // grid => battery || discharge

		CreateSwitch(new Vector3(296, 0, 136), 0, false); // switch 1
		CreateSwitch(new Vector3(352, 0, 152), 90, false); // switch 2
		CreateSwitch(new Vector3(288, 0, 176), 0, false); // switch 3
		CreateSwitch(new Vector3(400, 0, 176), 180, true); // switch 4
		CreateSwitch(new Vector3(256, 0, 176), 90, false); // switch input 1-8
		CreateSwitch(new Vector3(176, 0, 208), 90, true); // switch input 1-4
		CreateSwitch(new Vector3(336, 0, 208), 90, Random.Range(0, 2) == 0); // switch input 5-8
		CreateSwitch(new Vector3(136, 0, 240), 90, Random.Range(0, 2) == 0); // switch input 1-2
		CreateSwitch(new Vector3(216, 0, 240), 90 , true); // switch input 3-4
		CreateSwitch(new Vector3(296, 0, 240), 90, Random.Range(0, 2) == 0); // switch input 5-6
		CreateSwitch(new Vector3(376, 0, 240), 90, Random.Range(0, 2) == 0); // switch input 7-8
		CreateSwitch(new Vector3(144, 0, 392), 270, Random.Range(0, 2) == 0); // switch output 1-2
		CreateSwitch(new Vector3(224, 0, 392), 270, Random.Range(0, 2) == 0); // switch output 3-4
		CreateSwitch(new Vector3(304, 0, 392), 270, true); // switch output 5-6
		CreateSwitch(new Vector3(384, 0, 392), 270, Random.Range(0, 2) == 0); // switch output 7-8
		CreateSwitch(new Vector3(184, 0, 424), 270, Random.Range(0, 2) == 0); // switch output 1-4
		CreateSwitch(new Vector3(344, 0, 424), 270, true); // switch output 5-8
		CreateSwitch(new Vector3(264, 0, 456), 270, false); // switch output 1-8
	}

	private WireComponent CreateWire(Vector3 from, Vector3 to, bool active) {
		WireComponent result = Instantiate(WirePrefab);
		result.transform.parent = ObjectsContainer.transform;
		from.z = 512 - from.z;
		to.z = 512 - to.z;
		result.position = new KeyValuePair<Vector3, Vector3>(from, to);
		result.active = active;
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

	private SwitchComponent CreateSwitch(Vector3 pos, float rotation, bool defaultState) {
		SwitchComponent result = Instantiate(SwitchPrefab);
		result.transform.parent = ObjectsContainer.transform;
		pos.z = 512 - pos.z;
		result.transform.localPosition = pos;
		result.transform.localScale = Vector3.one;
		result.transform.localRotation = Quaternion.Euler(0f, rotation, 0f);
		result.state = defaultState;
		return result;
	}
}
