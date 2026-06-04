using UnityEngine;

using System;

[Serializable]
public class DialogueDatabase
{
    public Dialogue[] dialogues;
}

[Serializable]
public class Dialogue
{
    public string id;
    public string speaker;
    public string[] lines;
}