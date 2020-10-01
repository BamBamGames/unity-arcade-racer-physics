using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace ModularCar
{
	public enum CarState { DRIVE, DRIFT }

	public class CarControllerV3 : MonoBehaviour
	{
		[Header("Car performance variables")]
		public float carGrip = 1;
		public float wheelRadius = .37f;
		[Tooltip("How much do wheel meshs rotate when steering")]
		public float visualWheelSteerRotation = 20;
		[Tooltip("This is so the car doesn't turn on the spot, it needs to be moving to start turning")]
		public float downforce = 100;
		public Transform steeringTransform;

		[Header("Telemetry")]
		public bool debug = true;
		public float currentSpeed;
		public float slidingAngle;

		public float wheelPower = 1;

		//Gameobjects
		public GameObject visualMesh;
		public List<Wheel> wheels;
		//public List<GameObject> wheelMeshs;
		public GameObject bodyMesh;
		public Rigidbody rb;
		public Suspension suspension;
		public Friction friction;
		public CameraController cameraController;
		public Vector3 something;
		public Quaternion somethingElse;

		void Awake()
		{
			rb = GetComponent<Rigidbody>();
			visualMesh = transform.Find("Mesh").gameObject;
			var com = transform.Find("CenterOfMass") as Transform;
			rb.centerOfMass = com.localPosition;
			//rb.inertiaTensor = new Vector3(5000, 5000, 1000);
			
			var wheelObjects = transform.GetComponentsInChildren<Wheel>();

			foreach (var wheel in wheelObjects)
			{
				wheels.Add(wheel);
			}

			cameraController = GameObject.Find("CarCamera").GetComponent<CameraController>();

			suspension = transform.GetComponent<Suspension>();
			friction = transform.GetComponent<Friction>(); ;

			something = rb.inertiaTensor;
			somethingElse = rb.inertiaTensorRotation;

			Debug.Assert(rb != null); //must be set
			Debug.Assert(visualMesh != null); //must be set
			//Debug.Assert(bodyMesh != null); //must be set
			Debug.Assert(wheels != null); //must be set
		}

		void FixedUpdate()
		{
			currentSpeed = transform.InverseTransformDirection(rb.velocity).z;

			WheelPower();
			Downforce();
			something = rb.inertiaTensor;

			//Debug stuff
			if (debug)
			{
				Debug.DrawRay(rb.position + (rb.velocity * Time.deltaTime), transform.up, Color.red);
				Debug.DrawLine(rb.position + (rb.velocity * Time.deltaTime), rb.position + rb.velocity, Color.green);
			}
		}

		
		private void WheelPower()
		{
			wheelPower = 0;
			for (int i = 0; i < wheels.Count; i++)
			{
				if (debug)
					Debug.DrawRay(wheels[i].transform.position + (rb.velocity * Time.deltaTime), -wheels[i].transform.up * wheelRadius, Color.green);

				if (wheels[i].IsGrounded())
				{
					wheelPower += .25f;
				}

				//if (Physics.Raycast(wheels[i].transform.position + (rb.velocity * Time.deltaTime), -wheels[i].transform.up, out RaycastHit hit, wheelRadius))
				//{
				//	if (i < 2)
				//	{
				//		//var turnAngle = Quaternion.AngleAxis(Mathf.Rad2Deg * _horizontal * (Mathf.Clamp(currentSpeed, 0, fullTurnSpeed) / fullTurnSpeed) * steeringCurve.Evaluate(currentSpeed) * visualWheelSteerRotation * Time.deltaTime, visualMesh.transform.up);
				//		//wheelMeshs[i].transform.localRotation = turnAngle;
				//	}
				//	//wheelMeshs[i].transform.Rotate(0, 0, Mathf.Rad2Deg * (-currentSpeed / wheelRadius) * Time.deltaTime, Space.Self);
				//	wheelPower += .25f;
				//}
			}
		}
		
		private void Downforce()
		{
			rb.AddForce(-transform.up * downforce * currentSpeed);
		}

		private void Grip()
		{
			var rigidBodyDirection = rb.velocity.normalized;
			var rigidBodyDirectionTarget = rb.transform.forward.normalized * Mathf.Sign(currentSpeed);
			slidingAngle = Mathf.Deg2Rad * Vector3.Angle(rigidBodyDirection, rigidBodyDirectionTarget);
		}
	}
}