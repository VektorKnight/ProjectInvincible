using System;
using System.Collections.Generic;

namespace VektorLibrary.Math {
	/// <summary>
	/// An implementation of a low-pass filter based on a moving average.
	/// Designed to smooth out sudden changes in a value such as those produced by some mobile sensors and HID devices.
	/// Original Author: VektorKnight
	/// </summary>
	public class LowPassFloat {	
		// Size of the sample buffer
		private int _bufferSize;
		
		// Collection of velocity samples over time
		private readonly Queue<float> _buffer;

		// Extreme value threshold (current value < average * (1f + margin))
		private float _outlierMargin;
		
		// Property: Sample Window
		public int BufferSize {
			get { return _bufferSize; }
			set {
				if (value <= 0) throw new ArgumentException("Buffer size must be greater than zero!");
				_bufferSize = value;
			}
		}
		
		// Property: Filter Outliers
		public bool FilterOutliers { get; set; }

		// Property: Outlier Margin
		public float OutlierMargin {
			get { return _outlierMargin; }
			set {
				if (value <= 0) throw new ArgumentException("Outlier margin must be greater than zero!");
				_outlierMargin = value;
			}
		}
		
		// Property: Current Output Value
		public float Output { get; private set; }
		
		// Property: Buffer as float[]
		public float[] Samples => _buffer.ToArray();
		
		// Implicit Cast: Float
		public static implicit operator float(LowPassFloat lpf) {
			return lpf.Output;
		}
		
		// Implicit Cast: Int
		public static implicit operator int(LowPassFloat lpf) {
			return (int)lpf.Output;
		}

		// Class Constructor (Default Settings)
		public LowPassFloat() {
			_buffer = new Queue<float>();
		}
		
		// Class Constructor
		public LowPassFloat(int bufferSize = 16, bool filterOutliers = false, float outlierMargin = 0.5f) {
			// Sanity checks
			if (bufferSize <= 0) throw new ArgumentException("Buffer size must be greater than zero!");
			if (outlierMargin <= 0) throw new ArgumentException("Outlier margin must be greater than zero!");
			
			// Initialize
			_bufferSize = bufferSize;
			FilterOutliers = filterOutliers;
			_outlierMargin = outlierMargin;
			_buffer = new Queue<float>(bufferSize);
		}

		/// <summary>
		/// Add a single sample to the buffer.
		/// </summary>
		/// <param name="value">The sample to add.</param>
		/// <returns></returns>
		public void AddSample(float value) {
			// Cycle the sample queue
			if (_buffer.Count + 1 > _bufferSize) _buffer.Dequeue();
			
			// Outlier filtering
			if (FilterOutliers && Output > 0f) {
				// Check if new value is an outlier
				if (value > Output * (1f + _outlierMargin)) {
					// Discard the outlier, add lastValue * (1f + margin)
					_buffer.Enqueue(Output * (1f + _outlierMargin));
				}
				else {
					// Value is not an outlier, add new value
					_buffer.Enqueue(value);
				}
			}
			else {
				// No filtering, add new value
				_buffer.Enqueue(value);
			}
			
			// Average the samples within the buffer
			var sum = 0f;
			foreach (var sample in _buffer) {
				sum += sample;
			}
			
			// Calculate the new output value
			Output = sum / _buffer.Count;
		}
		
		/// <summary>
		/// Add multiple samples to the buffer at once.
		/// </summary>
		/// <param name="values">The samples to add.</param>
		public void AddSamples(float[] values) {
			foreach (var value in values) {
				AddSample(value);
			}
		}
		
		/// <summary>
		/// Flushes all stored sample data from the buffer and resets the output.
		/// </summary>
		public void Reset() {
			_buffer.Clear();
			Output = 0f;
		}
	}
}
