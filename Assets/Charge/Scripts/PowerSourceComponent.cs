using System.Collections.Generic;
using UnityEngine;

public class PowerSourceComponent : MonoBehaviour {
	public Renderer BorderRenderer;
	public Renderer LEDRenderer;

	private KeyValuePair<Color, Color> _color;
	public KeyValuePair<Color, Color> color {
		get { return _color; }
		set {
			if (_color.Key == value.Key && _color.Value == value.Value) return;
			_color = value;
			if (firstFlash ? (_color.Key == value.Key) : (_color.Value == value.Value)) return;
			nextUpdateTime = 0;
			firstFlash = Random.Range(0, 2) == 0;
		}
	}

	private bool _active; public bool active { get { return _active; } set { if (_active == value) return; _active = value; UpdateBorderColor(); } }

	private bool firstFlash;
	private float nextUpdateTime;

	private void Start() {
		UpdateBorderColor();
	}

	private void Update() {
		if (Time.time < nextUpdateTime) return;
		nextUpdateTime = Time.time + Random.Range(1f, 2f);
		firstFlash = !firstFlash;
		LEDRenderer.material.color = firstFlash ? color.Key : color.Value;
	}

	private void UpdateBorderColor() {
		BorderRenderer.material.color = active ? Color.green : (Color)(new Color32(0x88, 0x88, 0x88, 0xff));
	}
}
