using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ModularCar
{
	public class Wheel : MonoBehaviour
	{
		private CarControllerV3 control;
		private Rigidbody rb;

		private float radius;
		private float maxSuspensionLength;

		public GameObject mesh;
		private TrailRenderer skid;
		private Vector3 hitPosition;
		public float maxSlideAngle = .1f;
		public float minimumSlideSpeed = 10;
		private float slideAngle;

		private void Start()
		{
			control = this.GetComponentInParent<CarControllerV3>();
			rb = control.rb;
			radius = control.wheelRadius;

			if (control.suspension != null)
				maxSuspensionLength = control.suspension.maxSuspension;

			skid = GetComponentInChildren<TrailRenderer>();
			mesh = transform.Find("Mesh").gameObject;

			Debug.Assert(control != null, "Must have a controller");
			Debug.Assert(mesh != null, "Must have a mesh");
			Debug.Assert(skid != null, "Must have a trail");
		}

		private void Update()
		{
			RotateWheels();
			Sliding();
			Skidmarks();
		}

		private void FixedUpdate()
		{
			
		}

		public bool IsGrounded()
		{
			//Need to update hit position every frame, otherwise it leads to some weirdness
			if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, radius + maxSuspensionLength))
			{
				hitPosition = hit.point + (.05f * transform.up);
				return true;
			}
			else
			{
				hitPosition = transform.position + (-transform.up * radius);
				return false;
			}
		}

		private void RotateWheels()
		{
			mesh.transform.Rotate(0, 0, Mathf.Rad2Deg * (-control.currentSpeed / radius) * Time.deltaTime, Space.Self);
		}

		private void Skidmarks()
		{
			skid.emitting = IsGrounded() && IsSliding() && control.currentSpeed > 10;
			skid.transform.position = hitPosition;
		}

		private void Sliding()
		{
			var rbDirection = rb.velocity.normalized;
			slideAngle = Mathf.Deg2Rad * (Vector3.Angle(rbDirection, transform.forward.normalized) - 90);
		}

		private bool IsSliding()
		{
			return Mathf.Abs(slideAngle) >= maxSlideAngle;
		}
	}
}