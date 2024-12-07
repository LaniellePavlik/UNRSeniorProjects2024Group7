using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cameraControl : MonoBehaviour {



	void Update () {

		if (Input.GetKeyDown ("q")) {

			transform.position = new Vector3 (0.0f, 3.4f, 14.0f);

		}

		if (Input.GetKeyDown ("r")) {

			transform.position = new Vector3 (0.0f, 1.54f, 7.21f);

		}

		if (Input.GetKeyDown ("e")) {

			transform.position = new Vector3 (0.0f, 2.3f, 10.0f);

		}
	}
}
