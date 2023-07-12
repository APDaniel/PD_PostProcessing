using PD_ScriptTemplate.Helpers;
using PD_ScriptTemplate.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using VMS.TPS.Common.Model.API;

namespace PD_ScriptTemplate.ViewModels
{
    /// <summary>
    /// The view model that is used for navigation between other view models
    /// </summary>

    public class MainViewModel : BaseViewModel
    {
        #region private fields
        private Dispatcher userInterface;
        private string _dataPath { get; set; }
        private ObservableCollection<StructureViewModel> _structureModelCollection { get; set; }
            = new ObservableCollection<StructureViewModel>() { new StructureViewModel(
                new StructureModel("TEST ID", "TEST DICOMtype",
                    (Color)ColorConverter.ConvertFromString("#046e4c"), true, 21)) };
        private ObservableCollection<string> _structureIDsCollection { get; set; } = new ObservableCollection<string>() { "Test structure 1", "Test Structure 2" };
        #endregion

        #region public fields
        public EsapiWorker esapiWorker = null;
        public string currentStructureSetId { get; set; } = "Test StructureSet ID";
        public IEnumerable structuresIEnumerable => _structureModelCollection; //We use this one for binding with the view model
        public IEnumerable structureIDsIEnumerable => _structureIDsCollection;

        #endregion

        #region constructor
        public MainViewModel(EsapiWorker _esapiWorker = null) //initialize defined view model, set non-freezing user interface
        {
            esapiWorker = _esapiWorker;
            userInterface = Dispatcher.CurrentDispatcher;
            InitializeMainViewModel();
        }
        #endregion

        #region private methods
        private async void InitializeMainViewModel()
        {
            try
            {
                // Read Script Configuration
                _dataPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                //GetScriptConfigFromXML(); // note this will fail unless this config file is defined
                // Initialize other GUI settings

                string ew_currentStructureSetId = string.Empty;
                StructureSet ew_currentStructureSet;
                ///Clear the collection of all predefined items
                if (_structureIDsCollection != null)
                        _structureIDsCollection.Clear();
                
                //Get all structures for the structure set from context and add them into the StructureViewModel collection of StructureModels
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {
                    ew_currentStructureSetId = _structureSet.Id;
                    ew_currentStructureSet = _structureSet;


                    userInterface.Invoke(() => { currentStructureSetId = ew_currentStructureSetId;});

                    foreach (var selectedStructureID in _structureSet.Structures.Select(x=>x.Id))
                    {
                        var selectedStructure = _structureSet.Structures.FirstOrDefault(x => string.Equals(x.Id, selectedStructureID, StringComparison.OrdinalIgnoreCase));
                        
                        #region private variables to be used in AsyncRunStructureContext
                        string _selectedStructureID= selectedStructure.Id;
                        string _selectedStructureDICOMtype = selectedStructure.DicomType;
                        Color _selectedStructureColor= selectedStructure.Color;
                        bool _selectedStructureIsHighResolution= selectedStructure.IsHighResolution;
                        double _selectedStructureVolume= selectedStructure.Volume;
                        #endregion

                        //_structureModelCollection.Add(new StructureModel(selectedStructure.Id, selectedStructure.DicomType,
                        //    selectedStructure.Color, selectedStructure.IsHighResolution, selectedStructure.Volume));
                        userInterface.Invoke(()=> 
                        {
                            _structureIDsCollection.Add(selectedStructureID); //This works; Planned to be used for binding of ComboBoxes for structures in the ListView

                            if (_selectedStructureDICOMtype.Length < 1) _selectedStructureDICOMtype = "Not defined";

                            _structureModelCollection.Add(new StructureViewModel(
                                new StructureModel(_selectedStructureID, _selectedStructureDICOMtype,
                                _selectedStructureColor, _selectedStructureIsHighResolution, _selectedStructureVolume)));
                            
                        });
                    }
                         _structureModelCollection = new ObservableCollection<StructureViewModel>(_structureModelCollection.OrderBy(i => i.StructureID));
                });

            }
            catch (Exception ex)
            {
                Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", ex.Message, ex.InnerException, ex.StackTrace));
            }
        }
        #endregion

        #region public methods
        
        #endregion
    }
}
