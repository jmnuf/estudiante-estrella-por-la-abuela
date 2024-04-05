using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerObject : MonoBehaviour {
	public PlayerInteractor interactor { get; private set; }
	public FirstPersonCameraController camera_controller { get; private set; }

	void Awake() {
		interactor = GetComponent<PlayerInteractor>();
		camera_controller = GetComponent<FirstPersonCameraController>();
	}

	void FixedUpdate() {
		interactor.attempt_to_interact(null);
	}

	void LateUpdate() {
		FPSCurrentRotation cur_rotation = camera_controller.get_current_rotation();
		camera_controller.move_objects(cur_rotation);
	}
}
