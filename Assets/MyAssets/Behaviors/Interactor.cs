using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactor : MonoBehaviour {
	public Transform ray_ref_object;
	[SerializeField]
	[Range(1f, 50f)]
	private float ray_distance = 5f;

	public delegate void Can_Interact(Interactable interactable);
	public delegate void Interacted_With(Interactable interactable);
	public Can_Interact can_interact;
	public Interacted_With interacted_with;

	protected Interactable check_for_interactions(FPSCurrentRotation cur_rotation) {
		Vector3 ray_position = ray_ref_object.position;
		if (cur_rotation != null) ray_ref_object.localRotation = cur_rotation.get_only_y_rotation();

		Vector3 ray_direction = ray_ref_object.forward;
		Debug.DrawRay(ray_position, ray_direction * ray_distance, Color.red);

		RaycastHit ray_hit;
		if (Physics.Raycast(ray_position, ray_direction, out ray_hit, ray_distance)) {
			var interactable = ray_hit.collider.gameObject.GetComponent<Interactable>();
			if (interactable != null && interactable.is_active()) {
				if (!interactable.can_interact(this)) return null;
				can_interact?.Invoke(interactable);
				return interactable;
			}
		}

		return null;
	}

	protected void interact(Interactable interactable) {
		interactable.do_interaction(this);
		interacted_with?.Invoke(interactable);
	}
}
