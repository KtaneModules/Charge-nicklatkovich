using UnityEngine;

public class BatteryComponent : MonoBehaviour {
	public Renderer[] Borders;
	public TextMesh ChargeTextMesh;

	private bool _active; public bool active { get { return _active; } set { if (_active == value) return; _active = value; UpdateColor(); } }
	private int _charge; public int charge { get { return _charge; } set { if (_charge == value) return; _charge = value; UpdateText(); } }
	private int _max; public int max { get { return _max; } set { if (_max == value) return; _max = value; UpdateText(); } }

	private void Start() {
		UpdateText();
		UpdateColor();
	}

	private void UpdateText() {
		ChargeTextMesh.text = charge.ToString().PadLeft(4, ' ') + '/' + max.ToString().PadRight(4, ' ');
	}

	private void UpdateColor() {
		foreach (Renderer renderer in Borders) renderer.material.color = active ? Color.green : (Color)(new Color32(0x88, 0x88, 0x88, 0xff));
	}
}
