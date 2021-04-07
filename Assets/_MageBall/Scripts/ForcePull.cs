using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
namespace MageBall
{

	public class ForcePull : MonoBehaviour
	{
		
		private Transform player; //destination of pull
		private string pullableTag = "pullable";

		[SerializeField] //remove serializeField when you found a good number
		private float modifier = 1.0f;
		Vector3 pullForce; //direction of the force that pulls

		[Tooltip("The distance threshold in which the object is considered pulled to the hand")]
		public float positionDistanceThreshold;

		[Tooltip("The distance threshold in which the object's velocity is set to maximum")]
		public float velocityDistanceThreshold;

		[Tooltip("The maximum velocity of the object being pulled")]
		public float maxVelocity;


		void Update()
		{
			RaycastHit hit;
			if (Input.GetMouseButtonDown(0))
			{
				if (Physics.Raycast(transform.position, transform.forward, out hit, Mathf.Infinity))
				{
					if (hit.transform.tag.Equals(pullableTag))
					{
						StartCoroutine(PullObject(hit.transform));
					}
				}
			}
		}

        private void Start()
        {
			player = FindObjectOfType<LocalPlayer>().transform;
        }

        IEnumerator PullObject(Transform t)
		{
			Rigidbody r = t.GetComponent<Rigidbody>();
			while (true)
			{

				//	If the player right-clicks, stop pulling
				if (Input.GetMouseButtonDown(1))
				{
					break;
				}
				float distanceToHand = Vector3.Distance(t.position, player.position);
				/*
					If the object is withihn the distance threshold, consider it pulled all the way and:
					1) Set the object's position to the hand position
					2) make it's parent be the hand object
					3) Constrain its movement, but not rotation
					4) Set its velocity to be the forward vector of the camera * the throw velocity
					5) Break out of the coroutine
				*/
				if (distanceToHand < positionDistanceThreshold)
				{
					t.position = player.position;
					t.parent = player;
					r.constraints = RigidbodyConstraints.FreezePosition;
					break;
				}

				//	Calculate the pull direction vector
				Vector3 pullDirection = player.position - t.position;

				//	Normalize it and multiply by the force modifier
				pullForce = pullDirection.normalized * modifier;

				/*
					Check if the velocity magnitude of the object is less than the maximum velocity
					and
					check if the distance to hand is greater than the distance threshold
				*/
				if (r.velocity.magnitude < maxVelocity && distanceToHand > velocityDistanceThreshold)
				{

					//	Add force that takes the object's mass into account
					r.AddForce(pullForce, ForceMode.Force);
				}
				else
				{

					// Set a constant velocity to the object
					r.velocity = pullDirection.normalized * maxVelocity;
				}

				yield return null;
			}
		}
	}
}