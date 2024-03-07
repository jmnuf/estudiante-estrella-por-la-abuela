using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObject : MonoBehaviour {
	private PlayerInteractor interactor;
	private FirstPersonCameraController camera_controller;

	void Awake() {
		interactor = GetComponent<PlayerInteractor>();
		camera_controller = GetComponent<FirstPersonCameraController>();
	}

	void FixedUpdate() {
		FPSCurrentRotation cur_rotation = camera_controller.get_current_rotation();
		camera_controller.move_camera(cur_rotation);
		interactor.attempt_to_interact(cur_rotation);
	}
}
