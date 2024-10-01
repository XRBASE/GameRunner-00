using System;

namespace Cohort.GameRunner.Loading {
	public class LoadingAction {
		public Action<LoadType> onLoadingChanged;

		public LoadType Type {
			get { return _type; }
		}
		public bool Finished { get; private set; }
		public string Message { get; set; }
		public float Progress {
			get { return _step / (float)_nSteps; }
		}
		
		private int _nSteps = -1;
		private int _step = 0;
		private LoadType _type;

		public LoadingAction(LoadType type, string message, int nSteps = -1) {
			Message = message;
			
			_nSteps = nSteps;
			_step = 0;
			_type = type;
			
			Finished = false;
		}

		public void Start() {
			onLoadingChanged?.Invoke(_type);
		}

		public void Increment(string message) {
			_step++;
			if (!string.IsNullOrEmpty(message)) {
				SetMessage(message);
			}
			else {
				onLoadingChanged?.Invoke(_type);
			}
		}

		public void SetMessage(string message) {
			Message = message;
			
			onLoadingChanged?.Invoke(_type);
		}

		public void Finish() {
			_step = _nSteps;
			
			Finished = true;
			onLoadingChanged?.Invoke(_type);
		}
	}
}