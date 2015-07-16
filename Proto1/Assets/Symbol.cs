using UnityEngine;
using System.Collections;

public class Symbol : MonoBehaviour
{
	public enum SymbolTypes
	{
		Scissor,
		Thread,
		Needle
	}
	[SerializeField]
	SymbolTypes Type = SymbolTypes.Scissor;

	public enum CompareResult
	{
		Win,
		Draw,
		Lose
	}

	public static GameObject[] s_Symbols;
	public static GameObject Instantiate(SymbolTypes type)
	{
		for(int i = 0; i < s_Symbols.Length; ++i)
		{
			if(s_Symbols[i].GetComponent<Symbol>().Type == type)
			{
				return Instantiate(s_Symbols[i]);
			}
		}
		throw new UnityException("Type not found: " + type.ToString());
	}
	
	static SymbolTypes s_PreviousSymbolType = SymbolTypes.Scissor;
	public static SymbolTypes GetRandomSymbolType()
	{
		SymbolTypes type = s_PreviousSymbolType;
		switch(s_PreviousSymbolType)
		{
		case SymbolTypes.Scissor:
			s_PreviousSymbolType = SymbolTypes.Thread;
			break;
		case SymbolTypes.Thread:
			s_PreviousSymbolType = SymbolTypes.Needle;
			break;
		default:
			s_PreviousSymbolType = SymbolTypes.Scissor;
			break;
		}
		return s_PreviousSymbolType;
	}

	public CompareResult Compare(Symbol other)
	{
		switch(Type)
		{
		case SymbolTypes.Scissor:
			switch(other.Type)
			{
			case SymbolTypes.Thread:	return CompareResult.Win;
			case SymbolTypes.Needle:	return CompareResult.Lose;
			}
			break;
		case SymbolTypes.Thread:
			switch(other.Type)
			{
			case SymbolTypes.Needle:	return CompareResult.Win;
			case SymbolTypes.Scissor:	return CompareResult.Lose;
			}
			break;
		case SymbolTypes.Needle:
			switch(other.Type)
			{
			case SymbolTypes.Scissor:	return CompareResult.Win;
			case SymbolTypes.Thread:	return CompareResult.Lose;
			}
			break;
		}
		return CompareResult.Draw;
	}
}
