using System;
using UnityEngine;

namespace ModularCar
{
	public class Suspension : MonoBehaviour
	{
		private CarControllerV3 control;
		private Rigidbody rb;

		public bool debug;
		
		public bool springsInitialized;
		private Spring[] springs;
		public float mass = 1;
		public float radius = .4f;
		public float maxSuspension = .05f;
		public float springy = 30000;
		public float damper = 3000;

		public float springFLForce;
		public float springFRForce;
		public float springBLForce;
		public float springBRForce;

		private void Start()
		{
			control = this.GetComponent<CarControllerV3>();
			rb = control.rb;

			InitializeSprings(control.wheels.ToArray(), control.wheelMeshs.ToArray());

			Debug.Assert(control != null, "Must have a controller");
		}

		private void FixedUpdate()
		{
			if (springsInitialized)
			{
				foreach (var spring in springs)
				{
					GetGround(spring);
				}
			}
		}

		public void InitializeSprings(Transform[] wheels, GameObject[] wheelMeshs)
		{
			if (springsInitialized)
			{
				throw new InvalidOperationException("Springs already initialized");
			}

			springs = new Spring[4];

			for (int i = 0; i < wheels.Length; i++)
			{
				springs[i] = new Spring(wheels[i], wheelMeshs[i]);
			}
			
			springsInitialized = true;
		}

		void GetGround(Spring spring)
		{
			Vector3 downwards = spring.transform.TransformDirection(-Vector3.up);
			RaycastHit hit;

			// down = local downwards direction
			Vector3 down = spring.transform.TransformDirection(Vector3.down);

			if (Physics.Raycast(spring.transform.position, downwards, out hit, radius + maxSuspension))
			{
				// the velocity at point of contact
				Vector3 velocityAtTouch = rb.GetPointVelocity(hit.point);

				// calculate spring compression
				// difference in positions divided by total suspension range
				float compression = hit.distance / (maxSuspension + radius);
				compression = -compression + 1;

				// final force
				Vector3 force = -downwards * compression * springy;
				// velocity at point of contact transformed into local space

				Vector3 t = spring.transform.InverseTransformDirection(velocityAtTouch);

				// local x and z directions = 0
				t.z = t.x = 0;

				// back to world space * -damping
				Vector3 damping = spring.transform.TransformDirection(t) * -damper;
				Vector3 finalForce = force + damping;

				rb.AddForceAtPosition(finalForce, hit.point);

				spring.mesh.transform.position = spring.transform.position + (down * (hit.distance - radius));
				spring.mesh.transform.Rotate(0, 0, Mathf.Rad2Deg * (-control.currentSpeed / radius) * Time.deltaTime, Space.Self);
				//if (graphic) graphic.position = transform.position + (down * (hit.distance - radius));

			}
			else
			{
				//if (graphic) graphic.position = transform.position + (down * maxSuspension);
			}

		}

	}


	public struct Spring
	{
		public Transform transform;
		public GameObject mesh;

		public Spring(Transform spring, GameObject wheelMesh)
		{
			this.transform = spring;
			this.mesh = wheelMesh;
		}
	}
}