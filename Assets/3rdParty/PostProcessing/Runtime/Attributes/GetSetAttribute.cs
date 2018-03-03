using UnityEngine;

namespace _3rdParty.PostProcessing.Runtime.Attributes
{
    public sealed class GetSetAttribute : PropertyAttribute
    {
        public readonly string name;
        public bool dirty;

        public GetSetAttribute(string name)
        {
            this.name = name;
        }
    }
}
