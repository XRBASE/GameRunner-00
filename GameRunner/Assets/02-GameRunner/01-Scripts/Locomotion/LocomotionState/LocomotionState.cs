using Cohort.Input;
using Cohort.Networking.PhotonKeys;
using Cohort.Ravel.Patterns.States;
using ExitGames.Client.Photon;
using MathBuddy;
using UnityEngine;

namespace Cohort.GameRunner.LocoMovement {
	/// <summary>
	/// Locomotion behaviour state base class.
	/// </summary>
	public abstract class LocomotionState : IStateBehaviour {

		public abstract Locomotion.State State { get; }

		/// <summary>
		/// Input direction in world coordinates
		/// </summary>
		public Vector3 Direction { get; protected set; }
		
		/// <summary>
		/// Speed input in meters per second.
		/// </summary>
		public float Speed { get; private set; }
		
		//goal speed for target.
		private float _targetSpeed;
		//goal rotation for the target.
		protected float _targetRot;
		//direction in which target should rotate (positive or negative)
		private float _rotationDirection;

		protected Locomotion _lm;
		protected Rigidbody _rb;
		protected Transform _target;
		//TODO_COHORT: camera
		//protected Camera _cam;
		
		public LocomotionState(Rigidbody rb, Locomotion lm) {
			_lm = lm;
			_rb = rb;
			_target = rb.transform;

			if (_lm.Control == Locomotion.ControlType.Local) {
				//TODO_COHORT: camera
				//_cam = CameraState.Instance.Camera;
			}
		}

		public virtual void Enter()
		{
			if (_lm.Control == Locomotion.ControlType.Local) {
				InputManager.Instance.PlayerMove.directionChanged += OnDirectionChanged;
				InputManager.Instance.PlayerMove.runStateChanged += OnSpeedChanged;
				InputManager.Instance.PlayerMove.turn += SetRotateDirection;
			}

			_targetRot = _target.rotation.y;
		}

		public virtual void Exit()
		{
			if (_lm.Control == Locomotion.ControlType.Local && !InputManager.Disposed) {
				InputManager.Instance.PlayerMove.directionChanged -= OnDirectionChanged;
				InputManager.Instance.PlayerMove.runStateChanged -= OnSpeedChanged;
				InputManager.Instance.PlayerMove.turn -= SetRotateDirection;
			}
		}
		
		/// <summary>
		/// Set initial values for the state when entering it.
		/// </summary>
		/// <param name="direction">Direction input in world coordinates.</param>
		/// <param name="speed">Speed in metres per second.</param>
		public virtual void InitValues(Vector3 direction, float speed) {
			OnDirectionChanged(new Vector2(direction.x, direction.z));
			_targetRot = _target.rotation.y;
			OnSpeedChanged(speed);
		}

		public virtual void Update() {
			if (Mathf.Abs(_rotationDirection) > FloatingPoints.LABDA) {
				_target.Rotate(Vector3.up, _rotationDirection * Locomotion.ROT_INPUT_SPEED * Time.deltaTime);
			}
			
			UpdateSpeed();
			UpdateRotation();
		}

		/// <summary>
		/// Slowly increments speed value towards the target speed.
		/// </summary>
		protected virtual void UpdateSpeed() {
			float diff;
			if (Direction.magnitude < FloatingPoints.LABDA) {
				diff = 0 - Speed;
			}
			else {
				diff = _targetSpeed - Speed;
			}
			
			if (Mathf.Abs(diff) > FloatingPoints.LABDA) {
				if (diff > 0) {
					if (Direction.magnitude > FloatingPoints.LABDA) {
						Speed = Mathf.Min(Speed + Locomotion.SPEED_INC * Time.deltaTime, _targetSpeed);
					}
				}
				else {
					Speed = Mathf.Max(Speed - Locomotion.SPEED_INC * Time.deltaTime, _targetSpeed);
				}
			}
		}
		
