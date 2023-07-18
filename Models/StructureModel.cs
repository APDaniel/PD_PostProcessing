using System;
using System.Windows.Media;

namespace PD_ScriptTemplate.Models
{
    public class StructureModel
    {
        public string StructureID { get; set; }
        public string StructureLabel { get; set; }
        public SolidColorBrush StructureColor { get; set; }
        public Boolean IsHighResolution { get; set; }
        public Boolean InverseIsHighResolution { get; set; }
        public Boolean IsEmptyStructure { get; set; }
        public Boolean InverseIsEmptyStructure { get; set; }
        public double StructureVolume { get; set; }
        public string StructureVolumeWithcc { get; set; }
        public StructureModel(string structureID, string structureDICOMtype, 
            Color structureColor, bool structureIsHighResolution, double structureVolume)
        {
            StructureID = structureID; //string
            StructureLabel = structureDICOMtype; if (StructureLabel.Length < 1) StructureLabel = "NOT DEFINED"; //string
            StructureColor = new SolidColorBrush(structureColor); //Color
            IsHighResolution = structureIsHighResolution; //bool
            IsEmptyStructure = structureVolume < 0.1 ? true : false; //bool
            InverseIsEmptyStructure = structureVolume < 0.1 ? false : true; //bool 
            StructureVolume = Math.Round(structureVolume, 2); //double
            StructureVolumeWithcc = (Math.Round(structureVolume, 2)).ToString() +" cc";
        }
    }
}
