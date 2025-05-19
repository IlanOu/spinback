using Nenn.InspectorEnhancements.Runtime.Attributes;
using UnityEngine;

namespace Object.InvestigationReport
{
    [System.Serializable]
    public class InvestigationData
    {
        [HideInInspector] public string id;
        
        [HideLabel] public string description;
    }
}