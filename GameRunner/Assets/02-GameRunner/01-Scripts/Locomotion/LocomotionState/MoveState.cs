//Old MoveState
using Cohort.GameRunner.AvatarAnimations;
using Cohort.Networking.PhotonKeys;
using Cohort.GameRunner.Input;
using UnityEngine;
using MathBuddy;
using System.Collections;
using System;

using Hashtable = ExitGames.Client.Photon.Hashtable;

namespace Cohort.GameRunner.LocoMovement {
	/// <summary>
	/// State specifically made for walking around using input.
	/// </summary>
	public class MoveState : LocomotionState {
		public override Locomotion.State State {
			get { return Locomotion.State.Move; }
		}

		/// <summary>
		/// Checks if move to cursor routine is currently active.
		/// </summary>
		public bool UsingCoroutine {
			get { return _moveToRoutine != null; }
		}

		public enum MovementType {
			Default,
			Walking,
			Running,
			Teleporting,
		};
		
		private float _fallMultiplier = 1.75f;
		
		private float FallDelta => _rb.velocity.y + (Vector3.up * (Physics.gravity.y * (_fallMultiplier - 1) * Time.deltaTime)).y;

		//routine used for moving towards the cursor
		private Coroutine _moveToRoutine;

		//these values are used to update the coroutine for walking, without stopping and restarting it
		//timer for walk routine
		private float _numTimer;

		//goal position for walk routine
		private Vector3 _numGoal;

		//goal reached action for walk routine.
		private Action _numGoalReached;

		//routine used for jumping.
		private Coroutine _jumpRoutine;

		//counts the amount of jumps to prevent triple jumping, even when the animator does not register a jump properly.
		private int _jumpCounter;

		//position at which the target started falling, used to track the distance that the target fell.
		private Vector3 _fallPosition;

		//is the target falling
		private bool _falling;

		//the factor in which we fall faster than standard gravity
	


		public MoveState(Rigidbody rb, Locomotion lm) : base(rb, lm) { }

		public override void Enter() {
			base.Enter();

			_lm.StepCaster.enabled = true;
			if (_lm.Control == Locomotion.ControlType.Local) {
				InputManager.Instance.PlayerMove.moveToCursor += MoveToPosition;
				InputManager.Instance.PlayerMove.teleportToCursor += TeleportToPosition;
				InputManager.Instance.GamepadInput.leftJoystickAxisChanged += OnDirectionSpeedChange;

				InputManager.Instance.PlayerMove.jump += Jump;
				InputManager.Instance.GamepadInput.rightDownButtonPressed += Jump;
			}

			_lm.GroundCheck.onGroundHit += OnLand;
			_lm.GroundCheck.onFalling += OnFall;
		}

		public override void Exit() {
			base.Exit();
			StopMoveRoutine();

			_lm.StepCaster.enabled = false;
			if (_lm.Control == Locomotion.ControlType.Local) {
				InputManager.Instance.PlayerMove.moveToCursor -= MoveToPosition;
				InputManager.Instance.PlayerMove.teleportToCursor -= TeleportToPosition;
				InputManager.Instance.GamepadInput.leftJoystickAxisChanged -= OnDirectionSpeedChange;

				InputManager.Instance.PlayerMove.jump -= Jump;
				InputManager.Instance.GamepadInput.rightDownButtonPressed -= Jump;
			}

			_lm.GroundCheck.onGroundHit -= OnLand;
			_lm.GroundCheck.onFalling -= OnFall;

			_jumpCounter = 0;
			_falling = false;

			OnDirectionChanged(Vector2.zero);
			OnSpeedChanged(0f);
		}

		public override void Update() {
			base.Update();

			if (_lm.Control != Locomotion.ControlType.Remote && _moveToRoutine == null) {
				if (!_lm.Seated && Direction.magnitude > FloatingPoints.LABDA) {
					MoveTo(Direction, false);
				}
				else {
					_rb.velocity = new Vector3(0, FallDelta,0f);
				}
			}

			if (_falling) {
				UpdateFalling();
			}
		}

