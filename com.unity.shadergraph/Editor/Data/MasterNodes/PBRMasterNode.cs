using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor.Graphing;
using UnityEditor.ShaderGraph.Drawing;
using UnityEditor.ShaderGraph.Drawing.Controls;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityEditor.ShaderGraph
{
    [Serializable]
    [Title("Master", "PBR")]
    class PBRMasterNode : MaterialMasterNode<IPBRSubShader>, IMayRequirePosition, IMayRequireNormal, IMayRequireTangent
    {
        public const string AlbedoSlotName = "Albedo";
        public const string NormalSlotName = "Normal";
        public const string EmissionSlotName = "Emission";
        public const string MetallicSlotName = "Metallic";
        public const string SpecularSlotName = "Specular";
        public const string SmoothnessSlotName = "Smoothness";
        public const string OcclusionSlotName = "Occlusion";
        public const string AlphaSlotName = "Alpha";
        public const string AlphaClipThresholdSlotName = "AlphaClipThreshold";
        public const string PositionName = "Vertex Position";
        public const string NormalName = "Vertex Normal";
        public const string TangentName = "Vertex Tangent";
        public const string ColorName = "Vertex Color";
        public const string AlphaName = "Vertex Alpha";


        public const int AlbedoSlotId = 0;
        public const int NormalSlotId = 1;
        public const int MetallicSlotId = 2;
        //public const int SpecularSlotId = 3;
        public const int EmissionSlotId = 4;
        public const int SmoothnessSlotId = 5;
        public const int OcclusionSlotId = 6;
        public const int AlphaSlotId = 7;
        public const int AlphaThresholdSlotId = 8;
        public const int PositionSlotId = 9;
        public const int VertNormalSlotId = 10;
        public const int VertTangentSlotId = 11;

        public const string ReflectanceName = "Reflectance";
        public const int ReflectanceSlotID = 12;
        
        public const string GSAAVarienceName = "GSAA Variance";
        public const int GSAAVarienceSlotID = 13;
        public const string GSAAThresholdName = "GSAA Threshold";
        public const int GSAAThresholdSlotID = 14;
        
        public const string AnisotropyTangentName = "Tangent";
        public const int AnisotropyTangentSlotID = 15;
        public const string AnisotropyLevelName = "Anisotropy";
        public const int AnisotropyLevelSlotID = 16;
        
        public const int VertColorSlotID = 17;
        public const int VertAlphaSlotID = 18;




        public enum Model
        {
            Specular,
            Metallic
        }
        
        private void UpdateSettingValue(ref bool value, bool newValue)
        {
            if (value == newValue)
                return;

            value = newValue;
            UpdateNodeAfterDeserialization();
            Dirty(ModificationScope.Graph);
        }

        [SerializeField] private bool m_BicubicLightmap = false;
        [SerializeField] private bool m_Gsaa = false;
        [SerializeField] private bool m_Anisotropy = false;
        [SerializeField] private bool m_FlatLit = false;

        public bool flatLit
        {
            get => m_FlatLit;
            set => UpdateSettingValue(ref m_FlatLit, value);
        }
        public bool anisotropy
        {
            get => m_Anisotropy;
            set => UpdateSettingValue(ref m_Anisotropy, value);
        }

        public bool gsaa
        {
            get => m_Gsaa;
            set => UpdateSettingValue(ref m_Gsaa, value);
        }
        
        public bool bicubicLightmap
        {
            get => m_BicubicLightmap;
            set => UpdateSettingValue(ref m_BicubicLightmap, value);
        }

        [SerializeField]
        Model m_Model = Model.Metallic;

        public Model model
        {
            get { return m_Model; }
            set
            {
                if (m_Model == value)
                    return;

                m_Model = value;
                UpdateNodeAfterDeserialization();
                Dirty(ModificationScope.Topological);
            }
        }

        [SerializeField]
        SurfaceType m_SurfaceType;

        public SurfaceType surfaceType
        {
            get { return m_SurfaceType; }
            set
            {
                if (m_SurfaceType == value)
                    return;

                m_SurfaceType = value;
                Dirty(ModificationScope.Graph);
            }
        }

        [SerializeField]
        AlphaMode m_AlphaMode;

        public AlphaMode alphaMode
        {
            get { return m_AlphaMode; }
            set
            {
                if (m_AlphaMode == value)
                    return;

                m_AlphaMode = value;
                Dirty(ModificationScope.Graph);
            }
        }

        [SerializeField]
        bool m_TwoSided;

        public ToggleData twoSided
        {
            get { return new ToggleData(m_TwoSided); }
            set
            {
                if (m_TwoSided == value.isOn)
                    return;
                m_TwoSided = value.isOn;
                Dirty(ModificationScope.Graph);
            }
        }

        [SerializeField]
        NormalDropOffSpace m_NormalDropOffSpace;
        public NormalDropOffSpace normalDropOffSpace
        {
            get { return m_NormalDropOffSpace; }
            set
            {
                if (m_NormalDropOffSpace == value)
                    return;

                m_NormalDropOffSpace = value;
                if (!IsSlotConnected(NormalSlotId))
                    updateNormalSlot = true;
                UpdateNodeAfterDeserialization();
                Dirty(ModificationScope.Topological);
            }
        }
        bool updateNormalSlot;

        public PBRMasterNode()
        {
            UpdateNodeAfterDeserialization();
        }


        public sealed override void UpdateNodeAfterDeserialization()
        {
            base.UpdateNodeAfterDeserialization();
            name = "PBR Master ABC";
            AddSlot(new PositionMaterialSlot(PositionSlotId, PositionName, PositionName, CoordinateSpace.Object, ShaderStageCapability.Vertex));
            AddSlot(new NormalMaterialSlot(VertNormalSlotId, NormalName, NormalName, CoordinateSpace.Object, ShaderStageCapability.Vertex));
            AddSlot(new TangentMaterialSlot(VertTangentSlotId, TangentName, TangentName, CoordinateSpace.Object, ShaderStageCapability.Vertex));
            AddSlot(new ColorRGBMaterialSlot(VertColorSlotID, ColorName, ColorName, SlotType.Input, Color.grey.gamma, ColorMode.Default,  ShaderStageCapability.Vertex));
            AddSlot(new Vector1MaterialSlot(VertAlphaSlotID, AlphaName, AlphaName, SlotType.Input,1.0f,  ShaderStageCapability.Vertex));
            AddSlot(new ColorRGBMaterialSlot(AlbedoSlotId, AlbedoSlotName, AlbedoSlotName, SlotType.Input, Color.grey.gamma, ColorMode.Default, ShaderStageCapability.Fragment));
            //switch drop off delivery space for normal values
            var coordSpace = CoordinateSpace.Tangent;
            if (updateNormalSlot)
            {
                RemoveSlot(NormalSlotId);
                switch (m_NormalDropOffSpace)
                {
                    case NormalDropOffSpace.Tangent:
                        coordSpace = CoordinateSpace.Tangent;
                        break;
                    case NormalDropOffSpace.World:
                        coordSpace = CoordinateSpace.World;
                        break;
                    case NormalDropOffSpace.Object:
                        coordSpace = CoordinateSpace.Object;
                        break;
                }
                updateNormalSlot = false;
            }
            AddSlot(new NormalMaterialSlot(NormalSlotId, NormalSlotName, NormalSlotName, coordSpace, ShaderStageCapability.Fragment));
            AddSlot(new ColorRGBMaterialSlot(EmissionSlotId, EmissionSlotName, EmissionSlotName, SlotType.Input, Color.black, ColorMode.Default, ShaderStageCapability.Fragment));
            AddSlot(new Vector1MaterialSlot(MetallicSlotId, MetallicSlotName, MetallicSlotName, SlotType.Input, 0, ShaderStageCapability.Fragment));
            AddSlot(new Vector1MaterialSlot(SmoothnessSlotId, SmoothnessSlotName, SmoothnessSlotName, SlotType.Input, 0.5f, ShaderStageCapability.Fragment));
            AddSlot(new Vector1MaterialSlot(ReflectanceSlotID, ReflectanceName, ReflectanceName, SlotType.Input, 0.5f, ShaderStageCapability.Fragment));
            AddSlot(new Vector1MaterialSlot(OcclusionSlotId, OcclusionSlotName, OcclusionSlotName, SlotType.Input, 1f, ShaderStageCapability.Fragment));
            AddSlot(new Vector1MaterialSlot(AlphaSlotId, AlphaSlotName, AlphaSlotName, SlotType.Input, 1f, ShaderStageCapability.Fragment));
            AddSlot(new Vector1MaterialSlot(AlphaThresholdSlotId, AlphaClipThresholdSlotName, AlphaClipThresholdSlotName, SlotType.Input, 0.0f, ShaderStageCapability.Fragment));

            AddSlot(new Vector1MaterialSlot(GSAAVarienceSlotID, GSAAVarienceName, GSAAVarienceName, SlotType.Input, 0.15f, ShaderStageCapability.Fragment));
            AddSlot(new Vector1MaterialSlot(GSAAThresholdSlotID, GSAAThresholdName, GSAAThresholdName, SlotType.Input, 0.1f, ShaderStageCapability.Fragment));

            
            AddSlot(new TangentMaterialSlot(AnisotropyTangentSlotID, AnisotropyTangentName, AnisotropyTangentName, CoordinateSpace.Tangent, ShaderStageCapability.Fragment, false));
            AddSlot(new Vector1MaterialSlot(AnisotropyLevelSlotID, AnisotropyLevelName, AnisotropyLevelName, SlotType.Input, 0.0f, ShaderStageCapability.Fragment));


            // clear out slot names that do not match the slots
            // we support
            RemoveSlotsNameNotMatching(
                new[]
            {
                PositionSlotId,
                VertNormalSlotId,
                VertTangentSlotId,
                VertColorSlotID,
                VertAlphaSlotID,
                AlbedoSlotId,
                NormalSlotId,
                EmissionSlotId,
                MetallicSlotId,
                ReflectanceSlotID,
                SmoothnessSlotId,
                OcclusionSlotId,
                AlphaSlotId,
                AlphaThresholdSlotId,
                anisotropy ? AnisotropyLevelSlotID : -1,
                anisotropy ? AnisotropyTangentSlotID : -1,
                gsaa ? GSAAThresholdSlotID : -1,
                gsaa ? GSAAVarienceSlotID : -1,
                
            }, true);
        }

        protected override VisualElement CreateCommonSettingsElement()
        {
            return new PBRSettingsView(this);
        }

        public NeededCoordinateSpace RequiresNormal(ShaderStageCapability stageCapability)
        {
            List<MaterialSlot> slots = new List<MaterialSlot>();
            GetSlots(slots);

            List<MaterialSlot> validSlots = new List<MaterialSlot>();
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].stageCapability != ShaderStageCapability.All && slots[i].stageCapability != stageCapability)
                    continue;

                validSlots.Add(slots[i]);
            }
            return validSlots.OfType<IMayRequireNormal>().Aggregate(NeededCoordinateSpace.None, (mask, node) => mask | node.RequiresNormal(stageCapability));
        }

        public NeededCoordinateSpace RequiresPosition(ShaderStageCapability stageCapability)
        {
            List<MaterialSlot> slots = new List<MaterialSlot>();
            GetSlots(slots);

            List<MaterialSlot> validSlots = new List<MaterialSlot>();
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].stageCapability != ShaderStageCapability.All && slots[i].stageCapability != stageCapability)
                    continue;

                validSlots.Add(slots[i]);
            }
            return validSlots.OfType<IMayRequirePosition>().Aggregate(NeededCoordinateSpace.None, (mask, node) => mask | node.RequiresPosition(stageCapability));
        }

        public NeededCoordinateSpace RequiresTangent(ShaderStageCapability stageCapability)
        {
            List<MaterialSlot> slots = new List<MaterialSlot>();
            GetSlots(slots);

            List<MaterialSlot> validSlots = new List<MaterialSlot>();
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].stageCapability != ShaderStageCapability.All && slots[i].stageCapability != stageCapability)
                    continue;

                validSlots.Add(slots[i]);
            }
            return validSlots.OfType<IMayRequireTangent>().Aggregate(NeededCoordinateSpace.None, (mask, node) => mask | node.RequiresTangent(stageCapability));
        }
    }
}
