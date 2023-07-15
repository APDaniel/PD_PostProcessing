using PD_ScriptTemplate.Models;
using System;
using System.Windows.Media;

namespace PD_ScriptTemplate.ViewModels
{
    public class StructureViewModel:BaseViewModel
    {

        private StructureModel _structureModel;
        public string StructureID 
        { 
            get { return _structureModel.StructureID; } 
            set 
            { 
                _structureModel.StructureID = value;

            } 
        }
        
        
        public string StructureLabel { get { return _structureModel.StructureLabel; } set { _structureModel.StructureLabel = value; } }
        
        
        public SolidColorBrush StructureColor { get { return _structureModel.StructureColor; } set { _structureModel.StructureColor = value; } }
        
        public Boolean IsHighResolution { get { return _structureModel.IsHighResolution; } set { _structureModel.IsHighResolution = value; } }
       
        public Boolean IsEmptyStructure { get { return _structureModel.IsEmptyStructure; } set { _structureModel.IsEmptyStructure = value; } }
        
        public Boolean InverseIsEmptyStructure { get { return _structureModel.InverseIsEmptyStructure; } set { _structureModel.InverseIsEmptyStructure = value; } }
        
        public double StructureVolume { get { return _structureModel.StructureVolume; } set { _structureModel.StructureVolume = value; } }
       
        public string StructureVolumeWithcc { get => _structureModel.StructureVolume.ToString() + " cc"; set => _structureModel.StructureVolume.ToString(); }
        
        public string StructureViewModelGuid => Guid.NewGuid().ToString();

        public StructureViewModel(StructureModel structureModel)
        {
            _structureModel = structureModel??throw new ArgumentNullException(nameof(structureModel));
        }



        public async void PopulateStructuresToTheListView()
        {
           
            try
            {
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {
                    foreach (var selectedStructureID in _structureSet.Structures.Select(x => x.Id))
                    {
                        var selectedStructure = _structureSet.Structures.FirstOrDefault(x => string.Equals(x.Id, selectedStructureID, StringComparison.OrdinalIgnoreCase));
                        #region private variables to be used in AsyncRunStructureContext
                        string _selectedStructureID = selectedStructure.Id;
                        string _selectedStructureDICOMtype = selectedStructure.DicomType;
                        Color _selectedStructureColor = selectedStructure.Color;
                        bool _selectedStructureIsHighResolution = selectedStructure.IsHighResolution;
                        double _selectedStructureVolume = selectedStructure.Volume;
                        #endregion

                        //Invoke dispatcher to enable collection updates from a different thread
                        userInterface.Invoke(() =>
                        {
                            
                        });

                    }
                    
                });

            }
    }
}
