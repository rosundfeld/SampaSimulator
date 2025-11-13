using System;
using System.Collections.Generic;
using UnityEngine;

public enum MenuItem
{
	start,
	exit,
	credits
}

[Serializable]
public class MyEnumEntryMaterial
{
	public MenuItem key;
	public Material value;
}

[Serializable] // ADICIONADO
public class MyEnumEntryGameObject
{
	public MenuItem key;
	public List<GameObject> value;
}

[Serializable]
public class MyEnumEntryMaterialList
{
	public List<MyEnumEntryMaterial> entries;
}

[Serializable]
public class MyEnumEntryGameObjectList
{
	public List<MyEnumEntryGameObject> entries;
}

public class MenuManager : MonoBehaviour
{
	public Material glowMaterial;
	public Material standardMaterial;
	public List<MyEnumEntryGameObjectList> wordGameObject;     // agora serializável no Inspector

	void Start() { }

	void Update() { }

	void OnMouseEnter()
	{
		// Change to the hover material when the mouse enters
		if (glowMaterial != null)
		{
			foreach (var objList in wordGameObject)
			{
				foreach (var objEntry in objList.entries)
				{
					foreach (var obj in objEntry.value)
					{
						var renderer = obj.GetComponent<Renderer>();
						if (renderer != null)
						{
							renderer.material = glowMaterial;
						}
					}
				}
			}
		}
	}

	void OnMouseExit()
	{
		if (standardMaterial != null)
		{
			foreach (var objList in wordGameObject)
			{
				foreach (var objEntry in objList.entries)
				{
					foreach (var obj in objEntry.value)
					{
						var renderer = obj.GetComponent<Renderer>();
						if (renderer != null)
						{
							renderer.material = standardMaterial;
						}
					}
				}
			}
		}
	}
}