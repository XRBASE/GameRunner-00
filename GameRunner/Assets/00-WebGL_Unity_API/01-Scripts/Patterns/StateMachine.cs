using System;
using System.Collections.Generic;

namespace Cohort.Ravel.Patterns.States
{
	/// <summary>
	/// generic state machine implementation that aut switches states with matching Enter and Exit calls.
	/// </summary>
	/// <typeparam name="I">State index/identifier, used to read and set what state is currently active.</typeparam>
	/// <typeparam name="Behaviour">Behaviour of the state, implements IStateBehaviour.</typeparam>
	public class StateMachine<I, Behaviour> where Behaviour : IStateBehaviour
	{
		public bool InState { get; private set; }
		public I State {
			get { return _state;}
			set { ChangeState(value); }
		}

		public Behaviour Current {
			get { return this[State]; }
		}
		
		public Behaviour this[I id] {
			get { return _states[id]; }
			set {
				if (_states.ContainsKey(id)) {
					if (_states[id].Equals(value))
						return;
					
					if (State.Equals(id)) {
						_states[id].Exit();
						value.Enter();
						InState = true;
					}
				}

				_states[id] = value;
			}
		}

		private Dictionary<I, Behaviour> _states;
		private I _state;
		
		public StateMachine() {
			_states = new Dictionary<I, Behaviour>();
			InState = false;
		}
		
		~StateMachine() {
			Current.Exit();
			InState = false;
		}
		
		public StateMachine(int capacity) {
			_states = new Dictionary<I, Behaviour>(capacity);
			InState = false;
		}

		public void ChangeState(I newState) {
			if (InState && _state.Equals(newState))
				return;
			
			if (_states.ContainsKey(newState)) {
				if (this[State] != null) {
					this[State].Exit();
				}
				
				this[newState].Enter();
				InState = true;
				_state = newState;
			}
			else {
				throw new Exception($"Missing state {newState} in statemachine!");
			}
		}

		public void Update() {
			this[State].Update();
		}
	}

	public interface IStateBehaviour
	{
		public void Enter();
		public void Exit();
		public void Update();
	}
}