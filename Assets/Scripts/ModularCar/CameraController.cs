using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModularCar
{
	public class CameraController : MonoBehaviour
	{
		public Transform target;
		public GameObject car;
		public float distance = 3;
		public float height = 1;
		public float rotationDamping = 8;
		public float heighDamping = 8;
		public float zoomRatio = 4;
		public float defaultFOV = 60;
		public bool warpFOV = false;

		private float rotationVector;
		private Rigidbody rb;

		private void Start()
		{
			rb = car.GetComponent<Rigidbody>();
		}

		private void FixedUpdate()
		{
			Vector3 localVelocity = target.InverseTransformDirection(rb.velocity);
			var carPosition = car.transform.position + (rb.velocity * Time.deltaTime);
			var targetPosition = target.position + (rb.velocity * Time.deltaTime);

			if (localVelocity.z < -0.5f)
				rotationVector = target.eulerAngles.y + 180;
			else
				rotationVector = target.eulerAngles.y;

			float acceleration = rb.velocity.magnitude;

			if (warpFOV)
				Camera.main.fieldOfView = defaultFOV + acceleration + zoomRatio + Time.deltaTime;

			float wantedAngle = rotationVector;
			float wantedHeight = carPosition.y + height;
			float myAngle = transform.eulerAngles.y;
			float myHeight = transform.position.y;

			myAngle = Mathf.LerpAngle(myAngle, wantedAngle, rotationDamping * Time.deltaTime);
			myHeight = Mathf.LerpAngle(myHeight, wantedHeight, heighDamping * Time.deltaTime);

			Quaternion currentRotation = Quaternion.Euler(0, myAngle, 0);

			transform.position = carPosition;
			transform.position -= currentRotation * Vector3.forward * distance;

			Vector3 temp = transform.position;
			temp.y = myHeight;
			transform.position = temp;

			transform.LookAt(targetPosition);
		}
	}
}