		/// <summary>
		/// Called while falling, respawns player after falling a certain amount of units. 
		/// </summary>
		protected void UpdateFalling() {
			if ((_fallPosition - _target.position).y >= Locomotion.FALL_RESET_HEIGHT) {
				if (SpawnPoint.TryGetById(SpawnPoint.DEFAULT, out SpawnPoint spawn)) {
					spawn.TeleportToSpawnPoint();
				}
				else {
					TeleportToPosition(Vector3.zero, Quaternion.identity);
				}
				_fallPosition = _target.position;
			}
		}

		public override void OnPlayerPropertiesChanged(Hashtable changes, bool initialize) {
			string key = Keys.Get(Keys.Player.Position);
			if (_lm.Control != Locomotion.ControlType.Local &&
			    changes.ContainsKey(key) && changes[key] != null) {
				if (initialize) {
					_target.position = (Vector3) changes[key];
				}
				else {
					MoveToPosition((Vector3) changes[key]);
				}
			}

			base.OnPlayerPropertiesChanged(changes, initialize);
		}

		/// <summary>
		/// Slowly increments speed value towards the target speed.
		/// </summary>
		protected override void UpdateSpeed() {
			base.UpdateSpeed();
			_lm.Animator.SetSpeed(Speed / Locomotion.RUN_SPEED);
		}

		/// <summary>
		/// Called to change the targets speed, based on a floating value, in meters per second.
		/// </summary>
		/// <param name="speed">Speed value in metres per second.</param>
		public override void OnSpeedChanged(float speed) {
			base.OnSpeedChanged(speed);
			if (_lm.Animator != null) {
				_lm.Animator.SetSpeed(Speed / Locomotion.RUN_SPEED);
			}
		}

		/// <summary>
		/// Moves target in a specific direction.
		/// </summary>
		/// <param name="direction">Direction in which to move the target.</param>
		/// <param name="worldSpace">True/False is direction in world coordinates or should it be mapped using the camera alignment.</param>
		protected void MoveTo(Vector3 direction, bool worldSpace) {
			_lm.Animator.SetState(CharAnimator.AnimationState.Moving);
			LookInDirection(direction, worldSpace, true);
			
			Vector3 v = _target.TransformVector(Vector3.forward * Speed);
			v.y = FallDelta;
			_rb.velocity = v;
		}

		public void Jump(bool pressed) {
			if (pressed)
				Jump();
		}

		/// <summary>
		/// Make target jump using coroutine values.
		/// </summary>
		public void Jump() {
			if (_lm.Seated)
				return;
			float jumpHeight = Locomotion.JUMP_HEIGHT;
			if (_jumpCounter < 2) {
				CharAnimator.AnimationState state;
				if (_lm.Animator.State is CharAnimator.AnimationState.Jump or CharAnimator.AnimationState.Fall) {
					
					state = CharAnimator.AnimationState.DoubleJump;
					jumpHeight = Locomotion.DOUBLE_JUMP_HEIGHT;
				}
				else {
					state = CharAnimator.AnimationState.Jump;
				}

				if (!_lm.Animator.CanEnterState(state))
					return;

				_jumpCounter++;
				_lm.Animator.ForceEnterState(state);
				if (_lm.Control == Locomotion.ControlType.Local) {
					_lm.NetworkJump(state);
				}

				StopJumpRoutine();
				_lm.GroundCheck.enabled = false;
	
				_jumpRoutine = _lm.StartCoroutine(DoJump(jumpHeight));
			}
		}

