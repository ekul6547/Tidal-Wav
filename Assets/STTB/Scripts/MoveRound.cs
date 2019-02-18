using UnityEngine;
using System.Collections;

public class MoveRound : MonoBehaviour {

	private float add = 0.01f;

	// Update is called once per frame
	void Update () {
		var rot = transform.rotation;
		rot.z += add;
		Debug.Log (rot.z);
		if (rot.z >= 1f || rot.z <= -1f) {
			add = add * -1f;
		}
		transform.rotation = rot;
	}
}
