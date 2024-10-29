using System;
using Unity.Collections;

[Serializable]
public class LearningDescription {
	public string actionDescription = "Do minigame";
	public string sceneName = "";
	public string data;
	public int[] locations;
	
	[ReadOnly] public State state = State.None;
	[ReadOnly] public int index;
	
	public enum State {
		None = 0,
		Active,
		Completed,
		Failed
	}
}
