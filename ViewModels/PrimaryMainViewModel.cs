using PD_ScriptTemplate.Commands;
using PD_ScriptTemplate.Helpers;
using PD_ScriptTemplate.Models;
using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using VMS.TPS;
using VMS.TPS.Common.Model.API;


namespace PD_ScriptTemplate.ViewModels
{

    /// <summary>
    /// The primary view model. Represents logic for the list of structures: add/delete structure to/from the list, define actions to be performed with the structure
    /// </summary>

    public class PrimaryMainViewModel : BaseViewModel, IDisposable
    {
        #region private fields
        //Due to StructureViewModels inside the PrimaryMainViewModel, we cannot use CanExecuteChanged() for disabling the button. Instead, we use Communicatior and bool value 
        private bool _isButtonEnabled { get; set; }
        private Boolean _workIsInProgress { get; set; } = false;
        private Dispatcher userInterface;
        private string _spinnerPhrase { get; set; } = "Loading";
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
        public string spinnerPhrase {get => _spinnerPhrase; set { _spinnerPhrase = value; }}
        public Boolean workIsInProgress { get => _workIsInProgress; set { _workIsInProgress = value; } } 
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
                AddStructureToTheListView();
                PopulateStructuresDataToTheCollection();
            }
        }

        //Due to StructureViewModels inside the PrimaryMainViewModel, we cannot use CanExecuteChanged() for disabling the button. Instead, we use Communicatior and bool value 
        public bool IsButtonEnabled
        {
            get { return _isButtonEnabled; }
            set
            {
                _isButtonEnabled = value;
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
        public ICommand ExitCommand { get; set; } //This commands shuts down the application
        #endregion
        #region Constructor
        public PrimaryMainViewModel(EsapiWorker _esapiWorker = null) //initialize defined view model, set non-freezing user interface
        {
            var type = typeof(FontAwesome.WPF.FontAwesome); //This is required to ensure that FontAwesome will be created locally. Otherwise the script will not be executed

            esapiWorker = _esapiWorker;
            userInterface = Dispatcher.CurrentDispatcher;
            userInterface.Invoke(() => { _workIsInProgress = true; Task.Delay(500); _workIsInProgress = false; });
            //Find all available structure sets
            FindAllAvailableStructureSets();
            //Grab current structureSet ID from the context, populate list of structures
            GetAllStructureIDsFromStructureSet();
            AddStructureToTheListView();
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
            ///Create optimization structures
            /// </summary>
            CommandCreateOptimizationStructures = new RelayCommand(CreateOptimizationStructures);

            ///<summary>
            ///This command shuts down the application
            /// </summary>
            ExitCommand = new RelayCommand(Exit);
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
                    foreach (StructureSet _structureSetToAdd in _patient.StructureSets)
                    {
                        var check = _availableStructureSetIDsCollection.FirstOrDefault(x => x == _structureSetToAdd.Id);
                        if (check == null)
                        {
                            Logger.LogInfo(string.Format("...Adding the structure set: {0} to the collection of structure sets", _structureSetToAdd.Id));
                            _availableStructureSetIDsCollection.Add(_structureSetToAdd.Id);
                        }
                    }

                    userInterface.Invoke(() => 
                    {
                        foreach (string structureSetID in _availableStructureSetIDsCollection) 
                        {
                            var check = _availableStructureSetIDs.FirstOrDefault(x => x == structureSetID);
                            if (check==null) _availableStructureSetIDs.Add(structureSetID);
                        }
                        Logger.LogInfo(string.Format("...Selecting default structure set: {0}", currentStructureSetId));

                        if(_currentStructureSetId==null|| _currentStructureSetId.Length<2)
                            _currentStructureSetId = _availableStructureSetIDs.FirstOrDefault();
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
                Logger.LogInfo("Populating structures from the database to the collection of structures from Eclipse");

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
                    Logger.LogInfo("Run through structures in the structure set taken from context and take desired data");
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
                            _workIsInProgress = true;
                            //Add desired data from the Structure object to our collection of StructureViewModels
                            _structuresFromEclipseCollection.Add(new StructureViewModel(
                                new StructureModel(_selectedStructureID, _selectedStructureDICOMtype,
                                _selectedStructureColor, _selectedStructureIsHighResolution, _selectedStructureVolume)));
                            Logger.LogInfo(string.Format(".........Structure added: {0}", _selectedStructureID));
                            _workIsInProgress = false;
                        });
                    }
                    //Sort structures in the list by their ID
                    Logger.LogInfo("Structures have been populated to the to the collection of structures from Eclipse");
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
                _workIsInProgress = true;

                //Get all structures for the structure set from context and add them into the StructureViewModel collection of StructureModels
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {
                    ew_currentStructureSetId = _currentStructureSetId;
                    ew_currentStructureSet = _patient.StructureSets.FirstOrDefault(x => x.Id == _currentStructureSetId);

                    
                    
                    Logger.LogInfo("Run through structures in the structure set taken from the desired patient and take neccessary data");
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
                            Logger.LogInfo(string.Format("...Populating structure:{0}", _selectedStructureID));
                            _structureIDsCollection.Add(selectedStructureID); //Planned to be used for binding of ComboBoxes for structures in the ListView
                            //Add desired data from the Structure object to our collection of StructureVieModels
                            _structureViewModelCollection.Add(new StructureViewModel(
                                new StructureModel(_selectedStructureID, _selectedStructureDICOMtype,
                                _selectedStructureColor, _selectedStructureIsHighResolution, _selectedStructureVolume)));
                            
                        });
                        _workIsInProgress = false;
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
                        Logger.LogInfo(string.Format("...selectedStructureID is: {0}", _selectedStructureIDtoShow));
                    }
                    else
                    {
                        _selectedStructureIDtoShow = "No item selected";
                        _selectedStructureColortoShow = _defaultselectedStructureColortoShow;
                        Logger.LogInfo(string.Format("No item selected"));
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
                        Logger.LogInfo(string.Format("...Deleting the structure: {0} from the ListView", _listViewItemObjectToDelete.StructureID));
                        if (_structureViewModelCollection.Count > 1) _structureViewModelCollection.Remove(_listViewItemObjectToDelete);

                        else Logger.LogInfo(string.Format(
                            "...The structure: {0} has not been deleted from the ListView as it is the last structure in it",
                             _listViewItemObjectToDelete.StructureID));
                    });


                    else
                    {
                        Logger.LogInfo("...StructureViewModel has not been selected. Delete command has been ignored");
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
                    var selectedStructureSet = _patient.StructureSets.FirstOrDefault(x => x.Id == _currentStructureSetId);
                    var selectedStructure = selectedStructureSet.Structures.FirstOrDefault();
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
                        
                        Logger.LogInfo(string.Format("...Adding a random structure structure:{0}", _selectedStructureID));

                        //Add desired data from the Structure object to our collection of StructureVieModels
                        _structureViewModelCollection.Add(new StructureViewModel(
                            new StructureModel(_selectedStructureID, _selectedStructureDICOMtype,
                            _selectedStructureColor, _selectedStructureIsHighResolution, _selectedStructureVolume)));
                        
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
            Logger.LogInfo("Called method to read structure IDs from the structureSet selected in the ComboBox");
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
                            try { _allStructureIDs.Add(structureID); 
                                Logger.LogInfo(string.Format("...Structure: {0} has been added to the collection of all available structure IDs", structureID)); }

                            catch { Logger.LogWarning(string.Format("...Unable to add structureID of the structure {0} to the collection of structureIDs", structureID)); }
                        });
                    }
                    _structureIDsCollection = _allStructureIDs;

                });
                Logger.LogInfo("Structure IDs populated from the structureSet taken from the ComboBox selection");
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
            //_workIsInProgress = false;
            Logger.LogInfo("StructureID in the comboBox changed, called method to update the collection of StructureViewModel");

            try
            {
                //Get all structures for the structure set from context and add them into the StructureViewModel collection of StructureModels
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {
                    var _selectedStrucutreSet = _patient.StructureSets.FirstOrDefault(x => x.Id == currentStructureSetId);
                    foreach (var structureViewModel in _structureViewModelCollection)
                    {
                        
                        if (structureViewModel.StructureID == newValue)
                        {
                            Logger.LogInfo(string.Format("...Reading structureViewModel for the structure: {0}", newValue));
                            var _selectedStructure = _selectedStrucutreSet.Structures.FirstOrDefault(x => string.Equals(x.Id, structureViewModel.StructureID, StringComparison.OrdinalIgnoreCase));
                            var _selectedStructureID = _selectedStructure.Id;
                            var _selectedStructureDICOMtype = _selectedStructure.DicomType;
                            var _selectedStructureColor = _selectedStructure.Color;
                            var _selectedStructureIsHighRes = _selectedStructure.IsHighResolution;
                            var _selectedStructureVolume = _selectedStructure.Volume;
                            
                            userInterface.Invoke(() => 
                            {
                                Logger.LogInfo(string.Format("...Updating structureViewModel for the structure: {0} with data pulled from the structure: {1}", newValue, _selectedStructureID));
                                var updatedStructureViewModel = new StructureViewModel(
                                    new StructureModel(
                                        _selectedStructureID, _selectedStructureDICOMtype, _selectedStructureColor, _selectedStructureIsHighRes, _selectedStructureVolume)
                                );

                                //This chunk of code is required to update all the properties in the list view item to show (structureViewModel)
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
                    Logger.LogInfo("Structure data has been updated in accordance with the selected StructureID");
                });

                
                Logger.LogInfo("Checking if the margin, margin2, StructureToManipulate are valid for all StructureViewModels in the UI");
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {
                    userInterface.Invoke(() =>
                    {
                    foreach (StructureViewModel structureViewModel in _structureViewModelCollection)
                    {
                            Logger.LogInfo(string.Format("Checking StructureViewModel for the structure: {0}", structureViewModel.StructureID));

                            bool isValidMargin = int.TryParse(structureViewModel.Margin, out int margin) && margin >= -50 && margin <= 50;
                            Logger.LogInfo(string.Format("...margin of {0}mm is defined as: {1}", structureViewModel.Margin, isValidMargin.ToString()));

                            bool isValidMargin2 = int.TryParse(structureViewModel.Margin2, out int margin2) && margin2 >= -50 && margin2 <= 50;
                            Logger.LogInfo(string.Format("...margin of {0}mm is defined as: {1}", structureViewModel.Margin2, isValidMargin2.ToString()));

                            bool isStructure2Empty = !string.IsNullOrEmpty(structureViewModel.Structure2ToManipulateID);
                            Logger.LogInfo(string.Format("...Structure to Manipulate Id: {0} is defined as: {1}", structureViewModel.Structure2ToManipulateID, isStructure2Empty.ToString()));

                            _isButtonEnabled = isValidMargin && isValidMargin2 && isStructure2Empty;

                            if (structureViewModel.Operation == "ring" || structureViewModel.Operation == "PRV") 
                            {
                                _isButtonEnabled = isValidMargin && isValidMargin2;
                            }

                                if (_isButtonEnabled == false) 
                            { 
                                Logger.LogInfo("...One of the parameters is false. Disabling the button");
                                Logger.LogInfo(string.Format("...Margin={0} {1}, Margin2={2} {3}, structure to manipulate: {4} {5}",
                                    structureViewModel.Margin, isValidMargin.ToString(),
                                    structureViewModel.Margin2, isValidMargin2.ToString(),
                                    structureViewModel.Structure2ToManipulateID, isStructure2Empty.ToString()
                                    ));
                                break; 
                            }

                            Logger.LogInfo("...All parameters are valid. Enabling the button");
                            Logger.LogInfo(string.Format("...Margin={0} {1}, Margin2={2} {3}, structure to manipulate: {4} {5}",
                                    structureViewModel.Margin, isValidMargin.ToString(),
                                    structureViewModel.Margin2, isValidMargin2.ToString(),
                                    structureViewModel.Structure2ToManipulateID, isStructure2Empty.ToString()
                                    ));
                            _isButtonEnabled = true;
                    }
                        
                        Logger.LogInfo(".......................BUTTON UPDATED..........................");
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
        /// Method used to create optimization structures base on the instructions in the UI
        /// </summary>
        private async void CreateOptimizationStructures()
        {
            Logger.LogInfo("");
            Logger.LogInfo(".................................................");
            Logger.LogInfo("Called a method to create optimization structures");
            Logger.LogInfo(".................................................");
            Logger.LogInfo("");
            _spinnerPhrase = "Generating optimization structures...";
            _workIsInProgress = true;
            try
            {
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {

                    Logger.LogInfo(string.Format("Enabling modifications for the selected patient: {0}", _patient.Id.ToString()));
                    _patient.BeginModifications(); //Enable patient modifications

                    #region duplicate the structure set if desired
                    //Check if user intended to create structures on the duplicated structure set
                    StructureSet _duplicatedStructureSet=null;
                    var _duplicatedStructureSetId = "Auto0";
                    if (_duplicateStructureSet==true) 
                    {
                        DuplicateStructureSetHelper.DuplicateStructureSet(_patient, _structureSet, _duplicatedStructureSet, _duplicatedStructureSetId);
                    }
                        else { _duplicatedStructureSet = _patient.StructureSets.FirstOrDefault(x => x.Id ==_currentStructureSetId); _duplicatedStructureSetId = _duplicatedStructureSet.Id; }
                
                    #endregion

                Logger.LogInfo("Connecting to the data stored in the view model");
                foreach (StructureViewModel structureViewModel in _structureViewModelCollection)
                    {
                    #region Define optimization structure ID

                    Logger.LogInfo(string.Format("...Looking for the structure: {0} in the structure set: {1}", structureViewModel.StructureID, _duplicatedStructureSetId));
                    var _structureToManipulate1 = _duplicatedStructureSet.Structures.FirstOrDefault(x => x.Id == structureViewModel.StructureID);

                    Logger.LogInfo(string.Format("...Looking for the structure: {0} in the structure set: {1}", structureViewModel.Structure2ToManipulateID, _duplicatedStructureSetId));
                        
                        Structure _structureToManipulate2=null;
                        if (structureViewModel.Structure2ToManipulateID != null)
                        {
                            _structureToManipulate2 = _duplicatedStructureSet.Structures.FirstOrDefault(x => x.Id == structureViewModel.Structure2ToManipulateID);
                        }

                    //Create temporary structures for manipulations
                    Logger.LogInfo("...Creating technical structure 'PDtoDel1'");
                    Structure _structureToManipulate1operational1 = _duplicatedStructureSet.AddStructure("CONTROL", "PDtoDel1");
                    _structureToManipulate1operational1.SegmentVolume = _structureToManipulate1.Margin(0);

                    Logger.LogInfo("...Creating technical structure 'PDtoDel2'");
                    Structure _structureToManipulate1operational2 = _duplicatedStructureSet.AddStructure("CONTROL", "PDtoDel2");
                        if (_structureToManipulate2 != null) 
                        { 
                            _structureToManipulate1operational2.SegmentVolume = _structureToManipulate2.Margin(0); 
                        }

                    string structureToSaveID = "";

                    Logger.LogInfo("...Defining structure ID to save");
                    switch (structureViewModel.Operation)
                    {
                        case "+":
                            Logger.LogInfo("...Case for ID is identified as '+'");
                            structureToSaveID = _structureToManipulate1.Id + "+" + _structureToManipulate2.Id;  
                            break;
                        case "PRV":
                            Logger.LogInfo("...Case for ID is identified as 'PRV'");
                            structureToSaveID = _structureToManipulate1.Id + "_PRV" + string.Format("{0}", Double.Parse(structureViewModel.Margin));
                                break;
                        case "-":
                            Logger.LogInfo("...Case for ID is identified as '-'");
                            structureToSaveID = _structureToManipulate1.Id + "-" + _structureToManipulate2.Id;
                                break;

                        case "and":
                            Logger.LogInfo("...Case for ID is identified as 'and'");
                            structureToSaveID = _structureToManipulate1.Id + "_and_" + _structureToManipulate2.Id;
                                break;

                        case "sub":
                            Logger.LogInfo("...Case for ID is identified as 'sub'");
                            structureToSaveID = _structureToManipulate1.Id + "_sub_" + _structureToManipulate2.Id;
                                break;

                        case "ring":
                            Logger.LogInfo("...Case for ID is identified as 'ring'");
                            structureToSaveID = _structureToManipulate1.Id + "_ring";
                                break;
                    }
                        
                        if (structureToSaveID[0] != 'x') { structureToSaveID = "x" + structureToSaveID; }
                        structureToSaveID = structureToSaveID.Replace("__", "_");
                        Logger.LogInfo(string.Format("...Decided to use structure ID: {0}", structureToSaveID));

                        //Check if the optimization structure ID is lass then 16 characters
                        structureToSaveID=StructureUniqueIdHelper.CheckIfTheStructureIDisUnique(structureToSaveID, _duplicatedStructureSet);

                    #endregion

                    #region Create the optimization structure and define its volume
                    Logger.LogInfo(string.Format("Creating structure to save with ID: {0} in the structure set:{1}", structureToSaveID, _duplicatedStructureSet.Id));
                    var _structureToSave = _duplicatedStructureSet.AddStructure("CONTROL", structureToSaveID); //Assign the structure DICOM tag as "CONTROL" by default

                    Logger.LogInfo("Checking if one of the structures for operations is in high resolution");
                    //Check if at least one structure is in high resolution
                    if (_structureToManipulate1operational1.IsHighResolution == true || _structureToManipulate1operational2.IsHighResolution == true)
                    {
                        Logger.LogInfo("...Detected a structure in high resolution");
                            if (_structureToManipulate1operational1.CanConvertToHighResolution() == true)
                            {
                                _structureToManipulate1operational1.ConvertToHighResolution(); Logger.LogInfo(string.Format("...Converting structure:{0} in high resolution", _structureToManipulate1operational1.Id));
                            }

                            if (_structureToManipulate1operational2.CanConvertToHighResolution() == true)
                            {
                                _structureToManipulate1operational2.ConvertToHighResolution(); Logger.LogInfo(string.Format("...Converting structure:{0} in high resolution", _structureToManipulate1operational2.Id));
                            }

                            if (_structureToSave.CanConvertToHighResolution() == true)
                            {
                                _structureToSave.ConvertToHighResolution(); Logger.LogInfo(string.Format("...Converting structure:{0} in high resolution", _structureToSave.Id));
                            }
                        }
                    else Logger.LogInfo("No structures defined for manipulations are in high resolution");

                    Logger.LogInfo("Reading Margin");
                    var margin = Double.Parse(structureViewModel.Margin);
                    Logger.LogInfo(string.Format("   ...Margin={0}mm", margin.ToString()));

                    Logger.LogInfo("Reading Margin2");
                    var margin2 = Double.Parse(structureViewModel.Margin2);
                    Logger.LogInfo(string.Format("   ...Margin2={0}mm", margin2.ToString()));

                    Logger.LogInfo("Reading instructions for the manipulation");
                    switch (structureViewModel.Operation)
                    {
                        case "+":
                            Logger.LogInfo(string.Format(
                                "...Manipulation defined as '+'. Combining volumes of the structure: {0} and the structure: {1} with the margin={2}mm",
                                     _structureToManipulate1.Id, _structureToManipulate2, margin));

                                try 
                                { 
                                    _structureToSave.SegmentVolume = _structureToManipulate1operational1.Or(_structureToManipulate1operational2);
                                    _structureToSave.SegmentVolume = _structureToSave.Margin(margin);
                                    Logger.LogInfo("...........................................................");
                                    Logger.LogInfo("..........................SUCCESS..........................");
                                    Logger.LogInfo(string.Format
                                        ("Structure: {0} volume={1}cc has been created as a combination of " +
                                        "the structure: {2} volume={3}cc " +
                                        "and the structure: {4} volume={5}cc with the margin={6}mm", 
                                        _structureToSave.Id, Math.Round(_structureToSave.Volume,2),
                                        _structureToManipulate1.Id, Math.Round(_structureToManipulate1.Volume,2),
                                        _structureToManipulate2.Id, Math.Round(_structureToManipulate2.Volume, 2),
                                        margin.ToString()
                                        ));
                                    Logger.LogInfo("...........................................................");
                                    Logger.LogInfo("...........................................................");

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
                                "...Manipulation defined as 'PRV'. Creating a PRV with ID: {0} from the structure: {1} with the margin={2}", 
                                _structureToSave.Id, _structureToManipulate1.Id, margin.ToString()
                                ));
                                Logger.LogInfo("");
                                Logger.LogInfo("");

                                try 
                                { 
                                    _structureToSave.SegmentVolume = _structureToManipulate1operational1.Margin(margin);
                                    Logger.LogInfo("...........................................................");
                                    Logger.LogInfo("..........................SUCCESS..........................");
                                    Logger.LogInfo(string.Format(
                                "PRV has been created as a structure: {0} volume={1}cc by expanding the structure: {2} volume={3}cc with the margin={4}mm",
                                _structureToSave.Id, Math.Round(_structureToSave.Volume,2),
                                _structureToManipulate1.Id, Math.Round(_structureToManipulate1.Volume,2),
                                margin.ToString()
                                ));
                                    Logger.LogInfo("...........................................................");
                                    Logger.LogInfo("...........................................................");
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
                                "...Manipulation defined as '-'. Substracting the structure: {0} from the structure: {1} whith the margin={2}mm",
                                _structureToManipulate2.Id, _structureToManipulate1.Id, margin
                                ));
                                try
                                {
                                    _structureToManipulate1operational2.SegmentVolume = _structureToManipulate1operational2.Margin(margin);
                                    _structureToSave.SegmentVolume = _structureToManipulate1operational1.Sub(_structureToManipulate1operational2);
                                    Logger.LogInfo("...........................................................");
                                    Logger.LogInfo("..........................SUCCESS..........................");
                                    Logger.LogInfo(string.Format("Structure: {0} volume={1}cc has been created by " +
                                        "substracting the structure {2} volume={3}cc with the margin={4}mm from the structure: {5} volume={6}mm",
                                        _structureToSave.Id, Math.Round(_structureToSave.Volume,2),
                                        _structureToManipulate2.Id, Math.Round(_structureToManipulate2.Volume, 2),
                                        margin.ToString(),
                                        _structureToManipulate1.Id, Math.Round(_structureToManipulate1.Volume,2)

                           ));
                                    Logger.LogInfo("...........................................................");
                                    Logger.LogInfo("...........................................................");
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
                                "...Manipulation defined as 'and'. Defining the intersection of the structures: {0} and {1} with the margin={2}mm",
                                _structureToManipulate2.Id, _structureToManipulate1.Id, margin
                                ));
                                try
                                {
                                    _structureToSave.SegmentVolume = _structureToManipulate1operational1.And(_structureToManipulate1operational2);
                                    _structureToSave.SegmentVolume = _structureToSave.Margin(0);
                                    Logger.LogInfo("...........................................................");
                                    Logger.LogInfo("..........................SUCCESS..........................");
                                    Logger.LogInfo(string.Format("Structure: {0} volume={1}cc has been created by " +
                                        "combining the structure {2} volume={3}cc with the structure: {4} volume={5}mm with the margin={6}mm",
                                        _structureToSave.Id, Math.Round(_structureToSave.Volume, 2),
                                        _structureToManipulate1.Id, Math.Round(_structureToManipulate1.Volume, 2),
                                        _structureToManipulate2.Id, Math.Round(_structureToManipulate2.Volume, 2),
                                        margin.ToString()
                           ));
                                    Logger.LogInfo("...........................................................");
                                    Logger.LogInfo("...........................................................");
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
                                "...Manipulation defined as 'not'. Defining the volume that includes everynting but the intersection of the structures: {0} and {1} with the margin={2}mm",
                                _structureToManipulate2.Id, _structureToManipulate1.Id, margin
                                ));

                                try
                                {
                                    _structureToSave.SegmentVolume = _structureToManipulate1operational1.Xor(_structureToManipulate1operational2);
                                    _structureToSave.SegmentVolume = _structureToSave.Margin(margin);
                                    Logger.LogInfo("...........................................................");
                                    Logger.LogInfo("..........................SUCCESS..........................");
                                    Logger.LogInfo(string.Format("Structure: {0} volume={1}cc has been created by " +
                                        "substracting the intersection of the structure {2} volume={3}cc with the structure: {4} volume={5}mm with the margin={6}mm",
                                        _structureToSave.Id, Math.Round(_structureToSave.Volume, 2),
                                        _structureToManipulate1.Id, Math.Round(_structureToManipulate1.Volume, 2),
                                        _structureToManipulate2.Id, Math.Round(_structureToManipulate2.Volume, 2),
                                        margin.ToString()
                           ));
                                    Logger.LogInfo("...........................................................");
                                    Logger.LogInfo("...........................................................");
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
                                "...Manipulation defined as 'ring'. Creating a ring with ID: {0} from the structure: {1} with the margin={2}mm and inner margin of={3}mm",
                                _structureToSave.Id, _structureToManipulate1.Id, margin.ToString(), margin2.ToString()
                                ));

                                try
                                {
                                    _structureToSave.SegmentVolume = _structureToManipulate1operational1.Margin(margin);
                                    _structureToManipulate1operational1.SegmentVolume = _structureToManipulate1operational1.Margin(margin2);
                                    _structureToSave.SegmentVolume = _structureToSave.Sub(_structureToManipulate1operational1);
                                    Logger.LogInfo("...........................................................");
                                    Logger.LogInfo("..........................SUCCESS..........................");
                                    Logger.LogInfo(string.Format("Structure: {0} volume={1}cc has been created by " +
                                        "creating a ring from the structure {2} volume={3}cc with the margin={4}mm and inner margin={5}mm",
                                        _structureToSave.Id, Math.Round(_structureToSave.Volume, 2),
                                        _structureToManipulate1.Id, Math.Round(_structureToManipulate1.Volume, 2),
                                        margin.ToString(), margin2.ToString()
                                        ));
                                    Logger.LogInfo("...........................................................");
                                    Logger.LogInfo("...........................................................");
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
                        Logger.LogInfo(string.Format("Removing technical structure :{0}", _structureToManipulate1operational1.Id));
                        _duplicatedStructureSet.RemoveStructure(_structureToManipulate1operational1);

                        Logger.LogInfo(string.Format("Removing technical structure :{0}", _structureToManipulate1operational2.Id));
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
            GetAllStructureIDsFromStructureSet();
            AddStructureToTheListView();
            PopulateStructuresDataToTheCollection();
            Logger.LogInfo(".................................................");
            Logger.LogInfo("....Optimization structure have been created.....");
            Logger.LogInfo(".................................................");
            _workIsInProgress = false;
            _spinnerPhrase = "Loading";
        }

        ///<summary>
        ///This command is used to shut down the application
        /// </summary>
        private void Exit() 
        {
            Logger.LogInfo("....Called a command to shut down the script.....");
            Script.mainWindow?.Close(); 
        }



        #endregion
        #region public methods
        /// <summary>
        /// This method is used to dispose current view model. Which is required to unsubscribe from the supscriptions
        /// </summary>

        public void Dispose()
        {
            Communicator.StringChanged -= Communicator_UpdateCollectionOfViewModelsWhenStructureIDchanged;
        }
        #endregion
    }
}
