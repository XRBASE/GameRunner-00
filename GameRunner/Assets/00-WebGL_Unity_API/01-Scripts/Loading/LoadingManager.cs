using System;
using System.Collections.Generic;
using Cohort.Patterns;

namespace Cohort.GameRunner.Loading {
	public sealed class LoadingManager : Singleton<LoadingManager> {
		public Action onLoadingStart;
		public Action<float, string> onLoadingChanged;
		public Action onLoadingFinished;

		public LoadingAction this[LoadPhase p, LoadType t] {
			get { return _phases[p][t]; }
		}

		private Dictionary<LoadPhase, LoadingPhase> _phases;
		
		protected override void Awake() {
			base.Awake();
			
			SetUpPhases();
		}

		private void SetUpPhases() {
			_phases = new Dictionary<LoadPhase, LoadingPhase>();
			_phases.Add(LoadPhase.Scene, new LoadingPhase(
				            new LoadingAction(LoadType.RetrieveUserData, "Loading user data", 2),
				            new LoadingAction(LoadType.ConnectToPhoton, "Connecting to server", 4),
				            new LoadingAction(LoadType.LoadEnvironmentScene, "Loading lobby", 2)));
			
			foreach (var kv_phase in _phases) {
				kv_phase.Value.onLoadingStart += OnLoadingStart;
				kv_phase.Value.onLoadingProgression += OnLoadingChanged;
				kv_phase.Value.onLoadingFinished += OnLoadingFinished;
			}
		}

		private void OnLoadingStart() {
			onLoadingStart?.Invoke();
		}

		private void OnLoadingChanged(float progression, string message) {
			onLoadingChanged?.Invoke(progression, message);
		}

		private void OnLoadingFinished() {
			onLoadingFinished?.Invoke();
		}
	}

	public class LoadingPhase {
		public Action onLoadingStart;
		public Action<float, string> onLoadingProgression;
		public Action onLoadingFinished;

		public LoadingAction this[LoadType t] { get { return _actions[t]; } }
		public float Progression { get; private set; }
		public bool Finished { get; private set; }

		private bool _started = false;
		private Dictionary<LoadType, LoadingAction> _actions;

		public LoadingPhase(params LoadingAction[] actions) {
			_actions = new Dictionary<LoadType, LoadingAction>();
			Progression = 0f;
			Finished = false;
			
			for (int i = 0; i < actions.Length; i++) {
				_actions[actions[i].Type] = actions[i]; 
				_actions[actions[i].Type].onLoadingChanged += OnActionUpdated;
			}
		}

		private void OnActionUpdated(LoadType type) {
			Progression = 0f;
			if (!_started) {
				onLoadingStart?.Invoke();
				_started = true;
			}
			
			bool allFin = true;
			foreach (var kv_action in _actions) {
				if (kv_action.Value.Finished) {
					Progression += 1f;
				}
				else {
					allFin = false;
					Progression += kv_action.Value.Progress;
				}
			}

			if (allFin) {
				Progression = 1f;
				Finished = true;
				
				onLoadingFinished?.Invoke();
			}
			else
			{
				Progression = Progression / _actions.Count;
				onLoadingProgression?.Invoke(Progression, _actions[type].Message);
			}
		}
	}
	
	public enum LoadType {
		RetrieveUserData,
		ConnectToPhoton,
		LoadEnvironmentScene,
	}

	public enum LoadPhase {
		Scene,
	}
}