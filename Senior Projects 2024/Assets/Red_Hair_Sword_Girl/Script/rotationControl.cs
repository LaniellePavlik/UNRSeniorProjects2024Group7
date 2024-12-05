using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotationControl : MonoBehaviour {

	public Animator anim;

	public float sensitivity = 700.0f;
	public float rotationY = 0;

	int Jugement = 0;

	void Update () {

		if (Input.GetKeyDown ("a")) {

			Jugement = 1;
		}


		if (Input.GetKeyUp ("a")) {

			Jugement = 0;
		}

		if (Jugement == 1) {

			rotationY -= sensitivity * -0.2f * Time.deltaTime;

			transform.eulerAngles = new Vector3 (0.0f, rotationY, 0.0f);

		}


		if (Input.GetKeyDown ("d")) {

			Jugement = 2;
		}


		if (Input.GetKeyUp ("d")) {

			Jugement = 0;
		}

		if (Jugement == 2) {

			rotationY -= sensitivity * 0.2f * Time.deltaTime;

			transform.eulerAngles = new Vector3 (0.0f, rotationY, 0.0f);

		}
	}
}
