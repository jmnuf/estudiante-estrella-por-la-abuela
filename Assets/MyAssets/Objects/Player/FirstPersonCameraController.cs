using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirstPersonCameraController : MonoBehaviour {
	private static FirstPersonCameraController instance;

	private Vector2 rotation;

	public GameObject scene_camera;
	public Transform target_point;

	[SerializeField]
	[Range(0.05f, 5f)]
	private float mouse_sensitivity = 2.0f;
	[SerializeField]
	[Range(45f, 100f)]
	private float y_rotation_limit = 80f;
	[SerializeField]
	[Range(45f, 180f)]
	private float x_rotation_limit = 90f;
	
	const string xAxis = "Mouse X";
	const string yAxis = "Mouse Y";

	void Awake() {
		instance = this;
		Cursor.lockState = CursorLockMode.Locked;
	}

	public void move_camera(FPSCurrentRotation cur_rotation) {
		scene_camera.transform.position = target_point.position;

		scene_camera.transform.localRotation = cur_rotation.get_only_y_rotation();
		transform.localRotation = cur_rotation.get_only_x_rotation();
	}

	void toggle_lock() {
		if (Cursor.lockState == CursorLockMode.None) {
			Cursor.lockState = CursorLockMode.Locked;
		} else {
			Cursor.lockState = CursorLockMode.None;
		}
	}

	public void set_mouse_sensitivity(float new_sensitivity) {
		mouse_sensitivity = Mathf.Clamp(new_sensitivity, 0.05f, 5f);
	}
	public float get_mouse_sensitivity() {
		return mouse_sensitivity;
	}

	public FPSCurrentRotation get_current_rotation() {
		Vector2 axes_movement = new Vector2(
				Input.GetAxis(xAxis),
				Input.GetAxis(yAxis)
		);
		Vector2 rotation_limit = new Vector2(x_rotation_limit, y_rotation_limit);
		var cur_rot = calculate_current_rotation(rotation, axes_movement, mouse_sensitivity, rotation_limit);
		rotation = cur_rot.get_euler_rotation();
		return cur_rot;
	}

	static FirstPersonCameraController get_instance() {
		return instance;
	}

	static void lock_camera() {
		Cursor.lockState = CursorLockMode.Locked;
	}

	static void unlock_camera() {
		Cursor.lockState = CursorLockMode.None;
	}

	static FPSCurrentRotation calculate_current_rotation(Vector2 rotation, Vector2 axes_movement, float mouse_sensitivity, Vector2 rot_limit) {
		float new_x_rotation = Mathf.Clamp(rotation.x + axes_movement.x * mouse_sensitivity, -rot_limit.x, rot_limit.x);
		float new_y_rotation = Mathf.Clamp(rotation.y + axes_movement.y * mouse_sensitivity, -rot_limit.y, rot_limit.y);

		FPSCurrentRotation cur_rot = new FPSCurrentRotation(new_x_rotation, new_y_rotation);

		return cur_rot;
	}
}
public class FPSCurrentRotation {
	private Vector2 rotation;
	private Quaternion x_quat;
	private Quaternion y_quat;

	public FPSCurrentRotation(float x_rotation, float y_rotation) {
		rotation = new Vector2(x_rotation, y_rotation);
		this.x_quat = Quaternion.AngleAxis(x_rotation, Vector3.up);
		this.y_quat = Quaternion.AngleAxis(y_rotation, Vector3.left);
	}

	public Vector2 get_euler_rotation() {
		Vector2 euler = new Vector2(rotation.x, rotation.y);
		return euler;
	}

	public Quaternion get_only_y_rotation() {
		return Quaternion.AngleAxis(0f, Vector3.up) * y_quat;
	}

	public Quaternion get_only_x_rotation() {
		return x_quat * Quaternion.AngleAxis(0f, Vector3.left);
	}

	public Quaternion get_full_rotation() {
		return x_quat * y_quat;
	}
}
