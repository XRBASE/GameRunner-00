using System;

[Serializable]
public class MiniGameDescription {
    public string actionDescription = "Solve quiz";
    public string sceneName;
    public string data;

    [NonSerialized] public State _state = State.None;
    [NonSerialized] public int _index = -1;

    public enum State {
        None = 0,
        InGame,
        Completed,
        Failed,
    }
}
