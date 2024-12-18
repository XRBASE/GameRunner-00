using System;

namespace Cohort.GameRunner.Minigames.Quiz {
	[Serializable]
	public class QuizData {
		public Question[] _questions;
	}

	[Serializable]
	public class Question {
		public string question;
		public string[] answers;

		public int correctAnswerIndex;
	}
}