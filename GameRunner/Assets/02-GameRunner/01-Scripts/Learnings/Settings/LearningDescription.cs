using System;
using Cohort.CustomAttributes;

[Serializable]
public class LearningDescription {
	public string actionDescription = "Do minigame";
	public string sceneName = "";
	public string data;
	public int[] locations;
	
	[ReadOnly] public State state = State.Open; 
	[ReadOnly] public int index;
	
	public enum State {
		Open = 0,
		Active,
		Completed,
		Failed
	}
}
