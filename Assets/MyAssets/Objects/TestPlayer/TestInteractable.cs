using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInteractable : MonoBehaviour {
	public string interaction_text;
	public bool interaction_active = true;
	public bool oneshot_interaction;
	public void do_interaction(GameObject interactor) {
		Debug.Log("Interaction> " + interactor.name + " interacted with " + gameObject.name);
		if (oneshot_interaction) {
			interaction_active = false;
		}
	}
}
