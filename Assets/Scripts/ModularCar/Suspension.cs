using System;
using System.Collections.Generic;
using UnityEngine;

namespace ModularCar
{
	public class Suspension : MonoBehaviour
	{
		private CarControllerV3 control;
		private Rigidbody rb;
		
		public bool springsInitialized;
		private Spring[] springs;

		public float radius;
		
		public float maxSuspension = .05f;
		public float springy = 30000;
		public float damper = 6000;

		public Vector3 bounce;

		private void Start()
		{
			control = this.GetComponent<CarControllerV3>();
			rb = control.rb;
			radius = control.wheelRadius;

			Debug.Assert(control != null, "Must have a controller");
		}

		private void FixedUpdate()
		{
			if (!springsInitialized)
				InitializeSprings(control.wheels.ToArray());

			foreach (var spring in springs)
			{
				GetGround(spring);
			}

		}

		public void InitializeSprings(Wheel[] wheels)
		{
			if (springsInitialized)
			{
				throw new InvalidOperationException("Springs already initialized");
			}

			springs = new Spring[4];

			for (int i = 0; i < wheels.Length; i++)
			{
				springs[i] = new Spring(wheels[i].transform, wheels[i].mesh);
			}
			
			springsInitialized = true;
		}

		void GetGround(Spring spring)
		{
			Vector3 up = spring.transform.TransformDirection(Vector3.up);
			RaycastHit hit;

			// down = local downwards direction
			//Vector3 down = spring.transform.TransformDirection(Vector3.down);

			if (Physics.Raycast(spring.transform.position, -up, out hit, radius + maxSuspension))
			{
				
				// the velocity at point of contact
				Vector3 velocityAtTouch = rb.GetPointVelocity(hit.point);

				// calculate spring compression
				// difference in positions divided by total suspension range
				float compression = hit.distance / (maxSuspension + radius);
				compression = -compression + 1;

				// final force
				Vector3 force = up * compression * springy;

				// velocity at point of contact transformed into local space
				Vector3 t = spring.transform.InverseTransformDirection(velocityAtTouch);

				// local x and z directions = 0
				t.z = t.x = 0;

				// back to world space * -damping
				Vector3 damping = spring.transform.TransformDirection(t) * -damper;
				Vector3 finalForce = force + damping;

				bounce = finalForce;

				rb.AddForceAtPosition(finalForce, hit.point);
				
				spring.mesh.transform.position = spring.transform.position + (spring.transform.right * spring.offsetVector.x) + (spring.transform.forward * spring.offsetVector.z) + (-up * (hit.distance - radius));
				spring.mesh.transform.Rotate(0, 0, Mathf.Rad2Deg * (-control.currentSpeed / radius) * Time.deltaTime, Space.Self);

			}
			else
			{
				spring.mesh.transform.position = spring.transform.position + (spring.transform.right * spring.offsetVector.x) + (spring.transform.forward * spring.offsetVector.z) + (-up * maxSuspension);
				spring.mesh.transform.Rotate(0, 0, Mathf.Rad2Deg * (-control.currentSpeed / radius) * Time.deltaTime, Space.Self);
			}

		}

	}

	public class Spring
	{
		public Transform transform;
		public GameObject mesh;
		//I use this offset to calculate where the mesh is relative to the spring, so I can position the mesh correctly in the future
		public Vector3 offsetVector;

		public Spring(Transform spring, GameObject wheelMesh)
		{
			this.transform = spring;
			this.mesh = wheelMesh;
			offsetVector = mesh.transform.position - transform.position; 
		}
	}
}