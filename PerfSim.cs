using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Ex;

namespace PerfGather {

	public class PerfSim : MonoBehaviour {

		/// <summary> Duration of each test, in seconds. </summary>
		public float testDuration = 5;
		/// <summary> Number of tests to run. Simulation resets this many times. </summary>
		public int numTests = 8;
		/// <summary> Capacity used when creating a trender. 
		/// Total number of samples considered when making averages
		/// or calculating min/max. </summary>
		public int trenderCapacity = 16;
		/// <summary> Real time (in seconds) per fps sample. 
		/// With high enough FPS, this will be more consistant 
		/// than sampling every frame </summary>
		public float timePerSample = 1.0f / 60.0f;

		/// <summary> Trender instances per test so they don't interfere</summary>
		[NonSerialized] public List<Trender> trenders;
		/// <summary> Recent Averages samples </summary>
		[NonSerialized] public List<List<double>> averages;
		/// <summary> Samples at sample times </summary>
		[NonSerialized] public List<List<double>> samples;
		/// <summary> Samples of recent minimums  </summary>
		[NonSerialized] public List<List<double>> mins;
		/// <summary> Samples of recent maximums</summary>
		[NonSerialized] public List<List<double>> maxs;
	
		/// <summary> Current test index </summary>
		private int testNum = -1;
		/// <summary> Timeout for next test </summary>
		private float testTimeout = 0;
		/// <summary> Timeout for next sample </summary>
		private float sampleTimeout = 0;
		/// <summary> All tests done? </summary>
		private bool finished = false;

		/// <summary> Time to warm up for before testing 
		/// (to allow load frames to pass and stability to be reached) </summary>
		public float warmtime = 1;

		/// <summary> FPS of last frame to show in UI </summary>
		private double fps;

		/// <summary> Called by UnityEngine automatically when the object loads. </summary>
		void Awake() {
			trenders = new List<Trender>();
			averages = new List<List<double>>();
			samples = new List<List<double>>();
			mins = new List<List<double>>();
			maxs = new List<List<double>>();
			Application.targetFrameRate = 9999;
			Time.timeScale = 1.0f;
		}


		/// <summary> Called by UnityEngine automatically when the GUI needs to be updated. </summary>
		void OnGUI() {
			string str = $"FPS: {fps:F2}";
			if (testNum >= 0 && testNum < numTests) {
				str += $"\nTest #{testNum}\nAVG: {trenders[testNum].average:F2}";
			}
			GUI.Label(new Rect(0,0,200,100), str);
		}

		/// <summary> Called by UnityEngine automatically every frame </summary>
		void Update() {
			fps = 1.0 / Time.unscaledDeltaTime;

			if (warmtime > 0) {
				warmtime -= Time.unscaledDeltaTime;
				if (warmtime < 0) {
					NextTest();
				}
				return;
			}
			
			if (!finished) {
				trenders[testNum].Record(fps);
				sampleTimeout += Time.unscaledDeltaTime;
				
				if (sampleTimeout > timePerSample) {
					averages[testNum].Add(trenders[testNum].average);
					mins[testNum].Add(trenders[testNum].min);
					maxs[testNum].Add(trenders[testNum].max);
					samples[testNum].Add(fps);
				}

				testTimeout += Time.unscaledDeltaTime;
				if (testTimeout > testDuration) {
					NextTest();
				}
			}
		}

		/// <summary> Move Simulation to next test, broadcast "TestStarted" message, or "TestsCompleted" if finished.. </summary>
		void NextTest() {
			if (testNum < numTests-1) {
				testNum++;

				trenders.Add(new Trender(trenderCapacity));
				averages.Add(new List<double>());
				samples.Add(new List<double>());
				mins.Add(new List<double>());
				maxs.Add(new List<double>());

				// Logging has a lot of overhead in Unity, as it forces GUI to redraw.
				// Commented for actual testing.
				//Debug.Log($"Test {testNum} starting!");

				gameObject.BroadcastMessage("TestStarted", testNum, SendMessageOptions.DontRequireReceiver);
				testTimeout = 0;

			} else {
				finished = true;

				// Logging has a lot of overhead in Unity, as it forces GUI to redraw.
				// Commented for actual testing.
				//Debug.Log($"Tests completed!");

				gameObject.BroadcastMessage("TestsCompleted", SendMessageOptions.DontRequireReceiver);

				TestFinished();
			}
		}
		
		/// <summary> Calculate test results and write out some files. </summary>
		void TestFinished() {
			JsonArray output = new JsonArray();
			JsonArray preppedSmooth = new JsonArray();
			JsonArray preppedRaw = new JsonArray();
			JsonArray preppedMax = new JsonArray();
			JsonArray preppedMin = new JsonArray();

			for (int i = 0; i < numTests; i++) {
				JsonObject element = new JsonObject();
				output.Add(element);

				var avgs = averages[i];
				var smps = samples[i];
				var max = maxs[i];
				var min = mins[i];

				var smooth = Average(avgs);
				var raw = Average(smps);
				var mi = Average(min);
				var ma = Average(max);

				element["smoothAverage"] = smooth;
				element["rawAverage"] = raw;
				element["max"] = Max(max);
				element["min"] = Min(min);

				preppedSmooth.Add(new JsonArray(smooth));
				preppedRaw.Add(new JsonArray(raw));
				preppedMin.Add(new JsonArray(mi));
				preppedMax.Add(new JsonArray(ma));

				//element["averages"] = Json.Reflect(avgs);
				//element["samples"] = Json.Reflect(smps);
				//element["maxs"] = Json.Reflect(max);
				//element["mins"] = Json.Reflect(min);
			}
			DateTime now = DateTime.UtcNow;
			long timestamp = now.UnixTimestamp();
			File.WriteAllText($"PerfTests/{timestamp}-perftest.json", output.PrettyPrint());
			File.WriteAllText($"PerfTests/{timestamp}-preppedSmooth.json", preppedSmooth.PrettyPrint());
			File.WriteAllText($"PerfTests/{timestamp}-preppedRaw.json", preppedRaw.PrettyPrint());
			File.WriteAllText($"PerfTests/{timestamp}-preppedMin.json", preppedMin.PrettyPrint());
			File.WriteAllText($"PerfTests/{timestamp}-preppedMax.json", preppedMax.PrettyPrint());

			AudioSource src = GetComponent<AudioSource>();
			if (src != null) {
				src.Play();
			}
		}
		/// <summary> Helper method to get the max value in a list </summary>
		static double? Max(List<double> vals) {
			if (vals.Count == 0) { return null; }
			var max = double.MinValue;
			for (int i = 0; i < vals.Count; i++) {
				if (vals[i] > max) { max = vals[i]; }
			}
			return max;
		}
		/// <summary> Helper method to get the min value in a list </summary>
		static double? Min(List<double> vals) {
			if (vals.Count == 0) { return null; }
			var min = double.MaxValue;
			for (int i = 0; i < vals.Count; i++) {
				if (vals[i] < min) { min = vals[i]; }
			}
			return min;

		}
		/// <summary> Helper method to get the average of values in a list </summary>
		static double Average(List<double> vals) {
			if (vals.Count == 0) { return 0; }
			double sum = 0;
			for (int i = 0; i < vals.Count; i++) { sum += vals[i]; }
			return sum / vals.Count;
		}
	}
}
