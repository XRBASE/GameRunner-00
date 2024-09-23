using System;
using UnityEngine;

namespace Cohort.GameRunner.Input.Actions
{
    using Input = UnityEngine.Input;

    public class AxisInput
    {
        private const float DEAD_ZONE = .2f;
        private readonly string _xAxisName;
        private readonly string _yAxisName;


        /// <summary>
        /// Initialises the axis with given axis names from the unity Input Manager
        /// </summary>
        /// <param name="xAxisName"></param>
        /// <param name="yAxisName"></param>
        public AxisInput(string xAxisName, string yAxisName)
        {
            _xAxisName = xAxisName;
            _yAxisName = yAxisName;
            try
            {
                Input.GetAxis(xAxisName);
                Input.GetAxis(yAxisName);
            }
            catch
            {
                Debug.LogError("Axis with this name does not exist, Add the axis name in the Unity Input Manager");
                throw;
            }
        }

        /// <summary>
        /// Reads the axis value of an input axis and executes onAxisChanged 
        /// </summary>
        /// <param name="onAxisChanged">Executes true when the axis value is outside the DeadZone</param>
        public bool GetAxis(out Vector2 value)
        {
            value = new Vector2(Input.GetAxis(_xAxisName),
                Input.GetAxis(_yAxisName));
            
            if (!MathBuddy.FloatingPoints.IsInBounds(value.x, -DEAD_ZONE, DEAD_ZONE) ||
                !MathBuddy.FloatingPoints.IsInBounds(value.y, -DEAD_ZONE, DEAD_ZONE)) {
                return true;
            }
            else {
                value = Vector2.zero;
                return false;
            }
        }
    }
}