		/// <summary>
		/// Coroutine used for making the jump happen.
		/// </summary>
		private IEnumerator DoJump(float jumpHeight) {
			float f = Mathf.Sqrt(-2.0f * Physics.gravity.y * jumpHeight);
			Vector3 v = new Vector3(_rb.velocity.x, f, _rb.velocity.z);
			_rb.velocity = v;

			while (_rb.velocity.y > 0) {
				yield return new WaitForFixedUpdate();
			}

			_rb.velocity = new Vector3(_rb.velocity.x, 0, _rb.velocity.z);
			StopJumpRoutine();
		}

		/// <summary>
		/// Controller specific speed change call that handles both direction and speed based on the direction magnitude.
		/// </summary>
		public void OnDirectionSpeedChange(Vector2 direction) {
			StopMoveRoutine();
			
			if (Direction.magnitude < FloatingPoints.LABDA) {
				OnDirectionChanged(direction);
				return;
			}

			float speed = (Locomotion.RUN_SPEED - Locomotion.WALK_SPEED) * direction.magnitude;
			speed += Locomotion.WALK_SPEED;

			OnSpeedChanged(speed);
			OnDirectionChanged(direction);
		}

		/// <summary>
		/// Arrow keys direction change call, that only applies direction and not speed.
		/// </summary>
		public override void OnDirectionChanged(Vector2 direction) {
			if (_lm.Seated)
				return;

			StopMoveRoutine();

			base.OnDirectionChanged(direction);

			if (Direction.magnitude < FloatingPoints.LABDA && _lm.Animator != null) {
				_lm.Animator.SetState(CharAnimator.AnimationState.Idle);
			}
		}

		/// <summary>
		/// Teleport target to given (world)position. Also directs players focus in the move/teleport direction.
		/// </summary>
		public void TeleportToPosition(Vector3 position) {
			if (_lm.Seated)
				return;

			StopMoveRoutine();
			StopJumpRoutine();

			_jumpCounter = 0;
			LookInDirection(position - _target.position, true, true);

			_rb.MovePosition(position + Vector3.up * 0.05f);
			//_target.transform.position = position + Vector3.up * 0.05f;
			_lm.GroundCheck.ResetCheck();

			_lm.Animator.SetState(CharAnimator.AnimationState.Teleport);
		}

		/// <summary>
		/// Teleport target to given goal position and rotation.
		/// </summary>
		public void TeleportToPosition(Transform goal) {
			TeleportToPosition(goal.position, goal.rotation);
		}

		/// <summary>
		/// Teleport target to given (world)position and rotation.
		/// </summary>
		public void TeleportToPosition(Vector3 position, Quaternion rotation) {
			if (_lm.Seated)
				return;

			StopMoveRoutine();
			StopJumpRoutine();

			_jumpCounter = 0;

			_rb.MovePosition(position);
			//_target.position = position;
			_target.rotation = rotation;
			_lm.GroundCheck.ResetCheck();

			_lm.Animator.SetState(CharAnimator.AnimationState.Teleport);
		}

		/// <summary>
		/// Moves the target towards a world position
		/// </summary>
		public void MoveToPosition(Vector3 wPos) {
			MoveToPosition(wPos, null);
		}

		public void MoveToPosition(Vector3 wPos, MovementType movementType) {
			MoveToPosition(wPos, null, movementType);
		}
		
		/// <summary>
		/// Moves the target towards a world position and fires goal action.
		/// </summary>
		public void MoveToPosition(Vector3 wPos, Action onGoalReached,
		                           MovementType movementType = MovementType.Default) {
			if (_lm.Seated)
				return;
			Vector3 path = wPos - _target.position;

			if (movementType == MovementType.Default) {
				if ((_lm.Control != Locomotion.ControlType.Local || InputManager.Instance.teleportEnabled)
				    && path.magnitude > Locomotion.TELEPORT_THRESHOLD) {
					movementType = MovementType.Teleporting;
				}
				else {
					movementType = path.magnitude >= Locomotion.RUN_THRESHOLD ? MovementType.Running : MovementType.Walking;
				}
			}

			switch (movementType) {
				case MovementType.Walking:
				case MovementType.Running:
					ConfigureMovement(path, wPos, onGoalReached, movementType);
					break;
				case MovementType.Teleporting:
					TeleportToPosition(wPos);
					onGoalReached?.Invoke();
					break;
			}
		}

