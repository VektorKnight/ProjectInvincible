using UnityEngine;

namespace _3rdParty.PostProcessing.Runtime.Attributes
{
    public sealed class TrackballAttribute : PropertyAttribute
    {
        public readonly string method;

        public TrackballAttribute(string method)
        {
            this.method = method;
        }
    }
}
