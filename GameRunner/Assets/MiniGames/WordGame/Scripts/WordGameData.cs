using System;
using System.Collections.Generic;


[Serializable]
public class WordGameData
{
    public WordGame.WordGameMode wordGameMode;
    public string title;
    public int tries;
    public int puzzleAmount;
    public List<WordData> wordList;
}


[Serializable]
public class WordData
{
    public string word;
    public string hint;
}