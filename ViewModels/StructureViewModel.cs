using PD_ScriptTemplate.Models;
using System;
using System.Windows.Media;

namespace PD_ScriptTemplate.ViewModels
{
    public class StructureViewModel:BaseViewModel
    {
        private StructureModel _structureModel;
        public string StructureID => _structureModel.StructureID;
        public string StructureLabel => _structureModel?.StructureLabel;
        public SolidColorBrush StructureColor => _structureModel.StructureColor;
        public Boolean IsHighResolution => _structureModel.IsHighResolution;
        public Boolean IsEmptyStructure => _structureModel.IsEmptyStructure;
        public Boolean InverseIsEmptyStructure => _structureModel.InverseIsEmptyStructure;
        public string StructureVolume => _structureModel.StructureVolume;

        public StructureViewModel(StructureModel structureModel)
        {
            _structureModel = structureModel;
        }
    }
}
