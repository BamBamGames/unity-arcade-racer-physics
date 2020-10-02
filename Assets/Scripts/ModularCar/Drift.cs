using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ModularCar
{
	public class Drift : MonoBehaviour
	{
		private CarControllerV3 control;
		private InputController input;

		private bool driftingRight;

		[Header("Drift variables")]
		public float inputThreshold = .01f;
		public float minDriftSpeed = 20;
		[Tooltip("How effective is horizontal input when drifting")]
		public float driftStrength = .5f;
		//public float driftStrengthVisualMultipler = 12;
		[Tooltip("How much is always drifting")]
		public float driftOffset = 1;
		public float visualDriftOffsetVisualMultipler = 10;
		[Tooltip("How fast does the visual mesh rotate to 'drift'")]
		public float driftEnableSpeed = .5f;
		[Tooltip("How fast does the visual mesh rotate out of 'drift'")]
		public float driftCancelSpeed = .5f;
		[Tooltip("How much horizontal input in the other direction to cancel drifting")]
		public float driftCancelHorizontalInput = .5f;
		[Tooltip("How fast the horizontal input changes normally")]
		public float limitedHorizontalChangeRate = 10f;
		[Tooltip("How fast the horizontal input changes when drifting")]
		public float limitedHorizontalChangeRateDrift = 3f;

		public float normalTraction = 15;
		public float driftTraction = 1;

		public float normalCameraRotation = 8;
		public float driftCameraRotation = 2;

		[Header("Telemetry")]
		public bool debug = true;
		//public float _dampVisualHorizontalMultipler = 0;

		public float _driftChangeSpeed = 0;

		private Damper dampDriftOffset = new Damper();
		private Damper dampDriftStrength = new Damper();


		public List<Damper> dampers;

		private void Start()
		{
			control = this.GetComponent<CarControllerV3>();
			input = this.GetComponent<InputController>();

			Debug.Assert(control != null, "Must have a controller");
			Debug.Assert(input != null, "Must have an input controller");
		}

		private void FixedUpdate()
		{
			// If drifting criteria fulfilled, then drift
			if (input.driftInput && input.vertical > inputThreshold && control.currentSpeed > minDriftSpeed && (input.horizontal != 0 || control.state == CarState.DRIFT) && control.wheelPower == 1)
			{
				StartDrift(); //This function changes _driftOffset and _driftStrength up to the target values
			}
			else if (control.state == CarState.DRIFT && input.vertical > inputThreshold && control.currentSpeed > minDriftSpeed && control.wheelPower == 1)
			{
				if ((driftingRight && input.horizontal > -driftCancelHorizontalInput) || (!driftingRight && input.horizontal < driftCancelHorizontalInput))
				{
					StartDrift();
				}
				else
				{
					StopDrift();
				}
			}
			else
			{
				StopDrift(); //Resets them back
			}

			CalculateHorizontalInput();
		}

		private void CalculateHorizontalInput()
		{
			input.horizontalInputDriftAdjusted = (input.horizontal * dampDriftStrength.value) + dampDriftOffset.value; //add _driftOffset to horizontal input 

			if (control.wheelPower == 1)
			{
				dampDriftOffset.Damp();
				dampDriftStrength.Damp();
			}
		}

		private void StartDrift()
		{
			_driftChangeSpeed = driftEnableSpeed;

			if (control.state != CarState.DRIFT)
			{
				if (Input.GetAxis("Horizontal") > 0)
				{
					driftingRight = true;
					dampDriftOffset.target = driftOffset + driftStrength;
				}
				else
				{
					driftingRight = false;
					dampDriftOffset.target = -(driftOffset + driftStrength);
				}

				//_dampVisualHorizontalMultiplerTarget = 1;
				dampDriftStrength.target = driftStrength;

				control.friction.ChangeDampTargetAndSpeed(driftTraction, driftEnableSpeed);
				control.cameraController.ChangeDampTargetAndSpeed(driftCameraRotation, driftEnableSpeed);
				//_limitedHorizontalChangeRate = limitedHorizontalChangeRateDrift;
			}

			if (Input.GetAxis("Horizontal") == 0) //Anoying workaround for when horizontal input is nothing
			{
				if (control.state == CarState.DRIFT)
					input.SetChangeRate(limitedHorizontalChangeRateDrift);
			}
			else if (Input.GetAxis("Horizontal") > 0)
			{
				if (!driftingRight)
				{
					input.SetChangeRate(limitedHorizontalChangeRate);
				}
				else
				{
					input.SetChangeRate(limitedHorizontalChangeRateDrift);
				}
			}
			else if (Input.GetAxis("Horizontal") < 0)
			{
				if (driftingRight)
				{
					input.SetChangeRate(limitedHorizontalChangeRate);
				}
				else
				{
					input.SetChangeRate(limitedHorizontalChangeRateDrift);
				}
			}

			dampDriftOffset.speedOfChange = driftEnableSpeed;
			dampDriftStrength.speedOfChange = driftEnableSpeed;
			control.state = CarState.DRIFT;
		}

		private void StopDrift()
		{
			_driftChangeSpeed = driftCancelSpeed;
			control.state = CarState.DRIVE;
			dampDriftOffset.target = 0;
			dampDriftOffset.speedOfChange = driftCancelSpeed;
			dampDriftStrength.target = 1;
			dampDriftStrength.speedOfChange = driftCancelSpeed;
			//_dampVisualHorizontalMultiplerTarget = 0;
			control.friction.ChangeDampTargetAndSpeed(normalTraction, driftCancelSpeed);
			control.cameraController.ChangeDampTargetAndSpeed(normalCameraRotation, driftCancelSpeed);

			input.SetChangeRate(limitedHorizontalChangeRate);
		}
	}

	public class Damper
	{
		public float value;
		public float velocity;
		public float target;
		public float speedOfChange;

		public Damper()
		{
			target = 0;
			value = 0;
			velocity = 0;
			speedOfChange = 0;
		}

		public void Damp()
		{
			value = Mathf.SmoothDamp(value, target, ref velocity, speedOfChange);
		}
	}
}