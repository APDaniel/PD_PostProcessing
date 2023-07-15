using PD_ScriptTemplate.Commands;
using PD_ScriptTemplate.Helpers;
using PD_ScriptTemplate.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VMS.TPS.Common.Model.API;


namespace PD_ScriptTemplate.ViewModels
{

    /// <summary>
    /// The primary view model. Represents logic for the list of structures: add/delete structure to/from the list, define actions to be performed with the structure
    /// </summary>

    public class PrimaryMainViewModel : BaseViewModel
    {

        #region private fields
        private Boolean _workIsInProgress { get; set; } = false;
        private Dispatcher userInterface;
        private string _dataPath { get; set; }
        private string _selectedComboboxStructureID { get; set; } = "TestStructureID";
        //Selected structure ID from the StructureViewModel selected in the ListView
        private string _selectedStructureIDtoShow { get; set; } = "No item selected";

        //Selected structure Color from the StructureViewModel selectid in the ListView
        private Brush _selectedStructureColortoShow { get; set; } = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d9d9d9"));

        //Selected item in the ListView
        private StructureViewModel _selectedStructureListViewItem { get; set; }

        //Collection of all structures. Consists of ViewModels for each Model. The model takes data from the desired structure stored in the structure set
        private ObservableCollection<StructureViewModel> _structureViewModelCollection { get; set; }
            = new ObservableCollection<StructureViewModel>() { new StructureViewModel(
                new StructureModel("TEST ID", "TEST DICOMtype",
                    (Color)ColorConverter.ConvertFromString("#046e4c"), true, 21)) };

        //Default collection used to store structure retriewed from Eclipse
        private ObservableCollection<StructureViewModel> _structuresFromEclipseCollection { get; set; }
            = new ObservableCollection<StructureViewModel>() { new StructureViewModel(
                new StructureModel("TEST ID", "TEST DICOMtype",
                    (Color)ColorConverter.ConvertFromString("#046e4c"), true, 21)) };

        //Collection of all structure IDs
        private ObservableCollection<string> _structureIDsCollection { get; set; } = new ObservableCollection<string>() { "Test structure 1", "Test Structure 2" };
        #endregion
        #region public fields
        public EsapiWorker esapiWorker = null;
        public Boolean workIsInProgress => _workIsInProgress;
        //Selected structure ID from the StructureViewModel selectid in the ListView
        public string selectedStructureIDtoShow 
        { 
            get { return _selectedStructureIDtoShow; } 
            set { _selectedStructureIDtoShow = value; } 
        }

        //ID of the structureSet taken from the context
        public string currentStructureSetId { get; set; } = "Test StructureSet ID";

        //Structure ID for comboboxes
        public string selectedComboboxStructureID 
        {
            get {  return _selectedComboboxStructureID;  }
            set { _selectedComboboxStructureID = value; } 
        }

        //Selected structure Color from the StructureViewModel selectid in the ListView
        public Brush selectedStructureColorToShow 
        {
            get { return _selectedStructureColortoShow; }
            set { _selectedStructureColortoShow = value; }
        }

        //Set selected Item in the ListView and run the method specific to this model
        public StructureViewModel selectedStructureListViewItem
        {
            get { return _selectedStructureListViewItem; }
            set { _selectedStructureListViewItem = value; ShowInfoForTheSelectedStructureModel(); }
        }


        public IEnumerable structuresIEnumerable
        {
            get { return _structureViewModelCollection; }
            set => _structureViewModelCollection = (ObservableCollection<StructureViewModel>)value;
        } //We use this one for binding with the view model
        public IEnumerable structureIDsIEnumerable => _structureIDsCollection.OrderBy(i => i);//We use this one for binding with the view model
         
