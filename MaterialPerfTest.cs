using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace PerfGather {
	/// <summary> Class that is used to change the material when perf test starts. 
	/// Used with <see cref="PerfSim"/></summary>
	public class MaterialPerfTest : MonoBehaviour {

		/// <summary> Inner class to hold individual changes to apply </summary>
		[Serializable] public class Change {
			/// <summary> Property name of value to change </summary>
			public string propName;
			/// <summary> Property value to change </summary>
			public float propValue;
		}
		/// <summary> Base material settings </summary>
		public Material baseMaterial;
		/// <summary> List of changes to apply </summary>
		public Change[] changes;
		/// <summary> Target to change the material of during tests </summary>
		public Renderer target;

		/// <summary> Called by UnityEngine automatically when the object loads. </summary>
		void Awake() {
			PerfSim sim = GetComponent<PerfSim>();
			if (sim != null) {
				sim.numTests = (int)Mathf.Pow(2, changes.Length);
			}
		}

		/// <summary> Called by <see cref="PerfSim"/> when the next test starts. </summary>
		void TestStarted(int testNum) {
			if (target != null) {
				target.sharedMaterial = MakeMaterial(testNum);
			}
		}

		/// <summary> Helper to determine if the given <paramref name="bit"/> is on in the given <paramref name="value"/>. </summary>
		static bool BitOn(int bit, int value) {
			return (value & (1 << bit)) != 0;
		}

		/// <summary> Creates the material for the current test based on the bits that are on. </summary>
		/// <param name="materialIndex"> Test index to make material for </param>
		/// <returns> Copy of <see cref="baseMaterial"/> with changes applied to it for the current test. </returns>
		Material MakeMaterial(int materialIndex) {
			Material copy = new Material(baseMaterial);

			for (int i = 0; i < changes.Length; i++) {
				if (BitOn(i, materialIndex)) {
					Change c = changes[i];
					copy.SetFloat(c.propName, c.propValue);
				}
			}

			return copy;
		}
	
	}
}
