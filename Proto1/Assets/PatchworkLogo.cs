using UnityEngine;
using System.Collections;

public class PatchworkLogo : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Animator animator = GetComponent<Animator>();
//		animator.Play(Animator.StringToHash("logo_hide"));
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void NotifyShow()
	{
		Debug.Log("ADJAOSDJO");
	}
}
