using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour {
	public abstract bool is_active();
	public abstract void do_interaction(Interactor interactor);
	public abstract bool can_interact(Interactor interactor);
	public abstract string get_interaction_text();
}