        #endregion
        #region Public commands
        public ICommand CommandSortStructuresInTheListViewByStructureID { get; set; } //Command to sort structures in the list view. Hooked to a corresponding Header
        public ICommand CommandSortStructuresInTheListViewByStructureLabel { get; set; } //Command to sort structures in the list view. Hooked to a corresponding Header
        public ICommand CommandSortStructuresInTheListViewByStructureVolume { get; set; } //Command to sort structures in the list view. Hooked to a corresponding Header
        public ICommand CommandSortStructuresInTheListViewByStructureResolution { get; set; } //Command to sort structures in the list view. Hooked to a corresponding Header
        public ICommand CommandSortStructuresInTheListViewByStructureAlerts { get; set; } //Command to sort structures in the list view. Hooked to a corresponding Header
        public ICommand CommandDeleteStructureFromTheListView { get; set; } //This command hooked to each Delete button in the ListView
        public ICommand CommandAddStructureToTheListView { get; set; } //This command hooked to each Add button in the ListView
        public ICommand CommandPopulateStructuresToTheListView { get; set; } //This command clears the ListView and then populates all structures to the ListView
        #endregion
        #region constructor
        public PrimaryMainViewModel(EsapiWorker _esapiWorker = null) //initialize defined view model, set non-freezing user interface
        {
            esapiWorker = _esapiWorker;
            userInterface = Dispatcher.CurrentDispatcher;
            //Grab current structureSet ID from the context, populate list of structures
            GetAllStructureIDsFromStructureSet();
            PopulateStructuresToTheListView();
            Task.Delay(50);
            PopulateStructuresDataToTheCollection();

            #region Initialize commands

            #region ListView sorting commands
            CommandSortStructuresInTheListViewByStructureID = new RelayCommand(SortStructuresInTheListViewByStructureID);
            CommandSortStructuresInTheListViewByStructureLabel = new RelayCommand(SortStructuresInTheListViewByStructureLabel);
            CommandSortStructuresInTheListViewByStructureVolume = new RelayCommand(SortStructuresInTheListViewByStructureVolume);
            CommandSortStructuresInTheListViewByStructureResolution = new RelayCommand(SortStructuresInTheListViewByStructureResolution);
            CommandSortStructuresInTheListViewByStructureAlerts = new RelayCommand(SortStructuresInTheListViewByStructureAlerts);
            #endregion

            ///<summary>
            ///Populate structures for the structureSet command
            /// </summary>
            CommandAddStructureToTheListView = new RelayCommand(AddStructureToTheListView);

            ///<summary>
            ///Delete selected structure from the ListView
            /// </summary>
            CommandDeleteStructureFromTheListView = new RelayCommandSender<object>(DeleteStructureFromTheListView);

            ///<summary>
            ///Clear the ListView and then populate all structures to the ListView
            /// </summary>
            CommandPopulateStructuresToTheListView = new RelayCommand(PopulateStructuresToTheListView);
            ///<summary>
            ///Update the ListView item whe ComboBox selection is changed
            /// </summary>
            
            #endregion
        }
        #endregion

        #region private methods

