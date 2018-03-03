using System;
using System.Text;
using UnityEngine;

namespace InvincibleEngine.VektorLibrary.Utility {
	public class CompressionTest : MonoBehaviour {

		// Use this for initialization
		void Start () {
			var testData = new ChatMessage();
			var rawBinary = Serialization.SerializeToBytes(testData);
			var gzipBinary = Serialization.CompressBytesGzip(rawBinary);
			var lzfBinary = Serialization.CompressBytesLzf(rawBinary);
			
			var jsonString = JsonUtility.ToJson(testData);
			var jsonBytes = Encoding.UTF8.GetBytes(jsonString.ToCharArray());
			var gzipJson = Serialization.CompressBytesGzip(jsonBytes);
			var lzfJson = Serialization.CompressBytesLzf(jsonBytes);
			
			Debug.Log($"Binary: {rawBinary.Length}\n" +
			          $"Gzip Binary: {gzipBinary.Length}");
			Debug.Log($"Lzf Binary: {lzfBinary.Length}");
			
			Debug.Log($"Json String: {JsonUtility.ToJson(testData).Length}\n" +
			          $"Gzip Json: {gzipJson.Length}");
			Debug.Log($"Lzf Json: {lzfJson.Length}");
		}
	
		// Update is called once per frame
		void Update () {
		
		}
	}
	
	// test class for serialization
	[Serializable]
		public class ChatMessage {
		public string Message = "This is a test message. Something remotely similar to what a player might send. Bacon is fucking good m8!";
		public int PlayerId = 1337, A = 874764325, B = 746252968, C = 637285276, D = 932131877;
		public long TimeStamp = 7436271237144;
		public long Blarg = 4387458723487248675;
	}
}
