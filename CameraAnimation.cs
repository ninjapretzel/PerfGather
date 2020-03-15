using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using static UnityEngine.Mathf;

namespace PerfGather {
	/// <summary> Class which creates a camera animation. 
	/// Used with <see cref="PerfSim"/>. </summary>
	public class CameraAnimation : MonoBehaviour {
		
		/// <summary> Control for if the animation should play </summary>
		public bool go = false;
		/// <summary> Current time in the animation </summary>
		public float timeout = 0;

		/// <summary> Distance to move along +z per second </summary>
		public float forwardSpeed = 4.0f;
		/// <summary> Controls how much 'wobble' along forward direction is applied </summary>
		public float accelWobble = 1.5f;
		/// <summary> Controls how fast 'wobble' along forward direction is applied </summary>
		public float accelWobbleRate = 1.5f;
		/// <summary> Radius of x/y axis movement </summary>
		public float radius = 1.0f;
		/// <summary> Rate of x/y axis movement in radians per second </summary>
		public float circleRate = 1.0f;
		/// <summary> Controls how quickly the camera looks around </summary>
		public float lookRate = 1.0f;
		/// <summary> Controls how much the camera look 'wobbles' around </summary>
		public float lookWobble = 1.0f;
		/// <summary> Controls how quickly the camera look 'wobbles' around </summary>
		public float lookWobbleRate = 1.5f;

		/// <summary> Called by UnityEngine automatically every frame </summary>
		void Update() {
			if (go) {
				timeout += Time.unscaledDeltaTime;
				transform.position = new Vector3(radius * Cos(timeout * circleRate),
												radius * Sin(timeout * circleRate),
												timeout * forwardSpeed + accelWobble * Sin(timeout * accelWobbleRate));

				// Clamp to prevent div/0
				if (lookWobble < .1f) { lookWobble = .1f; } 
				Vector3 offset = new Vector3(-Sin(timeout * lookRate),
											Cos(timeout * lookRate),
											(1.0f / lookWobble) * (4 + 2 * Cos(timeout * lookWobbleRate)));

				transform.LookAt(transform.position + offset);
			}
		}

		/// <summary> Called by <see cref="PerfSim"/> when the all of the tests are completed. </summary>
		void TestsCompleted() {
			go = false;
		}

		/// <summary> Called by <see cref="PerfSim"/> when the next test starts. </summary>
		void TestStarted(int testNum) {
			go = true;
			timeout = 0;
		}

	}
}