		/// <summary>
		/// Configures the movement parameters before starting the movement routine
		/// </summary>
		private void ConfigureMovement(Vector3 path, Vector3 wPos, Action onGoalReached, MovementType movementType) {
			var speed = movementType == MovementType.Running ? Locomotion.RUN_SPEED : Locomotion.WALK_SPEED;
			_numGoal = wPos;
			_numTimer = (path.magnitude / speed) + Locomotion.TIMEOUT_THRESHOLD;
			_numGoalReached = onGoalReached;
			OnSpeedChanged(speed);

			if (_moveToRoutine == null) {
				_moveToRoutine = _lm.StartCoroutine(DoMoveRoutine());
			}
		}
		
		/// <summary>
		/// Routine that runs to keep player moving to a given position in a given time and fire an action
		/// whenever the position is reached.
		/// </summary>
		private IEnumerator DoMoveRoutine() {
			Vector3 path;
			Vector3 v;

			while (_numTimer > 0f) {
				_numTimer -= Time.deltaTime;
				path = _numGoal - _target.position;
				path.y = 0f;

				if (path.magnitude > Speed * Time.deltaTime) {
					Direction = path.normalized;
					MoveTo(Direction, true);

					yield return null;
				}
				else {
					_numGoal.y = _target.position.y;
					v = _numGoal - _target.transform.position;
					v.y = _rb.velocity.y;
					_rb.velocity = v;

					break;
				}
			}

			OnSpeedChanged(Locomotion.WALK_SPEED);
			Direction = Vector3.zero;
			_lm.Animator.SetState(CharAnimator.AnimationState.Idle);

			if (_numTimer <= 0f) {
				if (_lm.Control == Locomotion.ControlType.Local) {
					TeleportToPosition(_numGoal);
				}
				else {
					//don't use the teleport state for the remote
					_rb.MovePosition(_numGoal);
				}
			}

			_moveToRoutine = null;
			_numGoalReached?.Invoke();
		}

		/// <summary>
		/// Called whenever target lands on any surface.
		/// </summary>
		protected virtual void OnLand() {
			_falling = false;
			_jumpCounter = 0;

			if (_lm.Animator.State is CharAnimator.AnimationState.DoubleJump
				or CharAnimator.AnimationState.DoubleFall) {
				_lm.Animator.ForceEnterState(CharAnimator.AnimationState.DoubleLand);
			}
			else {
				_lm.Animator.ForceEnterState(CharAnimator.AnimationState.Land);
			}
		}

		/// <summary>
		/// Called whenever target starts falling.
		/// </summary>
		protected void OnFall() {
			if (_lm.Control == Locomotion.ControlType.Local) {
				_fallPosition = _target.transform.position;
			}

			_falling = true;
			_lm.Animator.SetState(CharAnimator.AnimationState.Fall);
		}

		public void StopMoving() {
			StopMoveRoutine();
			Direction = Vector3.zero;
		}

		/// <summary>
		/// Stops the move to cursor routine.
		/// </summary>
		private void StopMoveRoutine() {
			if (_moveToRoutine != null) {
				_lm.StopCoroutine(_moveToRoutine);
				_moveToRoutine = null;
				_numGoalReached = null;
			}

			Direction = Vector3.zero;
		}

		/// <summary>
		/// Stops the jumping routine.
		/// </summary>
		private void StopJumpRoutine() {
			if (_jumpRoutine == null)
				return;

			_lm.StopCoroutine(_jumpRoutine);
			_jumpRoutine = null;
			_lm.GroundCheck.enabled = true;
			//reset and fire in case there is a floor below you when stopping
			_lm.GroundCheck.ResetCheck(true);
		}
	}
}