using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModularCar
{
	public class ForcePower : MonoBehaviour
	{
		private CarControllerV3 control;
		private Rigidbody rb;
		private InputController input;

		[Tooltip("Y - Acceleration strength, X - Current speed")]
		public AnimationCurve accelerationCurve = AnimationCurve.Linear(0.0f, 50, 80, 0);

		private float verticalInput;

		private void Start()
		{
			verticalInput = 0;
			control = this.GetComponent<CarControllerV3>();
			input = this.GetComponent<InputController>();
			rb = control.rb;

			Debug.Assert(control != null, "Must have a controller");
			Debug.Assert(input != null, "Must have an input controller");
		}

		private void FixedUpdate()
		{
			verticalInput = input.vertical;

			//Add acceleration to car
			float forwardPower = verticalInput * accelerationCurve.Evaluate(control.currentSpeed) * control.wheelPower;
			rb.AddForce(rb.transform.forward * forwardPower, ForceMode.Acceleration);
		}
	}
}