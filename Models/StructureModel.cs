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
        public Boolean IsEmptyStructure { get; set; }
        public Boolean InverseIsEmptyStructure { get; set; }
        public string StructureVolume { get; set; }
        public StructureModel(string structureID, string structureDICOMtype, 
            Color structureColor, bool structureIsHighResolution, double structureVolume)
        {
            StructureID = structureID; //string
            StructureLabel = structureDICOMtype; //string
            StructureColor = new SolidColorBrush(structureColor); //Color
            IsHighResolution = structureIsHighResolution; //bool
            IsEmptyStructure = structureVolume < 0.1 ? true : false; //bool
            InverseIsEmptyStructure = structureVolume < 0.1 ? false : true; //bool 
            StructureVolume = Math.Round(structureVolume, 2).ToString() + "cc"; //double

        }
    }
}
