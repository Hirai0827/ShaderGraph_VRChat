using UnityEngine;
using UnityEditor.Graphing;

namespace UnityEditor.ShaderGraph
{
    [Title("Input", "Geometry", "Vertex ID")]
    class VertexIDNode : AbstractMaterialNode, IMayRequireVertexID
    {
        private const int kOutputSlotId = 0;
        private const string kOutputSlotName = "Out";

        public override bool hasPreview { get { return true; } }

        public VertexIDNode()
        {
            name = "Vertex ID";
            UpdateNodeAfterDeserialization();
        }
        
        public override PreviewMode previewMode
        {
            get { return PreviewMode.Preview3D; }
        }

        public sealed override void UpdateNodeAfterDeserialization()
        {
            AddSlot(new Vector1MaterialSlot(kOutputSlotId, kOutputSlotName, kOutputSlotName, SlotType.Output, (int)0, ShaderStageCapability.All));
            RemoveSlotsNameNotMatching(new[] { kOutputSlotId });
        }

        public override string GetVariableNameForSlot(int slotId)
        {
            return string.Format("IN.{0}", ShaderGeneratorNames.VertexID);
        }

        public bool RequiresVertexID(ShaderStageCapability stageCapability)
        {
            return true;
        }
    }
}