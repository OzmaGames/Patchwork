using UnityEngine;
using System.Collections;

public class GUI_Base : MonoBehaviour
{
	protected bool IsDirty = true;
	
	void Start()
	{
	}
	
	void Update()
	{
	}

	void OnValidate()
	{
		IsDirty = true;
	}
}
