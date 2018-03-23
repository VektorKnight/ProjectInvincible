using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

public class Test : MonoBehaviour {

    // Use this for initialization
    async void Start() {
        var stopWatch = new Stopwatch();
        UnityEngine.Debug.Log($"Starting threaded task...");
        stopWatch.Start();

        await Task.Run(() => {
            for (var i = 0; i < 1024; i++) UnityEngine.Debug.Log("Hello from threaded task!");
        });

        stopWatch.Stop();
        UnityEngine.Debug.Log($"Threaded task completed in {stopWatch.ElapsedMilliseconds}ms");
    }

    // Update is called once per frame
    void Update () {
		
	}
}
