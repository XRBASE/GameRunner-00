using System;
using Cohort.Tools.Timers;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Base.Ravel.Input.Actions
{
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class MouseDragInteraction : IInputInteraction
    {
        private Timer _timer;
        private float _dist;
        private float _threshold = 50f;
        
        static MouseDragInteraction()
        {
            InputSystem.RegisterInteraction<MouseDragInteraction>();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init() { }
        
        public void Reset() { }

        public float GetDeltaMag(InputInteractionContext context)
        {
            return context.action.controls[1].EvaluateMagnitude() + context.action.controls[2].EvaluateMagnitude();
        }

        //check values and fire the started/performed and ended events
        public void Process(ref InputInteractionContext context)
        {
            if (context.timerHasExpired) {
                //shouldn't happen, timer is infinity
                context.Performed();
                return;
            }

            var phase = context.phase;

            switch (phase) {
                case InputActionPhase.Disabled:
                    //don't do anything, disabled
                    break;
                case InputActionPhase.Waiting:
                    //waiting: pre-click called like an update, waiting for something to happen 
                    if (context.ControlIsActuated()) {
                        if (_timer == null) {
                            //duration after which the drag is triggered.
                            _timer = new Timer(0.05f, true);
                            _dist = GetDeltaMag(context);
                        }
                        else if (!_timer.HasFinished && _dist < _threshold) {
                            _dist += GetDeltaMag(context);
                        }
                        else {
                            context.Started();
                            context.SetTimeout(float.PositiveInfinity);
                            
                            _timer.Stop();
                            _timer = null;
                        }
                    } else if (!((ButtonControl) context.action.controls[0]).isPressed && _timer != null) {
                        _timer.Stop();
                        _timer = null;
                    }
                    break;
                case InputActionPhase.Started:
                    //this is called the frame after the button is initially pressed, but only called once
                    context.PerformedAndStayPerformed();
                    break;
                case InputActionPhase.Performed:
                    //update version each frame while button pressed and moving the axes
                    if (context.ControlIsActuated()) {
                        //button still pressed
                        context.PerformedAndStayPerformed();
                    }
                    else if (!((ButtonControl) context.action.controls[0]).isPressed) {
                        //button released (only one frame)
                        context.Canceled();
                    }
                    break;
                case InputActionPhase.Canceled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(phase), phase, null);
            }
        }
    }

#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    public class MouseDragComposite : InputBindingComposite<Vector2>
    {
        [InputControl(layout = "Button")]
        public int button;
 
        [InputControl(layout = "Axis")]
        public int axis1;
 
        [InputControl(layout = "Axis")]
        public int axis2;

        static MouseDragComposite()
        {
            InputSystem.RegisterBindingComposite<MouseDragComposite>();
        }
 
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
        }
 
        public override Vector2 ReadValue(ref InputBindingCompositeContext context)
        {
            //first check if button pressed
            var b = context.ReadValueAsButton(button);
            Vector2 v = default;
            if (b) {
                v.x = context.ReadValue<float>(axis1);
                v.y = context.ReadValue<float>(axis2);
            }
            //when button pressed v is value of axes, otherwise v is default (I think vector2.zero)
            return v;
        }
 
        public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
        {
            //check magnitude of vector
            return ReadValue(ref context).magnitude;
        }
    }
}