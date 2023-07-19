using PD_ScriptTemplate.Commands;
using PD_ScriptTemplate.Helpers;
using PD_ScriptTemplate.Models;
using System;
using System.Collections;
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

        //Checks if user wants to create structureson a duplicated structure set
        private bool _duplicateStructureSet { get; set; }
        private string _currentStructureSetId { get; set; }
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

        private StructureViewModel _structureViewModel;

        //Collection of all available structure sets for the selected patient
        private ObservableCollection<string> _availableStructureSetIDs { get; set; } = new ObservableCollection<string>();

        //Selected Operation
        private string _selectedManipulation { get; set; } = "Operation...";

        //Selected 2nd structure ID
        private string _selectedStructureIDtoManipulate { get; set; } = "2nd structure...";

        //Selected margin
        private int _selectedMargin { get; set; } = 0;

        //Available operations
        private ObservableCollection<string> _availableManipulations { get; set; } = new ObservableCollection<string>() { "Test structure 1", "Test Structure 2" };

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

        public bool duplicateStructureSet 
        { 
            get { return _duplicateStructureSet; } 
            set { _duplicateStructureSet = value; } }
        //ID of the structureSet taken from the context
        public string currentStructureSetId
        {
            get { return _currentStructureSetId; }
            set 
            {
                _currentStructureSetId=value;
                //Grab current structureSet ID from the selected structure set, populate list of structures
                GetAllStructureIDsFromStructureSet();
                //PopulateStructuresToTheListView();
                AddStructureToTheListView();
                Task.Delay(50);
                PopulateStructuresDataToTheCollection();
            }
        }

        //Structure ID for comboboxes
        public string selectedComboboxStructureID
        {
            get { return _selectedComboboxStructureID; }
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

        //Selected Operation
        public string SelectedManipulation { get { return _selectedManipulation; } set { _selectedManipulation = value; } } 

        //Selected 2nd structure ID
        public string StructureIDtoManipulate { get { return _selectedStructureIDtoManipulate; } set { _selectedStructureIDtoManipulate = value; } } 

        //Selected margin
        public int SelectedMargin { get { return _selectedMargin; } set { _selectedMargin = value; } }

        //Collection of available structure sets for the selected patient
        public IEnumerable availableStructureSets 
        { 
            get { return _availableStructureSetIDs; } 
            set { _availableStructureSetIDs = (ObservableCollection<string>)value; } 
        }

        //Collection of available manipulations
        public IEnumerable ManipulationsAvailable
        {
            get { return _availableManipulations; }
            set { _availableManipulations = (ObservableCollection<string>)value; }
        }

        /// <summary>
        /// //This is the source for the ListView. When the collection of populated structure changes, 
        /// it calls one metod to update ListViewItems
        /// </summary>
        public IEnumerable structuresIEnumerable
        {
            get { return _structureViewModelCollection; }
            set
            {
                _structureViewModelCollection = (ObservableCollection<StructureViewModel>)value;
            }
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
        public ICommand CommandCreateOptimizationStructures { get; set; } //This command creates optimization structures by presets taken from the UI (or from a scheme)
        #endregion
        #region Constructor
        public PrimaryMainViewModel(EsapiWorker _esapiWorker = null) //initialize defined view model, set non-freezing user interface
        {
            
            esapiWorker = _esapiWorker;
            userInterface = Dispatcher.CurrentDispatcher;

            //Find all available structure sets
            FindAllAvailableStructureSets();
            //Grab current structureSet ID from the context, populate list of structures
            GetAllStructureIDsFromStructureSet();
                        
            Task.Delay(50);
            PopulateStructuresDataToTheCollection();

            //Subscribe to the event to update StructureViewModelCollection when a ComboBox selection changed
            Communicator.StringChanged += Communicator_UpdateCollectionOfViewModelsWhenStructureIDchanged;

            //Define available manipulations
            ManipulationsAvailable = new ObservableCollection<string>() { "+", "-", "and", "sub","ring", "PRV" };

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
            ///Create the optimization structures
            /// </summary>
            CommandCreateOptimizationStructures = new RelayCommand(CreateOptimizationStructures);
            #endregion


        }
        #endregion
        #region Private methods

        #region ListView sorting Methods
        /// <summary>
        /// Methods used for List View sorting 
        /// </summary>
        private async void SortStructuresInTheListViewByStructureID()
        {
            try
            {
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {
                    _structureViewModelCollection = new ObservableCollection<StructureViewModel>(_structureViewModelCollection.OrderBy(i => i.StructureID));
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

        ///<summary>
        ///Find all available structure sets for the selected patient
        /// </summary>
        private async void FindAllAvailableStructureSets()
        {
            Logger.LogInfo("Called a method to read all structure sets for the selected patient");
            try
            {
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {
                    //Create a collection 
                    ObservableCollection<string> _availableStructureSetIDsCollection = new ObservableCollection<string>();
                    if (_availableStructureSetIDsCollection.Count > 0) _availableStructureSetIDsCollection.Clear();
                    foreach (StructureSet _structureSetToAdd in _patient.StructureSets)
                    {
                        Logger.LogInfo(string.Format("   ...Adding the structure set: {0} to the collection of structure sets", _structureSetToAdd.Id));
                        _availableStructureSetIDsCollection.Add(_structureSetToAdd.Id);
                    }

                    userInterface.Invoke(() => 
                    {
                        if (_availableStructureSetIDs.Count > 0) _availableStructureSetIDs.Clear();
                        foreach (string structureSetID in _availableStructureSetIDsCollection) 
                        {
                            _availableStructureSetIDs.Add(structureSetID);
                        }
                        Logger.LogInfo(string.Format("   ...Selecting default structure set: {0}", currentStructureSetId));
                        currentStructureSetId = _availableStructureSetIDs.FirstOrDefault();
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
                string ew_currentStructureSetId = _currentStructureSetId;
                StructureSet ew_currentStructureSet;

                ///Clear the collection of all predefined items
                if (_structureViewModelCollection != null) _structureViewModelCollection.Clear();

                //Get all structures for the structure set from context and add them into the StructureViewModel collection of StructureModels
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {
                    //ew_currentStructureSetId = _structureSet.Id; Grabs a structureSet from the context
                    ew_currentStructureSetId = currentStructureSetId;
                    ew_currentStructureSet = _patient.StructureSets.FirstOrDefault(x=>x.Id== ew_currentStructureSetId);

                    //userInterface.Invoke(() => { currentStructureSetId = ew_currentStructureSetId; });
                    Logger.LogInfo("   Run through structures in the structure set taken from context and take desired data");
                    foreach (var selectedStructureID in ew_currentStructureSet.Structures.Select(x => x.Id))
                    {
                        var selectedStructure = ew_currentStructureSet.Structures.FirstOrDefault(x => string.Equals(x.Id, selectedStructureID, StringComparison.OrdinalIgnoreCase));
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
                if (_structureViewModelCollection != null) _structureViewModelCollection.Clear();
                await Task.Delay(750);

                //Get all structures for the structure set from context and add them into the StructureViewModel collection of StructureModels
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {
                    ew_currentStructureSetId = _currentStructureSetId;
                    ew_currentStructureSet = _patient.StructureSets.FirstOrDefault(x => x.Id == _currentStructureSetId);


                    Logger.LogInfo("   Run through structures in the structure set taken from context and take desired data");
                    foreach (var selectedStructureID in ew_currentStructureSet.Structures.Select(x => x.Id))
                    {
                        var selectedStructure = ew_currentStructureSet.Structures.FirstOrDefault(x => string.Equals(x.Id, selectedStructureID, StringComparison.OrdinalIgnoreCase));
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
                            _structureIDsCollection.Add(selectedStructureID); //Planned to be used for binding of ComboBoxes for structures in the ListView

                            //Add desired data from the Structure object to our collection of StructureVieModels
                            _structureViewModelCollection.Add(new StructureViewModel(
                                new StructureModel(_selectedStructureID, _selectedStructureDICOMtype,
                                _selectedStructureColor, _selectedStructureIsHighResolution, _selectedStructureVolume)));
                        });

                    }
                    //Sort structures in the list by their ID
                    Logger.LogInfo("Structures have been populated");
                });
                _workIsInProgress = false;
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
                    var _currentStructureSet = _patient.StructureSets.FirstOrDefault(x => x.Id == _currentStructureSetId);

                    foreach (string structureID in _currentStructureSet.Structures.Select(x => x.Id))
                    {
                        userInterface.Invoke(() =>
                        {
                            try { _allStructureIDs.Add(structureID); }
                            catch { Logger.LogWarning(string.Format("   Unable to add structureID of the structure {0} to the collection of structureIDs", structureID)); }
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
        ///Method to update selected ListViewItem when ComboboxSelectedItem Changes
        /// </summary>
        private async void Communicator_UpdateCollectionOfViewModelsWhenStructureIDchanged(object sender, string newValue)
        {
            _workIsInProgress = true;
            Logger.LogInfo("StructureID in the comboBox changed, called method to update the collection of StructureViewModel");

            try
            {
                //Valiables to use later in this method
                await Task.Delay(250);

                //Get all structures for the structure set from context and add them into the StructureViewModel collection of StructureModels
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {
                    var _selectedStrucutreSet = _patient.StructureSets.FirstOrDefault(x => x.Id == currentStructureSetId);
                    foreach (var structureViewModel in _structureViewModelCollection)
                    {
                        foreach (var selectedStructureID in _selectedStrucutreSet.Structures.Select(x => x.Id))
                        {
                            var _selectedStructure = _selectedStrucutreSet.Structures.FirstOrDefault(x => string.Equals(x.Id, structureViewModel.StructureID, StringComparison.OrdinalIgnoreCase));
                            var _selectedStructureID = _selectedStructure.Id;
                            var _selectedStructureDICOMtype = _selectedStructure.DicomType;
                            var _selectedStructureColor = _selectedStructure.Color;
                            var _selectedStructureIsHighRes = _selectedStructure.IsHighResolution;
                            var _selectedStructureVolume = _selectedStructure.Volume;

                            userInterface.Invoke(() => 
                            {
                                var updatedStructureViewModel = new StructureViewModel(
                                    new StructureModel(
                                        _selectedStructureID, _selectedStructureDICOMtype, _selectedStructureColor, _selectedStructureIsHighRes, _selectedStructureVolume)
                                );

                                var structureViewModelType = structureViewModel.GetType();
                                var updatedStructureViewModelType = updatedStructureViewModel.GetType();

                                foreach (var property in structureViewModelType.GetProperties())
                                {
                                    
                                        var updatedStructureViewModelProperty = updatedStructureViewModelType.GetProperty(property.Name);

                                        if (updatedStructureViewModelProperty != null &&
                                            updatedStructureViewModelProperty.PropertyType == property.PropertyType &&
                                            updatedStructureViewModelProperty.CanRead)
                                        {
                                            var value = updatedStructureViewModelProperty.GetValue(updatedStructureViewModel);
                                            property.SetValue(structureViewModel, value);
                                        }
                                }
                            });
                            
                        }
                    }

                    // Sort structures in the list by their ID
                    Logger.LogInfo("Structures data has been updated in accordance with the selected StructureID");
                });
                _workIsInProgress = false;
            }
            catch (Exception exception)
            {
                //Log any appeared issues
                Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
            }

            _workIsInProgress = false;
        }


        /// <summary>
        /// Method used to create optimization structures base on the instructions in the UI
        /// </summary>
        private async void CreateOptimizationStructures()
        {
            Logger.LogInfo("Called a method to create optimization structures");
            _workIsInProgress = true;

            try
            {
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {
                Logger.LogInfo("   ...Duplicatiing structure set");
                #region duplicate the structure set
                var _duplicatedStructureSetId = "Auto0";


                //Check if the duplicated structureSet ID is unique
                Logger.LogInfo("   ...Checking if the ID of duplicated structure set is unique");
                for (int i = 1; i < 9; i++)
                {
                    _duplicatedStructureSetId = _duplicatedStructureSetId.Substring(0, _duplicatedStructureSetId.Length - 1) + i.ToString();

                    var check = _patient.StructureSets.FirstOrDefault(x => x.Id == _duplicatedStructureSetId);
                    if (check == null) break;
                }
                Logger.LogInfo(string.Format("   ...Decided to use ID: {0}", _duplicatedStructureSetId));

                Logger.LogInfo(string.Format("   ...Enagbling modifications for the patient: {0}", _patient.Id));
                _patient.BeginModifications(); //Enable patient modifications

                StructureSet _duplicatedStructureSet = _structureSet.Image.CreateNewStructureSet(); //Create new structureSet
                _duplicatedStructureSet.Id = _duplicatedStructureSetId; //Assign ss.Id
                Logger.LogInfo(string.Format("   ...Created structureSet with ID: ", _duplicatedStructureSetId));

                foreach (Structure structure in _structureSet.Structures) //Duplicate structures from one ss to another
                {
                    var _DICOMtype = structure.DicomType;
                    var _structureId = structure.Id;

                    if (_DICOMtype == null || _DICOMtype == "") _DICOMtype = "CONTROL"; //If DICOM tag is empty, then assign it to "CONTROL"

                    Structure _duplicatedStructure = _duplicatedStructureSet.AddStructure(_DICOMtype, _structureId);
                    Logger.LogInfo(string.Format("   ...Copied structureSet with ID: ", _structureId));

                    _duplicatedStructure.SegmentVolume = structure.SegmentVolume;
                    Logger.LogInfo(string.Format("   ...Copied structure volume with ID: ", _structureId));
                }
                #endregion

                Logger.LogInfo("   ...Connecting to the data in the UI");
                foreach (StructureViewModel structureViewModel in _structureViewModelCollection)
                {
                    #region define optimization structure ID

                    Logger.LogInfo(string.Format("   ...Looking for the structure: {0} in the structure set: {1}", structureViewModel.StructureID, _duplicatedStructureSetId));
                    var _structureToManipulate1 = _duplicatedStructureSet.Structures.FirstOrDefault(x => x.Id == structureViewModel.StructureID);

                    Logger.LogInfo(string.Format("   ...Looking for the structure: {0} in the structure set: {1}", structureViewModel.Structure2ToManipulateID, _duplicatedStructureSetId));
                        
                        Structure _structureToManipulate2=null;
                        if (structureViewModel.Structure2ToManipulateID != null)
                        {
                            _structureToManipulate2 = _duplicatedStructureSet.Structures.FirstOrDefault(x => x.Id == structureViewModel.Structure2ToManipulateID);
                        }

                    //Create temporary structures for manipulations
                    Logger.LogInfo("   ...Creating technical structure 'PDtoDel1'");
                    Structure _structureToManipulate1operational1 = _duplicatedStructureSet.AddStructure("CONTROL", "PDtoDel1");
                    _structureToManipulate1operational1.SegmentVolume = _structureToManipulate1.Margin(0);

                    Logger.LogInfo("   ...Creating technical structure 'PDtoDel2'");
                    Structure _structureToManipulate1operational2 = _duplicatedStructureSet.AddStructure("CONTROL", "PDtoDel2");
                        if (_structureToManipulate2 != null) 
                        { 
                            _structureToManipulate1operational2.SegmentVolume = _structureToManipulate2.Margin(0); 
                        }

                    string structureToSaveID = "";

                    Logger.LogInfo("   ...Defining structure ID to save");
                    switch (structureViewModel.Operation)
                    {
                        case "+":
                            Logger.LogInfo("   ...Case for ID is identified as '+'");
                            structureToSaveID = "z" + _structureToManipulate1.Id + "+" + _structureToManipulate2.Id;
                            break;
                        case "PRV":
                            Logger.LogInfo("   ...Case for ID is identified as 'PRV'");
                            structureToSaveID = "z" + _structureToManipulate1.Id + "_PRV";
                            break;
                        case "-":
                            Logger.LogInfo("   ...Case for ID is identified as '-'");
                            structureToSaveID = "z" + _structureToManipulate1.Id + "-" + _structureToManipulate2.Id;
                            break;

                        case "and":
                            Logger.LogInfo("   ...Case for ID is identified as 'and'");
                            structureToSaveID = "z" + _structureToManipulate1.Id + "_and_" + _structureToManipulate2.Id;
                            break;

                        case "sub":
                            Logger.LogInfo("   ...Case for ID is identified as 'sub'");
                            structureToSaveID = "z" + _structureToManipulate1.Id + "_sub_" + _structureToManipulate2.Id;
                            break;

                        case "ring":
                            Logger.LogInfo("   ...Case for ID is identified as 'ring'");
                            structureToSaveID = "z" + _structureToManipulate1.Id + "_ring";
                            break;
                    }
                        structureToSaveID = structureToSaveID.Replace("__", "_");
                        Logger.LogInfo(string.Format("   ...Decided to use structure ID: {0}", structureToSaveID));

                    //Check if the optimization structure ID is lass then 16 characters
                    if (structureToSaveID.Length > 16)
                    {
                            //delete '_' if length is >16
                            if (structureToSaveID.Length > 16)
                            {
                                Logger.LogInfo("   ...Structure ID is too long. Trying to resolve the issue getting rid of '_'");
                                string toDelete = "_";
                                string updatedStructureToSaveID1 = "";
                                for (int i = 0; i < structureToSaveID.Length; i++)
                                {
                                    if (!toDelete.Contains(structureToSaveID[i]))
                                    { updatedStructureToSaveID1 += structureToSaveID[i]; };
                                }
                                if (updatedStructureToSaveID1.Length > 1)
                                {
                                    structureToSaveID = updatedStructureToSaveID1;
                                }
                            }

                            //delete vowels if length is >16
                            if (structureToSaveID.Length > 16)
                            {
                                Logger.LogInfo("   ...Structure ID is too long. Trying to resolve the issue getting rid of vowels");
                                //delete vowels
                                string vowels = "AEIOUaeiou";
                                string updatedStructureToSaveID = "";
                                for (int i = 0; i < structureToSaveID.Length; i++)
                                {
                                    if (!vowels.Contains(structureToSaveID[i]))
                                    { updatedStructureToSaveID += structureToSaveID[i]; };
                                }
                                if (updatedStructureToSaveID.Length > 1)
                                {
                                    structureToSaveID = updatedStructureToSaveID;
                                }
                            }
                            

                            //delete numbers if length is still>16
                            if (structureToSaveID.Length > 16)
                            {
                                Logger.LogInfo("   ...Structure ID is too long. Trying to resolve the issue getting rid of numbers");
                                string toDelete = "1234567890";
                                string updatedStructureToSaveID1 = "";
                                for (int i = 0; i < structureToSaveID.Length; i++)
                                {
                                    if (!toDelete.Contains(structureToSaveID[i]))
                                    { updatedStructureToSaveID1 += structureToSaveID[i]; };
                                }
                                if (updatedStructureToSaveID1.Length > 1)
                                {
                                    structureToSaveID = updatedStructureToSaveID1;
                                }
                            }

                            //delete excessive letters if length is still>16
                            if (structureToSaveID.Length > 16)
                        {
                                Logger.LogInfo("   ...Structure ID is too long. Trying to resolve the issue getting rid of excessive letters");
                                int numberOfLettersToDelete = structureToSaveID.Length - 16;
                                int numberOfLettersToKeep = structureToSaveID.Length - numberOfLettersToDelete;
                                structureToSaveID = structureToSaveID.Substring(0, numberOfLettersToKeep);
                        }
                            
                        Logger.LogInfo(string.Format("   ...Corrected structure ID is: {0}", structureToSaveID));
                    }

                    Logger.LogInfo("   ...Checking if the structure ID is unique");
                    //Check if the structure name is unique
                    var _checkIfStructureIdUnique = _duplicatedStructureSet.Structures.FirstOrDefault(x => x.Id == structureToSaveID);

                    if (_checkIfStructureIdUnique != null)
                    {
                        Logger.LogInfo("   ...The structure ID is not unique, correcting");
                        for (int i = 1; i < 9; i++)
                        {
                            structureToSaveID = structureToSaveID.Substring(0, structureToSaveID.Length - 1) + i.ToString();

                            var check = _duplicatedStructureSet.Structures.FirstOrDefault(x => x.Id == structureToSaveID);
                            if (check == null) break;
                        }
                        Logger.LogInfo(string.Format("   ...Corrected structure ID is: {0}", structureToSaveID));
                    }
                    #endregion
                    #region Create the optimization structure and define its volume
                    Logger.LogInfo(string.Format("   ...Creating structure to save with ID: {0} in the structure set:{1}", structureToSaveID, _duplicatedStructureSet.Id));
                    var _structureToSave = _duplicatedStructureSet.AddStructure("CONTROL", structureToSaveID); //Assign the structure DICOM tag as "CONTROL" by default

                    Logger.LogInfo("   ...Checking if one of the structures for operations is in high resolution");
                    //Check if at least one structure is in high resolution
                    if (_structureToManipulate1operational1.IsHighResolution == true || _structureToManipulate1operational2.IsHighResolution == true)
                    {
                        Logger.LogInfo("   ...Detected a structure in high resolution");
                            if (_structureToManipulate1operational1.CanConvertToHighResolution() == true)
                            {
                                _structureToManipulate1operational1.ConvertToHighResolution(); Logger.LogInfo(string.Format("   ...Converting structure:{0} in high resolution", _structureToManipulate1operational1.Id));
                            }

                            if (_structureToManipulate1operational2.CanConvertToHighResolution() == true)
                            {
                                _structureToManipulate1operational2.ConvertToHighResolution(); Logger.LogInfo(string.Format("   ...Converting structure:{0} in high resolution", _structureToManipulate1operational2.Id));
                            }

                            if (_structureToSave.CanConvertToHighResolution() == true)
                            {
                                _structureToSave.ConvertToHighResolution(); Logger.LogInfo(string.Format("   ...Converting structure:{0} in high resolution", _structureToSave.Id));
                            }
                        }
                    else Logger.LogInfo("   ...No structures defined for manipulations are in high resolution");

                    Logger.LogInfo("   ...Reading Margin");
                    var margin = Double.Parse(structureViewModel.Margin);
                    Logger.LogInfo(string.Format("   ...Margin={0}mm", margin.ToString()));

                    Logger.LogInfo("   ...Reading Margin2");
                    var margin2 = Double.Parse(structureViewModel.Margin2);
                    Logger.LogInfo(string.Format("   ...Margin2={0}mm", margin2.ToString()));

                    Logger.LogInfo("   ...Reading instructions for the manipulation");
                    switch (structureViewModel.Operation)
                    {
                        case "+":
                            Logger.LogInfo(string.Format(
                                "   ...Manipulation defined as '+'. Combining volumes of the structure: {0} and the structure: {1} with the margin={2}mm",
                                     _structureToManipulate1.Id, _structureToManipulate2, margin));

                                try 
                                { 
                                    _structureToSave.SegmentVolume = _structureToManipulate1operational1.Or(_structureToManipulate1operational2);
                                    _structureToSave.SegmentVolume = _structureToSave.Margin(margin);

                                    Logger.LogInfo(string.Format
                                        ("   ...Structure: {0} volume={1}cc has been created as a combination of " +
                                        "the structure: {2} volume={3}cc " +
                                        "and the structure: {4} volume={5}cc with the margin={6}mm", 
                                        _structureToSave.Id, Math.Round(_structureToSave.Volume,2),
                                        _structureToManipulate1.Id, Math.Round(_structureToManipulate1.Volume,2),
                                        _structureToManipulate2.Id, Math.Round(_structureToManipulate2.Volume, 2),
                                        margin.ToString()
                                        ));
                                    Logger.LogInfo("");
                                    Logger.LogInfo("");
                                } 
                                catch (Exception exception)
                                {
                                    //Log any appeared issues
                                    Logger.LogInfo("");
                                    Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                                    Logger.LogInfo("");
                                    MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                                }
                                break;

                            case "PRV":
                                Logger.LogInfo(string.Format(
                                "   ...Manipulation defined as 'PRV'. Creating a PRV with ID: {0} from the structure: {1} with the margin={2}", 
                                _structureToSave.Id, _structureToManipulate1.Id, margin.ToString()
                                ));
                                Logger.LogInfo("");
                                Logger.LogInfo("");

                                try 
                                { 
                                    _structureToSave.SegmentVolume = _structureToManipulate1operational1.Margin(margin);

                                    Logger.LogInfo(string.Format(
                                "   ...PRV has been created as a structure: {0} volume={1}cc by expanding the structure: {2} volume={3}cc with the margin={4}mm",
                                _structureToSave.Id, Math.Round(_structureToSave.Volume,2),
                                _structureToManipulate1.Id, Math.Round(_structureToManipulate1.Volume,2),
                                margin.ToString()
                                ));
                                    Logger.LogInfo("");
                                    Logger.LogInfo("");
                                }

                                catch (Exception exception)
                                {
                                    //Log any appeared issues
                                    Logger.LogInfo("");
                                    Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                                    Logger.LogInfo("");
                                    MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                                }
                                break;

                            case "-":
                                Logger.LogInfo(string.Format(
                                "   ...Manipulation defined as '-'. Substracting the structure: {0} from the structure: {1} whith the margin={2}mm",
                                _structureToManipulate2.Id, _structureToManipulate1.Id, margin
                                ));
                                try
                                {
                                    _structureToManipulate1operational2.SegmentVolume = _structureToManipulate1operational2.Margin(margin);
                                    _structureToSave.SegmentVolume = _structureToManipulate1operational1.Sub(_structureToManipulate1operational2);

                                    Logger.LogInfo(string.Format("   ...structure: {0} volume={1}cc has been created by " +
                                        "substracting the structure {2} volume={3}cc with the margin={4}mm from the structure: {5} volume={6}mm",
                                        _structureToSave.Id, Math.Round(_structureToSave.Volume,2),
                                        _structureToManipulate2.Id, Math.Round(_structureToManipulate2.Volume, 2),
                                        margin.ToString(),
                                        _structureToManipulate1.Id, Math.Round(_structureToManipulate1.Volume,2)
                           ));
                                    Logger.LogInfo("");
                                    Logger.LogInfo("");
                                }
                                catch (Exception exception)
                                {
                                    //Log any appeared issues
                                    Logger.LogInfo("");
                                    Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                                    Logger.LogInfo("");
                                    MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                                }
                                break;
                                

                            case "and":
                                Logger.LogInfo(string.Format(
                                "   ...Manipulation defined as 'and'. Defining the intersection of the structures: {0} and {1} with the margin={2}mm",
                                _structureToManipulate2.Id, _structureToManipulate1.Id, margin
                                ));
                                try
                                {
                                    _structureToSave.SegmentVolume = _structureToManipulate1operational1.And(_structureToManipulate1operational2);
                                    _structureToSave.SegmentVolume = _structureToSave.Margin(0);

                                    Logger.LogInfo(string.Format("   ...structure: {0} volume={1}cc has been created by " +
                                        "combining the structure {2} volume={3}cc with the structure: {4} volume={5}mm with the margin={6}mm",
                                        _structureToSave.Id, Math.Round(_structureToSave.Volume, 2),
                                        _structureToManipulate1.Id, Math.Round(_structureToManipulate1.Volume, 2),
                                        _structureToManipulate2.Id, Math.Round(_structureToManipulate2.Volume, 2),
                                        margin.ToString()
                           ));
                                    Logger.LogInfo("");
                                    Logger.LogInfo("");
                                }
                                catch (Exception exception)
                                {
                                    //Log any appeared issues
                                    Logger.LogInfo("");
                                    Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                                    Logger.LogInfo("");
                                    MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                                }
                                break;


                            case "not":
                                Logger.LogInfo(string.Format(
                                "   ...Manipulation defined as 'not'. Defining the volume that includes everynting but the intersection of the structures: {0} and {1} with the margin={2}mm",
                                _structureToManipulate2.Id, _structureToManipulate1.Id, margin
                                ));

                                try
                                {
                                    _structureToSave.SegmentVolume = _structureToManipulate1operational1.Xor(_structureToManipulate1operational2);
                                    _structureToSave.SegmentVolume = _structureToSave.Margin(margin);

                                    Logger.LogInfo(string.Format("   ...structure: {0} volume={1}cc has been created by " +
                                        "substracting the intersection of the structure {2} volume={3}cc with the structure: {4} volume={5}mm with the margin={6}mm",
                                        _structureToSave.Id, Math.Round(_structureToSave.Volume, 2),
                                        _structureToManipulate1.Id, Math.Round(_structureToManipulate1.Volume, 2),
                                        _structureToManipulate2.Id, Math.Round(_structureToManipulate2.Volume, 2),
                                        margin.ToString()
                           ));
                                    Logger.LogInfo("");
                                    Logger.LogInfo("");
                                }
                                catch (Exception exception)
                                {
                                    //Log any appeared issues
                                    Logger.LogInfo("");
                                    Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                                    Logger.LogInfo("");
                                    MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                                }
                                break;

                            case "ring":
                                Logger.LogInfo(string.Format(
                                "   ...Manipulation defined as 'ring'. Creating a ring with ID: {0} from the structure: {1} with the margin={2}mm and inner margin of={3}mm",
                                _structureToSave.Id, _structureToManipulate1.Id, margin.ToString(), margin2.ToString()
                                ));

                                try
                                {
                                    _structureToSave.SegmentVolume = _structureToManipulate1operational1.Margin(margin);
                                    _structureToManipulate1operational1.SegmentVolume = _structureToManipulate1operational1.Margin(margin2);
                                    _structureToSave.SegmentVolume = _structureToSave.Sub(_structureToManipulate1operational1);

                                    Logger.LogInfo(string.Format("   ...structure: {0} volume={1}cc has been created by " +
                                        "creating a ring from the structure {2} volume={3}cc with the margin={4}mm and inner margin={5}mm",
                                        _structureToSave.Id, Math.Round(_structureToSave.Volume, 2),
                                        _structureToManipulate1.Id, Math.Round(_structureToManipulate1.Volume, 2),
                                        margin.ToString(), margin2.ToString()
                                        ));
                                    Logger.LogInfo("");
                                    Logger.LogInfo("");
                                }
                                catch (Exception exception)
                                {
                                    //Log any appeared issues
                                    Logger.LogInfo("");
                                    Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                                    Logger.LogInfo("");
                                    MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                                }
                                break;
                        }

                        //Remove temporary structures
                        _duplicatedStructureSet.RemoveStructure(_structureToManipulate1operational1);
                        _duplicatedStructureSet.RemoveStructure(_structureToManipulate1operational2);
                        #endregion
                    }
            });
            }
            catch (Exception exception)
            {
                //Log any appeared issues
                Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
            }
            FindAllAvailableStructureSets();
            _workIsInProgress = false;
        }

        #endregion
        #region public methods

        #endregion
    }
}
