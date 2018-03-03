using System.Collections.Generic;
using UnityEngine;

namespace InvincibleEngine.VektorLibrary.Utility {
	public class LowPassFloat {
		
		//Number of samples to average
		private int _sampleWindow = 8;
		
		//Collection of velocity samples over time
		private readonly Queue<float> _samples;	
		
		//Filter extreme deviations
		private bool _filterOutliers = false;
		
		//Extreme value threshold (current value < average * (1f + margin))
		private float _outlierMargin = 0.5f;
		
		//The last filtered value
		private float _lastValue;
		
		//Property: Max Samples
		public int SampleWindow {
			get { return _sampleWindow; }
			set { _sampleWindow = Mathf.Clamp(value, 1, int.MaxValue); }
		}
		
		//Property: Filter Outliers
		public bool FilterOutliers {
			get { return _filterOutliers; }
			set { _filterOutliers = value; }
		}
		
		//Property: Outlier Margin
		public float OutlierMargin {
			get { return _outlierMargin; }
			set { _outlierMargin = Mathf.Clamp(value, 0.001f, float.MaxValue); }
		}

		//Class Constructor (Default Settings)
		public LowPassFloat() {
			_samples = new Queue<float>();
		}
		
		//Class Constructor (Custom Sample Window)
		public LowPassFloat(int sampleWindow) {
			_sampleWindow = Mathf.Clamp(sampleWindow, 1, int.MaxValue);
			_samples = new Queue<float>();
		}
		
		//Class Constructor (Full Custom Settings)
		public LowPassFloat(int sampleWindow, bool filterOutliers, float outlierMargin) {
			_sampleWindow = Mathf.Clamp(sampleWindow, 1, int.MaxValue);
			_filterOutliers = filterOutliers;
			_outlierMargin = Mathf.Clamp(outlierMargin, 0.001f, float.MaxValue);
			_samples = new Queue<float>();
		}
		
		/// <summary>
		/// Get a filtered value from the Low-Pass filter
		/// </summary>
		/// <param name="current">The current value to be filtered</param>
		/// <returns></returns>
		public float GetFilteredValue(float current) {
			//Cycle the sample queue
			if (_samples.Count + 1 > _sampleWindow) _samples.Dequeue();
			
			//Outlier filtering
			if (_filterOutliers && _lastValue > 0f) {
				//Check if new value is an outlier
				if (current > _lastValue * (1f + _outlierMargin)) {
					//Discard the outlier, add lastValue * (1f + margin)
					_samples.Enqueue(_lastValue * (1f + _outlierMargin));
				}
				else {
					//Value is not an outlier, add new value
					_samples.Enqueue(current);
				}
			}
			else {
				//No filtering, add new value
				_samples.Enqueue(current);
			}
			
			//Average the samples
			var sum = 0f;
			foreach (var sample in _samples) {
				sum += sample;
			}
			
			//Return the average
			_lastValue = sum / _samples.Count;
			return _lastValue;
		}
		
		/// <summary>
		/// Flushes all stored sample data
		/// </summary>
		public void ClearSamples() {
			_samples.Clear();
		}
	}
}