		/// <summary>
		/// Slowly increments rotation value towards the target rotation.
		/// </summary>
		protected virtual void UpdateRotation() {
			if (Mathf.Abs(_targetRot) > FloatingPoints.LABDA) {
				float speed = (_targetRot / Mathf.Abs(_targetRot)) * Locomotion.ROT_SPEED * Time.deltaTime;
				//speed = _targetRot;
				
				if (Mathf.Abs(_targetRot) < Mathf.Abs(speed)) {
					//goto goal
					speed = _targetRot;
				}
				
				_target.Rotate(Vector3.up, speed);
				//Debug.LogError(_targetRot);
				_targetRot -= speed;
			}
		}

		public virtual void OnPlayerPropertiesChanged(Hashtable changes, bool initialize) {
			string key = Keys.Get(Keys.Player.Rotation);
			if (changes.ContainsKey(key) && changes[key] != null) {
				Vector3 fwd = (Quaternion)changes[key] * Vector3.forward;
				fwd.y = 0f;
				
				_target.rotation = Quaternion.LookRotation(fwd.normalized, Vector3.up);
			}
		}
		
		/// <summary>
		/// Called to make the target rotate in (counter) clockwise direction
		/// </summary>
		/// <param name="direction">Direction in which to rotate.</param>
		public void SetRotateDirection(float direction) {
			if (State != Locomotion.State.Move && State != Locomotion.State.Spectator)
				return;
			
			_rotationDirection = direction;
		}
		
		/// <summary>
		/// Called to update the input direction of the target, the y axis is mapped onto the z axis for this input.
		/// </summary>
		/// <param name="direction">Direction 2D of the input, y direction is mapped onto the z axis.</param>
		public virtual void OnDirectionChanged(Vector2 direction) {
			Direction = new Vector3(direction.x, 0f, direction.y);
			
			if (direction.magnitude > FloatingPoints.LABDA) {
				OnSpeedChanged(Mathf.Max(_targetSpeed,  direction.magnitude * Locomotion.WALK_SPEED));
			}
		}
		
		/// <summary>
		/// Called whenever the target speed changes from walking to running.
		/// </summary>
		/// <param name="run">True/False should target be running.</param>
		protected void OnSpeedChanged(bool run) {
			OnSpeedChanged(run ? Locomotion.RUN_SPEED : Locomotion.WALK_SPEED);
		}
		
		/// <summary>
		/// Called to change the targets speed, based on a floating value, in meters per second.
		/// </summary>
		/// <param name="speed">Speed value in metres per second.</param>
		public virtual void OnSpeedChanged(float speed) {
			if (_lm.Control == Locomotion.ControlType.Remote) {
				Speed = speed + Locomotion.REMOTE_DIFF;
			}
			
			_targetSpeed = speed;
		}
		
		/// <summary>
		/// Makes the target look in, or rotate to a specific direction.
		/// </summary>
		/// <param name="direction">direction towards which to rotate.</param>
		/// <param name="worldSpace">Should direction be applied locally or based on a world space rotation?</param>
		/// <param name="direct">Should rotation speed be used, or rotation just be applied instantaneously.</param>
		public void LookInDirection(Vector3 direction, bool worldSpace, bool direct) {
			if (!worldSpace) {
				direction = GetWorldDirection(direction);
			}
			
			if (direct) {
				direction.y = 0f;
				_target.LookAt(_target.position + direction, Vector3.up);
				_targetRot = 0f;
				return;
			}
			
			SetTargetRotation(direction);
		}

		protected Vector3 GetWorldDirection(Vector3 direction) {
			//TODO_COHORT: camera 
			//direction = _cam.transform.TransformDirection(direction);
			//direction = _lm.transform.TransformDirection(direction);
			direction.y = 0f;
			return direction;
		}

		private void SetTargetRotation(Vector3 fwd) {
			Quaternion rot = Quaternion.LookRotation(_target.InverseTransformDirection(fwd), Vector3.up);
			
			_targetRot = rot.eulerAngles.y % 360;
			if (_targetRot > 180) {
				_targetRot -= 360f;
			} else if (_targetRot < -180) {
				_targetRot += 360f;
			}
		}
	}
}
