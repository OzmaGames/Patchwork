using UnityEngine;
using System.Collections;

public abstract class GameState
{
	public Game ActiveGame;
	
	public abstract void Start();
	public abstract void Stop();
	public abstract void Update();
}