        #region ListView sorting Methods
        /// <summary>
        /// Methods used for List View sorting 
        /// </summary>
        private async void SortStructuresInTheListViewByStructureID()
        {
            try
            {
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                { _structureViewModelCollection = new ObservableCollection<StructureViewModel>(_structureViewModelCollection.OrderBy(i => i.StructureID));
                });
            }
            catch (Exception exception)
            {
                //Log any appeared issues
                Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
            }
        }
        private async void SortStructuresInTheListViewByStructureLabel()
        {
            try
            {
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {
                    _structureViewModelCollection = new ObservableCollection<StructureViewModel>(_structureViewModelCollection.OrderBy(i => i.StructureLabel));
                });
            }
            catch (Exception exception)
            {
                //Log any appeared issues
                Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
            }
        }
        private async void SortStructuresInTheListViewByStructureResolution()
        {
            Logger.LogInfo("Command to sort ListView by StructuresIDs called");
            try
            {
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                { 
                    _structureViewModelCollection = new ObservableCollection<StructureViewModel>(_structureViewModelCollection.OrderByDescending(i => i.IsHighResolution));
                    Logger.LogInfo("   ListView sorted by StructuresIDs");
                });
            }
            catch (Exception exception)
            {
                //Log any appeared issues
                Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
            }
        }
        private async void SortStructuresInTheListViewByStructureVolume()
        {
            Logger.LogInfo("Command to sort ListView by Structure Volumes called");
            try
            {
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                { 
                    _structureViewModelCollection = new ObservableCollection<StructureViewModel>(_structureViewModelCollection.OrderBy(i => i.StructureVolume));
                    Logger.LogInfo("ListView sorted by structure volumes");
                });
            }
            catch (Exception exception)
            {
                //Log any appeared issues
                Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
            }
        }
        private async void SortStructuresInTheListViewByStructureAlerts()
        {
            Logger.LogInfo("Command to sort ListView by Structure Alerts called");
            try
            {
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                { 
                    _structureViewModelCollection = new ObservableCollection<StructureViewModel>(_structureViewModelCollection.OrderBy(i => i.InverseIsEmptyStructure));
                    Logger.LogInfo("ListView sorted by structure alerts");
                });
            }
            catch (Exception exception)
            {
                //Log any appeared issues
                Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
            }
        }
        #endregion

        /// <summary>
        /// Method called when the PrimaryViewModel is initialized.
        /// It creates a copy of ObservableCollection with required data from Eclipse
        /// </summary>
        private async void PopulateStructuresDataToTheCollection()
        {
            try
            {
                Logger.LogInfo("Populating structures from the database to the structuresFromEclipseCollection...");

                //Valiables to use later in this method
                string ew_currentStructureSetId = string.Empty;
                StructureSet ew_currentStructureSet;

                ///Clear the collection of all predefined items
                if (_structureViewModelCollection != null) _structureViewModelCollection.Clear();

                //Get all structures for the structure set from context and add them into the StructureViewModel collection of StructureModels
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {
                    ew_currentStructureSetId = _structureSet.Id;
                    ew_currentStructureSet = _structureSet;

                    userInterface.Invoke(() => { currentStructureSetId = ew_currentStructureSetId; });
                    Logger.LogInfo("   Run through structures in the structure set taken from context and take desired data");
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
                            

                            //Check is DICOM type has been defined for the selected structure
                            if (_selectedStructureDICOMtype.Length < 1) _selectedStructureDICOMtype = "NOT DEFINED";

                            //Add desired data from the Structure object to our collection of StructureViewModels
                            _structuresFromEclipseCollection.Add(new StructureViewModel(
                                new StructureModel(_selectedStructureID, _selectedStructureDICOMtype,
                                _selectedStructureColor, _selectedStructureIsHighResolution, _selectedStructureVolume)));
                            Logger.LogInfo(string.Format("         Structure added: {0}", _selectedStructureID));
                        });
                    }
                    //Sort structures in the list by their ID
                    Logger.LogInfo("Structures have been populated to the _structuresFromEclipseCollection");
                });

            }
            catch (Exception exception)
            {
                //Log any appeared issues
                Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
            }
            _workIsInProgress = false;
            GetAllStructureIDsFromStructureSet();

        }

        /// <summary>
        /// Method called when the PrimaryViewModel is initialized.
        /// It populates structure for the ListView from the StructureSet taken from context
        /// </summary>
        public async void PopulateStructuresToTheListView()
        {
            _workIsInProgress = true;
            try
            {
                Logger.LogInfo("PrimaryMainViewModel initialization: Populating structures from the database to the ListView...");
                // Read Script Configuration
                _dataPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                //GetScriptConfigFromXML(); // note this will fail unless this config file is defined

                //Valiables to use later in this method
                string ew_currentStructureSetId = string.Empty;
                StructureSet ew_currentStructureSet;

                ///Clear the collection of all predefined items
                if (_structureIDsCollection != null) _structureIDsCollection.Clear();
                if (_structureViewModelCollection != null)  _structureViewModelCollection.Clear();
                await Task.Delay(750);

                //Get all structures for the structure set from context and add them into the StructureViewModel collection of StructureModels
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {
                    ew_currentStructureSetId = _structureSet.Id;
                    ew_currentStructureSet = _structureSet;

                    Logger.LogInfo("   Set Current structure set ID");
                    userInterface.Invoke(() => { currentStructureSetId = ew_currentStructureSetId; });
                    Logger.LogInfo("   Run through structures in the structure set taken from context and take desired data");
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
                            Logger.LogInfo(string.Format("      Populating structure:{0}", _selectedStructureID));
                            _structureIDsCollection.Add(selectedStructureID); //This works; Planned to be used for binding of ComboBoxes for structures in the ListView
                            //Check is DICOM type has been defined for the selected structure
                            if (_selectedStructureDICOMtype.Length < 1) _selectedStructureDICOMtype = "NOT DEFINED";

                            //Add desired data from the Structure object to our collection of StructureVieModels
                            _structureViewModelCollection.Add(new StructureViewModel(
                                new StructureModel(_selectedStructureID, _selectedStructureDICOMtype,
                                _selectedStructureColor, _selectedStructureIsHighResolution, _selectedStructureVolume)));
                        });
                        
                    }
                    //Sort structures in the list by their ID
                    Logger.LogInfo("Structures have been populated");
                });

            }
            catch (Exception exception)
            {
                //Log any appeared issues
                Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
            }
            _workIsInProgress = false;
            GetAllStructureIDsFromStructureSet();
            
        }
        /// <summary>
        /// Method updates values binded to TextBlocks that represent structure ID and SolidColorBrush for foreground of this text block
        /// </summary>
        private async void ShowInfoForTheSelectedStructureModel()
        {
            Brush _defaultselectedStructureColortoShow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#d9d9d9"));
            Logger.LogInfo("Called method to update selectedStructureID and foreground color for the TextBlock");
            try
            {
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {
                    if (_selectedStructureListViewItem != null)
                    {
                        _selectedStructureIDtoShow = _selectedStructureListViewItem.StructureID;
                        _selectedStructureColortoShow = _selectedStructureListViewItem.StructureColor;
                        Logger.LogInfo(string.Format("   selectedStructureID is: {0}", _selectedStructureIDtoShow));
                    }
                    else
                    {
                        _selectedStructureIDtoShow = "No item selected";
                        _selectedStructureColortoShow = _defaultselectedStructureColortoShow;
                        Logger.LogInfo(string.Format("   No item selected"));
                    }
                });
            }
            catch (Exception exception)
            {
                //Log any appeared issues
                Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
            }
        }


        ///<summary>
        ///Method deletes selected StructureViewModel from the ListView
        /// </summary>
        private async void DeleteStructureFromTheListView(object _listViewItemObject)
        {
            Logger.LogInfo("Called method to delete selectedStructureViewModel from the ListView");

            try
            {
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {

                    if (_listViewItemObject is StructureViewModel _listViewItemObjectToDelete)
                        userInterface.Invoke(() =>
                    {

                        //This method catches the ListViewModel based on the sender object
                        Logger.LogInfo(string.Format("   Deleting the structure: {0} from the ListView", _listViewItemObjectToDelete.StructureID));
                        if (_structureViewModelCollection.Count > 1) _structureViewModelCollection.Remove(_listViewItemObjectToDelete);

                        else Logger.LogInfo(string.Format(
                            "   The structure: {0} has not been deleted from the ListView as it is the last structure in it",
                             _listViewItemObjectToDelete.StructureID));
                    });

                    
                    else
                    {
                        Logger.LogInfo("   selectedStructureViewModel has not been selected. Delete command has been ignored");
                    }
                });
            }
            catch (Exception exception)
            {
                //Log any appeared issues
                Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
            }
        }


        ///<summary>
        ///Method adds a random structure to the ListView
        /// </summary>
        private async void AddStructureToTheListView()
        {
            
            Logger.LogInfo("Called method to Add a random structure to the ListView");
            try
            {
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {
                    var selectedStructure = _structureSet.Structures.FirstOrDefault();
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
                        _workIsInProgress = true;
                        Logger.LogInfo(string.Format("   Populating structure:{0}", _selectedStructureID));

                        //Check is DICOM type has been defined for the selected structure
                        if (_selectedStructureDICOMtype.Length < 1) _selectedStructureDICOMtype = "NOT DEFINED";

                        //Add desired data from the Structure object to our collection of StructureVieModels
                        _structureViewModelCollection.Add(new StructureViewModel(
                            new StructureModel(_selectedStructureID, _selectedStructureDICOMtype,
                            _selectedStructureColor, _selectedStructureIsHighResolution, _selectedStructureVolume)));
                        _workIsInProgress = false;
                    });
                });
            }
            catch (Exception exception)
            {
                //Log any appeared issues
                Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
            }
        }


        ///<summary>
        ///Method created a list of all available structure IDs in the structureSet taken from context
        /// </summary>
        private async void GetAllStructureIDsFromStructureSet()
        {
            Logger.LogInfo("Called method to read structure IDs from the structureSet taken from context");
            try
            {
                //Grab structure IDs
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {
                    var _allStructureIDs = new ObservableCollection<string>();
                    //Invoke dispatcher to enable collection updates from a different thread


                    foreach (string structureID in _structureSet.Structures.Select(x => x.Id))
                    {
                        userInterface.Invoke(() =>
                        {
                            try { _allStructureIDs.Add(structureID); }
                            catch { Logger.LogWarning(string.Format("   Unable to add structureID of the structure {0} tot he collection of structureIDs", structureID)); }
                        });
                    }
                    _structureIDsCollection = _allStructureIDs;

                });
                Logger.LogInfo("   Structure IDs from the structureSet taken from context");
            }
            catch (Exception exception)
            {
                //Log any appeared issues
                Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
            }
        }


        ///<summary>
        ///Method to update selected ListViewItem when ComboboxSelection Changed
        /// </summary>
        #endregion
    }
}
