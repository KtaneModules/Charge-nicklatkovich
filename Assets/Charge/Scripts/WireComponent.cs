using System.Collections.Generic;
using UnityEngine;

public class WireComponent : MonoBehaviour {
	public const float WIDTH = 2;

	public Renderer Renderer;

	private Vector3 _from; public Vector3 from { get { return _from; } set { if (_from == value) return; _from = value; Transform(); } }
	private Vector3 _to; public Vector3 to { get { return _to; } set { if (_to == value) return; _to = value; Transform(); } }
	public KeyValuePair<Vector3, Vector3> position { set { if (_from == value.Key && _to == value.Value) return; _from = value.Key; _to = value.Value; Transform(); } }
	private bool _active; public bool active { get { return _active; } set { if (_active == value) return; _active = value; UpdateColor(); } }

	private void Start() {
		UpdateColor();
	}

	private void UpdateColor() {
		Renderer.material.color = active ? Color.green : (Color)(new Color32(0x88, 0x88, 0x88, 0xff));
		Vector3 pos = transform.localPosition;
		if (active) pos.y = 1;
		transform.localPosition = pos;
	}

	private void Transform() {
		transform.localPosition = (from + to) / 2;
		transform.localScale = new Vector3(WIDTH, WIDTH, (from - to).magnitude + WIDTH);
		transform.localRotation = Quaternion.LookRotation(to - from, Vector3.up);
	}
}
