using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ModularCar
{
	class ForceSteering : MonoBehaviour
	{
		private CarControllerV3 control;
		private Rigidbody rb;
		private InputController input;
		private Transform steeringTransform;

		[Header("Steering")]
		[Tooltip("Y - Steer strength, X - Current speed")]
		public AnimationCurve steeringCurve = AnimationCurve.Linear(0.0f, 1f, 60, .4f);
		[Tooltip("This is so the car doesn't turn on the spot, it needs to be moving to start turning")]
		public float fullTurnSpeed = 10;
		public float steerPower = 15;
		public float angularTraction = 15;

		public ForceMode forceMode = ForceMode.Acceleration;

		[Header("Telemetry")]
		public float turningPower;

		private void Start()
		{
			control = this.GetComponent<CarControllerV3>();
			input = this.GetComponent<InputController>();
			steeringTransform = control.steeringTransform;
			rb = control.rb;

			Debug.Assert(control != null, "Must have a controller");
			Debug.Assert(input != null, "Must have an input controller");
		}

		private void FixedUpdate()
		{
			Turn();
		}

		private void Turn()
		{
			var steerSpeed = steeringCurve.Evaluate(control.currentSpeed);
			turningPower = input.horizontalInputDriftAdjusted * (Mathf.Clamp(control.currentSpeed, -fullTurnSpeed, fullTurnSpeed) / fullTurnSpeed) * control.wheelPower;

			RaycastHit hit;
			Vector3 down = -steeringTransform.transform.up;

			if (Physics.Raycast(steeringTransform.transform.position, down, out hit, control.wheelRadius))
			{
				var x = Vector3.Cross(rb.transform.forward, hit.point - steeringTransform.transform.position);
				var something = Vector3.Reflect(x, hit.normal).normalized;

				//Debug.DrawLine(steeringTransform.transform.position, hit.point);
				Debug.DrawRay(steeringTransform.transform.position, 5 * something);
				Debug.DrawRay(steeringTransform.transform.position, steeringTransform.transform.right, Color.blue);
				something = steeringTransform.transform.right;

				//Steer
				rb.AddForceAtPosition(turningPower * something * steerPower * control.wheelPower, steeringTransform.position, forceMode);

				//Add counter force so the car doesn't keep spinning
				rb.AddTorque(-new Vector3(0.0f, rb.angularVelocity.y, 0.0f) * angularTraction * control.wheelPower, ForceMode.Acceleration);
			}
		}
	}
}