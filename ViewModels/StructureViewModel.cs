using PD_ScriptTemplate.Helpers;
using PD_ScriptTemplate.Models;
using System;
using System.Windows.Media;

namespace PD_ScriptTemplate.ViewModels
{
    public class StructureViewModel : BaseViewModel
    {
       
        private StructureModel _structureModel;
        public string StructureID
        {
            get { return _structureModel.StructureID; }
            set
            {
                _structureModel.StructureID = value;
                Communicator.PublishStringChanged(value);
            }
        }
        
        public string StructureLabel 
        { 
            get { return _structureModel.StructureLabel; } 
            set 
            { 
                _structureModel.StructureLabel = value; 
            } 
        }

        public SolidColorBrush StructureColor { get { return _structureModel.StructureColor; } set { _structureModel.StructureColor = value; } }

        public Boolean IsHighResolution { get { return _structureModel.IsHighResolution; } set { _structureModel.IsHighResolution = value; } }

        public Boolean IsEmptyStructure { get { return _structureModel.IsEmptyStructure; } set { _structureModel.IsEmptyStructure = value; } }

        public Boolean InverseIsEmptyStructure { get { return _structureModel.InverseIsEmptyStructure; } set { _structureModel.InverseIsEmptyStructure = value; } }

        public double StructureVolume { get { return _structureModel.StructureVolume; } set { _structureModel.StructureVolume = value; } }

        public string StructureVolumeWithcc
        {
            get { return _structureModel.StructureVolumeWithcc; }
            set { _structureModel.StructureVolumeWithcc=value; }
        }

        public string Operation { get; set; }
        
        public string Structure2ToManipulateID { get; set; }
        public string  Margin { get; set; } = "0";

        public string Margin2 { get; set; } = "0";

        public StructureViewModel(StructureModel structureModel)
        {
            _structureModel = structureModel ?? throw new ArgumentNullException(nameof(structureModel));
        }
        
    }
}
