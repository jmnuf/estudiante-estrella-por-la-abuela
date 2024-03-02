using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TestPlayerCameraHandler : MonoBehaviour {
	public Transform base_camera_transform;
	public GameObject scene_camera;
	public float mouse_sensitivity;

	public GameObject interaction_text_label;

	private float rotation_x;
	private float rotation_y;
	private Vector2 rotation;
	
	const string xAxis = "Mouse X";
	const string yAxis = "Mouse Y";

	void Awake() {
		Cursor.lockState = CursorLockMode.Locked;
	}

	void Start() {
		interaction_text_label.SetActive(false);
	}

	void Update() {
		var cur_rot = calculate_current_rotation();
		move_camera(cur_rot);
		check_for_interactable(cur_rot);
	}

	void FixedUpdate() {
		//if (Input.GetKeyDown(KeyCode.Escape)) {
		if (Input.GetKeyDown(KeyCode.P)) {
			toggle_lock();
		}
	}
	
	(Quaternion, Quaternion) calculate_current_rotation() {
		rotation.x += Input.GetAxis(xAxis) * mouse_sensitivity;
		rotation.y += Input.GetAxis(yAxis) * mouse_sensitivity;
		float y_rot_limit = 80;
		rotation.y = Mathf.Clamp(rotation.y, -y_rot_limit, y_rot_limit);
		var x_quat = Quaternion.AngleAxis(rotation.x, Vector3.up);
		var y_quat = Quaternion.AngleAxis(rotation.y, Vector3.left);

		return (x_quat, y_quat);
	}

	void move_camera((Quaternion, Quaternion) cur_rotation) {
		scene_camera.transform.position = base_camera_transform.position;

		var (x_quat, y_quat) = cur_rotation;
		//transform.localRotation = x_quat * y_quat;
		scene_camera.transform.localRotation = Quaternion.AngleAxis(0f, Vector3.up) * y_quat;
		transform.localRotation = x_quat * Quaternion.AngleAxis(0f, Vector3.left);
	}

	void toggle_lock() {
		if (Cursor.lockState == CursorLockMode.None) {
			Cursor.lockState = CursorLockMode.Locked;
		} else {
			Cursor.lockState = CursorLockMode.None;
		}
	}

	void check_for_interactable((Quaternion, Quaternion) cur_rotation) {
		Vector3 ray_position = base_camera_transform.position;
		var (_, y_quat) = cur_rotation;
		base_camera_transform.localRotation = Quaternion.AngleAxis(0f, Vector3.up) * y_quat;
		float ray_distance = 5f;

		Vector3 ray_direction = base_camera_transform.forward;
		Debug.DrawRay(ray_position, ray_direction * ray_distance, Color.red);

		RaycastHit ray_hit;
		interaction_text_label.SetActive(false);
		if (Physics.Raycast(ray_position, ray_direction, out ray_hit, ray_distance)) {
			var interactable = ray_hit.collider.gameObject.GetComponent<TestInteractable>();
			if (interactable != null && interactable.interaction_active) {
				interaction_text_label.SetActive(true);
				var text_mesh = interaction_text_label.GetComponent<TextMeshProUGUI>();
				text_mesh.text = "Press F to " + interactable.interaction_text;
				if (Input.GetKeyDown(KeyCode.F)) {
					interactable.do_interaction(gameObject);
					interaction_text_label.SetActive(false);
				}
			}
		}
	}

}
