using UnityEngine;

namespace _3rdParty.PostProcessing.Runtime.Attributes
{
    public sealed class MinAttribute : PropertyAttribute
    {
        public readonly float min;

        public MinAttribute(float min)
        {
            this.min = min;
        }
    }
}
