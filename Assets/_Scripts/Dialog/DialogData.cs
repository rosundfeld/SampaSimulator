using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DialogCharacter
{
    public string name;
    public Sprite portrait;
}

[Serializable]
public class DialogLine
{
    public DialogCharacter character;
    [TextArea(3, 10)]
    public string line;
}

[Serializable]
public class Dialog
{
    public List<DialogLine> dialogueLines = new List<DialogLine>();
}
