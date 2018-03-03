using System.Collections.Generic;
using UnityEngine;

namespace InvincibleEngine.VektorLibrary.Utility {
    //Weighted Generator
    public class WeightedGenerator {

        //Weighted List
        private List<int> _weightedList;

        //Constructor: Basic Weighted Value
        public WeightedGenerator(List<WeightedValue> sourceList) {
            _weightedList = new List<int>();

            //Iterate through the Source List
            for (int i = 0; i < sourceList.Count; i++) {
                //Add IndexOf(_sourceList[i]) to Weighted List (t) times
                int t = Mathf.RoundToInt(sourceList.Count * 100 * (sourceList[i].Weight / 100));
                for (int j = 0; j < t; j++) {
                    _weightedList.Add(i);
                }
            }
        }

        //Constructor: Basic Weighted String
        public WeightedGenerator(List<WeightedString> sourceList) {
            _weightedList = new List<int>();

            //Iterate through the Source List
            for (int i = 0; i < sourceList.Count; i++) {
                //Add IndexOf(_sourceList[i]) to Weighted List (t) times
                int t = Mathf.RoundToInt(sourceList.Count * 100 * (sourceList[i].Weight / 100));
                for (int j = 0; j < t; j++) {
                    _weightedList.Add(i);
                }
            }
        }

        //Constructor: Basic Weighted Object
        public WeightedGenerator(List<WeightedObject> sourceList) {
            _weightedList = new List<int>();

            //Iterate through the Source List
            for (int i = 0; i < sourceList.Count; i++) {
                //Add IndexOf(_sourceList[i]) to Weighted List (t) times
                int t = Mathf.RoundToInt(sourceList.Count * 100 * (sourceList[i].Weight / 100));
                for (int j = 0; j < t; j++) {
                    _weightedList.Add(i);
                }
            }
        }

        //Return a Weighted Random Value based on the Weighted List
        public int GetWeightedRandom() {
            return _weightedList[Random.Range(0, _weightedList.Count)];
        }
    }

    //Chunk Config Struct
    [System.Serializable]
    public struct WeightedObject {
        public GameObject Object;
        public float Weight;
    }

    //Generic Weighted Value
    [System.Serializable]
    public struct WeightedValue {
        public int Value;
        public float Weight;
    }

    //Generic Weighted String
    [System.Serializable]
    public struct WeightedString {
        public string String;
        public float Weight;
    }

    //Generator Config Struct
    [System.Serializable]
    public struct GeneratorConfig {
        public List<WeightedObject> Chunks;
        public string CustomSeed;
        public int GenerateDistance;
        public int MaxChunks;
        public int ChunkPoolSize;
    }
}

