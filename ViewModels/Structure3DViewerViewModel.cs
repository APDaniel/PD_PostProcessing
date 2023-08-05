using PD_ScriptTemplate.Commands;
using PD_ScriptTemplate.Helpers;
using PD_ScriptTemplate.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using VMS.TPS;
using VMS.TPS.Common.Model.API;
using VMS.TPS.Common.Model.Types;

namespace PD_ScriptTemplate.ViewModels
{
    /// <summary>
    /// This class is used to create and correct structure 3D models
    /// </summary>
    public class Structure3DViewerViewModel : BaseViewModel
    {
        #region Private fields
        private string _volumeTolerance { get; set; } = "1";
        private string _spinnerPhrase { get; set; } = "Loading...";
        //Private field to make the loading spinner visible
        private bool _workIsInProgress { get; set; } = false;
        //Volume of the selected structure
        private string _structureVolume { get; set; } = "0";
        //Arrows coordinates
        private Point3D _point1ForArrows { get; set; } = new Point3D(-200, 200, -200);
        private Point3D _point2ForXarrow { get; set; } = new Point3D(-200, -200, -200);
        private Point3D _point2ForYarrow { get; set; } = new Point3D(200, 200, 200);
        private Point3D _point2ForZarrow { get; set; } = new Point3D(-200, 200, 200);

        //Mesh geometry of a structure
        private MeshGeometry3D _meshGeometry { get; set; } = new MeshGeometry3D();

        //Color of the mesh geometry
        private DiffuseMaterial _diffuseMaterial { get; set; } = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(255, 255, 255)));

        //Position of the camera in the viewport
        private Point3D _cameraPosition { get; set; } = new Point3D(0, 0, 0);

        //Look direction of the camera in the viewport
        private Vector3D _cameraDirection { get; set; } = new Vector3D();

        //Geometry model to represent in the viewport
        private GeometryModel3D _structure3DmodelToShow { get; set; }

        //Geometry to represent the slider 
        private GeometryModel3D _slider3Dmodel { get; set; }
        private GeometryModel3D _slider3Dmodel2 { get; set; }

        //Private fields for maximum and minimum slider values
        private double _maximumSliderZposition { get; set; } = 0;
        private double _minimumSliderZposition { get; set; } = 0;

        //Maximal and minimal coordinates of the slider. Do not have public fields and bindings. Used to optimize hardware resources consumed by ViewPort3D
        private double _maximumSliderXposition { get; set; } = 0;
        private double _minimumSliderXposition { get; set; } = 0;
        private double _maximumSliderYposition { get; set; } = 0;
        private double _minimumSliderYposition { get; set; } = 0;

        //Number of slices for the ROI slider
        private double _numberOfROISlices { get; set; } = 1;

        //Slider Z position
        private double _sliderZposition { get; set; } = 0;

        //Selected structure set ID 
        private string _currentStructureSetId { get; set; }

        //Current dispatcher
        private Dispatcher _userInterface;
        //Collection of all available structureSets for the selected patient

        //Collection of all available structure set IDs
        private ObservableCollection<string> _availableStructureSetIDs { get; set; } = new ObservableCollection<string>();

        //Collection of all available structure IDs
        private ObservableCollection<string> _structureIDsCollection { get; set; } = new ObservableCollection<string>() { "Test structure 1", "Test Structure 2" };

        //Selected structure set ID 
        private string _selectedStructureID { get; set; }

        //List of all available meshes
        private ObservableCollection<Structure3DModelViewModel> _structures3DViewModels { get; set; } = new ObservableCollection<Structure3DModelViewModel>();
        #endregion
        #region Public fields
        //Used for binding with the user interface
        public string volumeTolerance { get { return _volumeTolerance; } set { _volumeTolerance = value; } }
        //Used for binding with the phrase represented in the spinner grid
        public string spinnerPhrase { get { return _spinnerPhrase; } set { _spinnerPhrase = value; } }
        //Used for binding with the spinner grid
        public bool workIsInProgress { get { return _workIsInProgress; } set { _workIsInProgress = value; } }
        //Used for binding with the UI volume data for the selected structure
        public string structureVolume { get { return _structureVolume; } set { _structureVolume = value; } }
        // Used for binding in the viewport
        public GeometryModel3D structure3DmodelToShow { get { return _structure3DmodelToShow; } set { _structure3DmodelToShow = value; } }

        //Used for camera position binding
        public Point3D cameraPosition { get { return _cameraPosition; } set { _cameraPosition = value; } }

        //Used for camera look direction binding
        public Vector3D cameraDirection { get { return _cameraDirection; } set { _cameraDirection = value; } }

        //Used for selected item combobox selection. Updates the VisualGeometryModel when selection changes
        public string selectedStructureID
        {
            get { return _selectedStructureID; }
            set
            {
                _selectedStructureID = value;
                if (_selectedStructureID != null)
                {
                    _structure3DmodelToShow = null;
                    DisplayStructure3DModel();
                    DisplaySlider();
                }
            }
        }
        //Used for binding of arrow positions in the ViewPort3D
        public Point3D point1ForArrows { get { return _point1ForArrows; } set { _point1ForArrows = value; } }
        public Point3D point2ForXarrow { get { return _point2ForXarrow; } set { _point2ForXarrow = value; } }
        public Point3D point2ForYarrow { get { return _point2ForYarrow; } set { _point2ForYarrow = value; } }
        public Point3D point2ForZarrow { get { return _point2ForZarrow; } set { _point2ForZarrow = value; } }

        //Used for binding to slider 3D geometry model
        public GeometryModel3D slider3Dmodel { get { return _slider3Dmodel; } set { _slider3Dmodel = value; } }
        public GeometryModel3D slider3Dmodel2 { get { return _slider3Dmodel2; } set { _slider3Dmodel2 = value; } }

        //Used for binding with a slider in the UI in order to allow user to move the region of interest
        public double sliderZposition
        {
            get { return _sliderZposition; }
            set
            {
                _sliderZposition = value;
                DisplaySlider();
            }
        }

        //Used for binding with the slider that is moving the region of interest
        public double maximumSliderZposition { get { return _maximumSliderZposition; } set { _maximumSliderZposition = value; } }
        public double minimumSliderZposition { get { return _minimumSliderZposition; } set { _minimumSliderZposition = value; } }

        //Used for binding with the slider defining number of slices for the ROI slider
        public double numberOfROISlices { get { return _numberOfROISlices; } set { _numberOfROISlices = value; DisplaySlider(); } }

        //Used for binding of items source for the ComboBox
        public IEnumerable<string> structureIDsIEnumerable => _structureIDsCollection.OrderBy(i => i);

        //Used for asyn running tasks
        public EsapiWorker esapiWorker = null;

        //IEnumerable of all available structureSets for the selected patient
        public IEnumerable<string> availableStructureSetIDs { get { return _availableStructureSetIDs; } set { _availableStructureSetIDs = (ObservableCollection<string>)value; } }

        //List of all available meshes
        public IEnumerable<Structure3DModelViewModel> structures3DViewModels => _structures3DViewModels;

        //Used for binding with the selected item in the structure set ComboBox.
        //Creates observable collection of all available structures and mesh geometries when the selection changes
        public string currentStructureSetId
        {
            get { return _currentStructureSetId; }
            set
            {
                _currentStructureSetId = value;

                //Grab current structureSet ID from the selected structure set, populate list of structures
                GetAllStructureIDsFromStructureSet();
                CreateStructures3DModels();
            }
        }
        #endregion
        #region Public commands
        //Command to cancel selection of the region to postprocess
        public ICommand cancelSelectedSlice { get; set; }

        //Command to defie selection of the region to postprocess
        public ICommand selectSliceCommand { get; set; }

        //Command to close the window
        public ICommand exitCommand { get; set; }

        //Command to duplicate the selected structure in the selected structure set
        public ICommand duplicate3DModelCommand { get; set; }

        //Command to launch the postrocessing algoritm on the selected region
        public ICommand repairSliceCommand { get; set; }
        #endregion
        #region Constructor
        public Structure3DViewerViewModel(EsapiWorker _esapiWorker = null)
        {
            Logger.LogInfo("Initializing 3D viewer view model");
            //Define current dispatcher and EsapiWorker class
            esapiWorker = _esapiWorker;
            _userInterface = Dispatcher.CurrentDispatcher;

            Logger.LogInfo("Reading all available structure sets");
            FindAllAvailableStructureSets();

            Logger.LogInfo("Reading all available structures for the selected structure set");
            GetAllStructureIDsFromStructureSet();

            Logger.LogInfo("Reading all available mesh geometries for the selected structure set");
            CreateStructures3DModels();

            #region Initialize commands
            cancelSelectedSlice = new RelayCommand(null);
            selectSliceCommand = new RelayCommand(null);
            exitCommand = new RelayCommand(Exit);
            duplicate3DModelCommand = new RelayCommand(null);
            repairSliceCommand = new RelayCommand(PostprocessTheStructure);
            #endregion

        }

        #endregion
        #region Private methods
        /// <summary>
        /// This method creates a list of all available mesh geometries
        /// </summary>
        private async void CreateStructures3DModels()
        {
            try
            {
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {
                    var _currentStructureSet = _patient.StructureSets.FirstOrDefault(x => x.Id == _currentStructureSetId);

                    foreach (string structureID in _currentStructureSet.Structures.Select(x => x.Id))
                    {
                        Structure structureToAdd = _currentStructureSet.Structures.FirstOrDefault(x => x.Id == structureID);
                        string _structureID = structureToAdd.Id;
                        Color _structureColor = structureToAdd.Color;
                        MeshGeometry3D _structureMesh = new MeshGeometry3D();
                        if (structureToAdd.MeshGeometry != null)
                            _structureMesh = structureToAdd.MeshGeometry.CloneCurrentValue();
                        else _structureMesh = null;

                        var modelToAdd = new Structure3DModelViewModel(new Structure3Dmodel(_structureID, _structureColor, _structureMesh));

                        _userInterface.Invoke(() =>
                        {
                            _structures3DViewModels.Add(modelToAdd);
                        });
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
        ///Find all available structure sets for the selected patient
        /// </summary>
        private async void FindAllAvailableStructureSets()
        {
            Logger.LogInfo("Called a method to read all structure sets for the selected patient");
            try
            {
                await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
                {
                    Logger.LogInfo(string.Format("Patient ID is:{0}", _patient.Id));

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

                    _userInterface.Invoke(() =>
                    {
                        foreach (string structureSetID in _availableStructureSetIDsCollection)
                        {
                            var check = _availableStructureSetIDs.FirstOrDefault(x => x == structureSetID);
                            if (check == null) _availableStructureSetIDs.Add(structureSetID);
                        }
                        Logger.LogInfo(string.Format("...Selecting default structure set: {0}", currentStructureSetId));

                        if (_currentStructureSetId == null || _currentStructureSetId.Length < 2)
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
                    var _currentStructureSet = _patient.StructureSets.FirstOrDefault(x => x.Id == _currentStructureSetId);

                    foreach (string structureID in _currentStructureSet.Structures.Select(x => x.Id))
                    {
                        //Invoke dispatcher to enable collection updates from a different thread
                        _userInterface.Invoke(() =>
                        {
                            try
                            {
                                _allStructureIDs.Add(structureID);
                                Logger.LogInfo(string.Format("...Structure: {0} has been added to the collection of all available structure IDs", structureID));
                            }

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

        /// <summary>
        /// This method creates a list of all available mesh geometries
        /// </summary>
        private async void DisplayStructure3DModel()
        {
            await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
            {
                _workIsInProgress = true;
                _spinnerPhrase = "Loading structure 3D model...";
                //Find the structure object by selected ID in the selected structure set
                Logger.LogInfo(".............................................");
                Logger.LogInfo("Called a method to Display structure 3D model");
                Logger.LogInfo(".............................................");
                var _structureToGrab = _patient.StructureSets.FirstOrDefault(x => x.Id == _currentStructureSetId).Structures.FirstOrDefault(x => x.Id == _selectedStructureID);

                //Define the coordinates of the geometrical center of the structure
                var _centerOfTheStructure = _structureToGrab.CenterPoint;

                Logger.LogInfo(string.Format("Structure ID:{0} in the StructureSet:{1}", _selectedStructureID, _currentStructureSetId));

                //Define color and geometry of the structure
                Logger.LogInfo("Defining color of the structure");
                var _color = _structureToGrab.Color;

                Logger.LogInfo("Defining MeshGeometry of the structure");
                var _mesh = _structureToGrab.MeshGeometry.Clone();

                //Create collections to be passed into the UI thread
                Logger.LogInfo("Generating a collection of meshPositions");

                var _meshPositions = new ObservableCollection<Point3D>();
                if (_mesh.Positions != null)
                    foreach (var point in _mesh.Positions) { _meshPositions.Add(new Point3D(point.X, point.Y, point.Z)); }

                Logger.LogInfo("Generating a collection of meshTriangles");
                var _meshTriangles = new ObservableCollection<Int32>();
                if (_mesh.TriangleIndices != null)
                    foreach (var index in _mesh.TriangleIndices) { _meshTriangles.Add(index); }

                Logger.LogInfo("Generating a collection of meshNormals");
                var _meshNormals = new ObservableCollection<Vector3D>();
                if (_mesh.Normals != null)
                    foreach (var normal in _mesh.Normals) { _meshNormals.Add(new Vector3D(normal.X, normal.Y, normal.Z)); }

                Logger.LogInfo("Generating a collection of meshTextureCoordinates");
                var _meshTextureCoordinates = new ObservableCollection<Point>();
                if (_mesh.TextureCoordinates != null)
                    foreach (var textureCoordinate in _mesh.TextureCoordinates) { _meshTextureCoordinates.Add(new Point(textureCoordinate.X, textureCoordinate.Y)); }

                Logger.LogInfo("Copying bounds of the selected mesh");
                var meshBounds = _mesh.Bounds;

                Logger.LogInfo(string.Format("Reading volume of {0}", _structureToGrab.Id));
                var _structureVolumeString = Math.Round(_structureToGrab.Volume, 2).ToString() + "cc";
                try
                {
                    Logger.LogInfo("Invoking UI");
                    _userInterface.Invoke(() =>
                    {
                        Logger.LogInfo("Setting volume of the structure");
                        _structureVolume = _structureVolumeString;
                        Logger.LogInfo(string.Format("Volume = {0} set", _structureVolume));

                        //Define color of our mesh and create empty camera position and mesh geometry
                        Logger.LogInfo("Creating diffure material");
                        _diffuseMaterial = new DiffuseMaterial(new SolidColorBrush(_color));

                        Logger.LogInfo("Creating empty MeshGeometry");
                        _meshGeometry = new MeshGeometry3D();

                        Logger.LogInfo("Creating empty camera position");
                        _cameraPosition = new Point3D();

                        //Copy mesh parameters from the collections to the mesh geometry existing in the UI
                        if (_mesh != null)
                        {
                            Logger.LogInfo("Mesh is not empty check passed");
                            if (_meshPositions != null)
                            {

                            }

                            //Create lists to store values for coordinates
                            List<double> _positionsX = new List<double>();
                            List<double> _positionsY = new List<double>();
                            List<double> _positionsZ = new List<double>();
                            _maximumSliderXposition = 0;
                            _maximumSliderYposition = 0;
                            _maximumSliderZposition = 0;
                            _minimumSliderXposition = 0;
                            _minimumSliderYposition = 0;
                            _minimumSliderZposition = 0;

                            Logger.LogInfo("Pass mesh positions");
                            foreach (var point in _meshPositions)
                            {
                                _meshGeometry.Positions.Add(new Point3D(point.X, point.Y, point.Z));
                                //Add coordinates to lists for further dimensions calculation
                                _positionsX.Add(point.X); _positionsY.Add(point.Y); _positionsZ.Add(point.Z);
                            }

                            if (_meshTriangles != null)
                                Logger.LogInfo("Pass mesh triangles");
                            foreach (var index in _meshTriangles)
                            { _meshGeometry.TriangleIndices.Add(index); }

                            if (_meshNormals != null)
                                Logger.LogInfo("Pass mesh normals");
                            foreach (var normal in _meshNormals)
                            { _meshGeometry.Normals.Add(new Vector3D(normal.X, normal.Y, normal.Z)); }

                            if (_meshTextureCoordinates != null)
                                Logger.LogInfo("Pass mesh testure coordinates");
                            foreach (var textureCoordinate in _meshTextureCoordinates)
                            { _meshGeometry.TextureCoordinates.Add(new Point(textureCoordinate.X, textureCoordinate.Y)); }

                            //Define dimensions of the ROI slider for the selected structure
                            Logger.LogInfo("Defining dimensions of the slider");
                            _maximumSliderXposition = _positionsX.Max();
                            _maximumSliderYposition = _positionsY.Max();
                            _maximumSliderZposition = _positionsZ.Max();
                            _minimumSliderXposition = _positionsX.Min();
                            _minimumSliderYposition = _positionsY.Min();
                            _minimumSliderZposition = _positionsZ.Min();
                            Logger.LogInfo(string.Format("Slider dimensions are: X:{0} {1}; Y:{2} {3}; Z:{4} {5}",
                                _maximumSliderXposition, _minimumSliderXposition,
                                _maximumSliderYposition, _minimumSliderYposition,
                                _maximumSliderZposition, _minimumSliderZposition));

                            //Set slider initial position
                            Logger.LogInfo("Setting slider Z position");
                            _sliderZposition = _minimumSliderZposition;

                            //Define coordinates for arrows
                            Logger.LogInfo("Defining coordinates for arrows");
                            _point1ForArrows = new Point3D(_minimumSliderXposition, _maximumSliderYposition, _minimumSliderZposition);

                            _point2ForXarrow = new Point3D(_minimumSliderXposition, _minimumSliderYposition, _minimumSliderZposition);
                            _point2ForYarrow = new Point3D(_maximumSliderXposition, _maximumSliderYposition, _minimumSliderZposition);
                            _point2ForZarrow = new Point3D(_minimumSliderXposition, _maximumSliderYposition, _maximumSliderZposition);

                            Logger.LogInfo("Defining camera position");
                            _cameraPosition = new Point3D(
                                (_centerOfTheStructure.x + 100/*+ meshBounds.SizeX*/) / 20,
                                (_centerOfTheStructure.y + 100/*+ meshBounds.SizeY*/) / 20,
                                (_centerOfTheStructure.z + 100/*+ meshBounds.SizeZ*/) / 20);

                            Logger.LogInfo("Defining camera look direction");
                            _cameraDirection = new Vector3D(
                                -(_centerOfTheStructure.x + 100/*+ meshBounds.SizeX*/) / 20,
                                -(_centerOfTheStructure.y + 100/*+ meshBounds.SizeY*/) / 20,
                                -(_centerOfTheStructure.z + 100/*+ meshBounds.SizeZ*/) / 20);

                            //Assign defined parameters to the displaying model
                            Logger.LogInfo("Assigning  geometry and material to the 3D view geometry");
                            _structure3DmodelToShow = new GeometryModel3D(_meshGeometry, _diffuseMaterial);

                        }
                    });
                    Logger.LogInfo(".............................................");
                    Logger.LogInfo("...................SUCCESS...................");
                    Logger.LogInfo(".............................................");
                }

                catch (Exception exception)
                {
                    // Log any appeared issues
                    Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                    MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                }
            });
            _workIsInProgress = false;
            _spinnerPhrase = "Structure 3D model successfully loaded!";
        }

        ///<summary>
        ///Method to generate a slider 3D model
        /// </summary>
        private async void DisplaySlider()
        {
            Logger.LogInfo("Called a method to create a slider 3D model");
            await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
            {
                Logger.LogInfo("Invoking user interface");
                try
                {
                    _userInterface.Invoke(() =>
                    {
                        Logger.LogInfo("Setting new diffusive material for the ROI (transparent)");
                        DiffuseMaterial _sliderDiffuseMaterial = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(74, 67, 126)));
                        DiffuseMaterial _sliderDiffuseMaterial2 = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(74, 67, 126)));

                        Logger.LogInfo("Setting new mesh geometry for the ROI");
                        MeshGeometry3D _sliderMesh = new MeshGeometry3D();
                        MeshGeometry3D _sliderMesh2 = new MeshGeometry3D();


                        Logger.LogInfo("Adding mesh positions");

                        _sliderMesh.Positions.Add(new Point3D(_minimumSliderXposition, _minimumSliderYposition, _sliderZposition + _numberOfROISlices));
                        _sliderMesh.Positions.Add(new Point3D(_maximumSliderXposition, _minimumSliderYposition, _sliderZposition + _numberOfROISlices));
                        _sliderMesh.Positions.Add(new Point3D(_maximumSliderXposition, _maximumSliderYposition, _sliderZposition + _numberOfROISlices));
                        _sliderMesh.Positions.Add(new Point3D(_minimumSliderXposition, _maximumSliderYposition, _sliderZposition + _numberOfROISlices));

                        _sliderMesh2.Positions.Add(new Point3D(_minimumSliderXposition, _minimumSliderYposition, _sliderZposition - _numberOfROISlices));
                        _sliderMesh2.Positions.Add(new Point3D(_maximumSliderXposition, _minimumSliderYposition, _sliderZposition - _numberOfROISlices));
                        _sliderMesh2.Positions.Add(new Point3D(_maximumSliderXposition, _maximumSliderYposition, _sliderZposition - _numberOfROISlices));
                        _sliderMesh2.Positions.Add(new Point3D(_minimumSliderXposition, _maximumSliderYposition, _sliderZposition - _numberOfROISlices));

                        Logger.LogInfo("Adding triangle indices");
                        _sliderMesh.TriangleIndices.Add(0);
                        _sliderMesh.TriangleIndices.Add(1);
                        _sliderMesh.TriangleIndices.Add(2);
                        _sliderMesh.TriangleIndices.Add(0);
                        _sliderMesh.TriangleIndices.Add(2);
                        _sliderMesh.TriangleIndices.Add(3);

                        _sliderMesh2.TriangleIndices.Add(0);
                        _sliderMesh2.TriangleIndices.Add(1);
                        _sliderMesh2.TriangleIndices.Add(2);
                        _sliderMesh2.TriangleIndices.Add(0);
                        _sliderMesh2.TriangleIndices.Add(2);
                        _sliderMesh2.TriangleIndices.Add(3);


                        Logger.LogInfo("Assigning defined geometry and diffusive material to rhe ROI 3D model");
                        _slider3Dmodel = new GeometryModel3D(_sliderMesh, _sliderDiffuseMaterial);
                        _slider3Dmodel2 = new GeometryModel3D(_sliderMesh2, _sliderDiffuseMaterial2);
                    });


                }
                catch (Exception exception)
                {
                    // Log any appeared issues
                    Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                    MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                }
            });
        }

        ///<summary>
        ///Method to postprocess selected structure
        /// </summary>
        private async void PostprocessTheStructure()
        {
            await esapiWorker.AsyncRunStructureContext((_patient, _structureSet) =>
            {
                Logger.LogInfo("Called a method to repair the defined region for the structure");
                _spinnerPhrase = "Repairing slices in the region of interest...";
                _workIsInProgress = true;
                try
                {
                    #region Enable modifications and create technical structures
                    //Enable modifications for the selected patient
                    Logger.LogInfo(string.Format("Enabling modifications for the patient:{0}", _patient.Id));
                    _patient.BeginModifications();

                    //Reading data for the desired structure set
                    Logger.LogInfo(string.Format("Reading data of the structure set: {0} for the patient: {1}", _currentStructureSetId, _patient.Id));
                    var _selectedStructureSet = _patient.StructureSets.FirstOrDefault(x => x.Id == _currentStructureSetId);

                    //Reading the coordinate of user origin
                    var _userOriginZcoordinate = Convert.ToInt32(_selectedStructureSet.Image.UserOrigin.z);
                    Logger.LogInfo(string.Format("The coordinate of user origin is: {0} ", _userOriginZcoordinate));

                    //Reading the resolution of the CT image
                    var _imageZResolution = _selectedStructureSet.Image.ZRes;
                    Logger.LogInfo(string.Format("The resolution of the CT image is: {0} ", _imageZResolution));

                    //Reading data from the selected structure
                    Logger.LogInfo(string.Format("Pulling data of the structure: {0} from the structure set: {1}", _selectedStructureID, _selectedStructureSet.Id));
                    var _selectedStructure = _selectedStructureSet.Structures.FirstOrDefault(x => x.Id == _selectedStructureID);

                    //Create two placeholder structures: one for storing slices that desired to be unchanged.
                    //The other one is to define the volume that requires corrections.
                    Logger.LogInfo(string.Format("Creating placeholder structures"));
                    Structure _structureToCorrect = _selectedStructureSet.AddStructure("CONTROL", StructureUniqueIdHelper.CheckIfTheStructureIDisUnique("PDtoDel", _selectedStructureSet));
                    Structure _structureCorrected = _selectedStructureSet.AddStructure("CONTROL", StructureUniqueIdHelper.CheckIfTheStructureIDisUnique("PDtoDel", _selectedStructureSet));
                    Logger.LogInfo(string.Format(
                        "Created two structures: '{0}' for storing unchanged slices, and '{1}' for estimation of volume that requires corrections",
                        _structureCorrected.Id, _structureToCorrect.Id));

                    //Define segment volumes of the placeholder structures
                    _structureToCorrect.SegmentVolume = StructureCopyHelper.CopyStructureWithUniformMargin(_structureToCorrect, _selectedStructure, 0);
                    _structureCorrected.SegmentVolume = StructureCopyHelper.CopyStructureWithUniformMargin(_structureCorrected, _selectedStructure, 0);

                    Logger.LogInfo(string.Format("Placeholder structure with Id:{0}, Volume={1}cc has been created in the structure set:{2}", 
                        _structureToCorrect.Id, Math.Round(_structureToCorrect.Volume,2), _selectedStructureSet.Id));
                    #endregion
                    #region Find the last and the first slices of the ROI making calculations from its bounds
                    int _sliceSlider1 =0;
                    int _sliceSlider2=0;
                    int _boundZ1 = 0;
                    int _boundZ2 = 0;
                    var _originZ = _selectedStructureSet.Image.Origin.z;
                    var _sliceCTresolution = _selectedStructureSet.Image.ZRes;

                    _userInterface.Invoke(() =>
                    {
                        //Coordinates of boundaries for the slider
                        _boundZ1 = Convert.ToInt32(_slider3Dmodel.Geometry.Bounds.Z);
                        _boundZ2 = Convert.ToInt32(_slider3Dmodel2.Geometry.Bounds.Z);

                        //Convert coordinates of boundaries to coordinates of the slices recognized by ESAPI
                        _sliceSlider1 = Convert.ToInt32((_boundZ1 + Math.Abs(_originZ)) / _sliceCTresolution);
                        _sliceSlider2 = Convert.ToInt32((_boundZ2 + Math.Abs(_originZ)) / _sliceCTresolution);
                    });
                    #endregion
                    #region Define volume for the structure we are correcting (ROI) and for the structure we keep unchanged
                    ///<summary>
                    /// I find it useful to use ClearAllContoursOnImagePlane() method insted of AddContourOnImagePlane(), 
                    /// as when we add a contour, it is automatically added in default resolution.
                    /// In order to prevent unwanted loss of volume for structures, we will try to avoid AddContourOnImagePlane() further as well
                    /// </summary>
                    //Define the physical volume for the region of interest

                    //Define the first and the last plane of the structure
                    Logger.LogInfo(string.Format("Defining the first and the last slice of the structure:'{0}'", _selectedStructureID));
                    var _firstPlaneForTheStructure = Convert.ToInt32((_point1ForArrows.Z + Math.Abs(_originZ)) / _sliceCTresolution - 1);
                    var _lastPlaneForTheStructure = Convert.ToInt32((_point2ForZarrow.Z + Math.Abs(_originZ)) / _sliceCTresolution + 1);

                    //If the structure is created on all planes, we have to make sure that the value of the first slice is not negative
                    if (_firstPlaneForTheStructure < 0)
                    {
                        for (int i = 1; i < 999; i++)
                        {
                            _firstPlaneForTheStructure = Convert.ToInt32((_point1ForArrows.Z + Math.Abs(_originZ)) / _sliceCTresolution) + i;
                            if (_firstPlaneForTheStructure >= 0) break;
                        }
                    }
                    Logger.LogInfo(string.Format("The first plane of the structure: '{0}' is {1}", _selectedStructureID, _firstPlaneForTheStructure));
                    Logger.LogInfo(string.Format("The last plane of the structure: '{0}' is {1}", _selectedStructureID, _lastPlaneForTheStructure));

                    Logger.LogInfo(string.Format("Deleting excessive slices for:'{0}' volume={1}",
                        _structureToCorrect.Id, Math.Round(_structureToCorrect.Volume, 2)));

                    //Clear all slices outside of the ROI for the structure we will be correcting
                    for (int i = _firstPlaneForTheStructure; i < _lastPlaneForTheStructure; i++)
                    {
                        if (i < _sliceSlider2 + 1 || i > _sliceSlider1 - 1)
                        {
                            try { _structureToCorrect.ClearAllContoursOnImagePlane(i); } catch { }
                        }
                    }
                    Logger.LogInfo(string.Format("Excessive slices deleted for:'{0}', new volume is: {1}cc",
                        _structureToCorrect.Id, Math.Round(_structureToCorrect.Volume, 2)));

                    //Clear all slices inside the ROI for the structure we will not be correcting
                    Logger.LogInfo(string.Format("Deleting excessive slices for:'{0}' volume={1}",
                        _structureCorrected.Id, Math.Round(_structureCorrected.Volume, 2)));

                    _structureCorrected.SegmentVolume = _structureCorrected.Sub(_structureToCorrect);

                    Logger.LogInfo(string.Format("Excessive slices deleted for:'{0}', new volume is: {1}cc",
                        _structureCorrected.Id, Math.Round(_structureCorrected.Volume, 2)));
                    #endregion
                    #region Define volume for the larger, last slice and first slice of the ROI
                    //Find the larger slice in the ROI
                    Structure _largerSlice = _selectedStructureSet.AddStructure("CONTROL", StructureUniqueIdHelper.CheckIfTheStructureIDisUnique("PDtoDel", _selectedStructureSet));

                    //Define initial volume of the larger slice
                    int _planeOfTheLargerSlice = 0;
                    _largerSlice.SegmentVolume = StructureCopyHelper.CopyStructureWithUniformMargin(_largerSlice, _structureToCorrect, 0);
                    for (int i = _firstPlaneForTheStructure; i < _lastPlaneForTheStructure; i++)
                    {
                        if (i != _sliceSlider2 + 1) _largerSlice.ClearAllContoursOnImagePlane(i);
                    }

                    //Compare volume slice by slice the selected slices in the ROI and define volumes of the first slice and the lasat slice
                    Structure _firstSlice = _selectedStructureSet.AddStructure("CONTROL", StructureUniqueIdHelper.CheckIfTheStructureIDisUnique("PDtoDel", _selectedStructureSet));
                    for (int i = _sliceSlider2; i < _sliceSlider1; i++)
                    {
                        _firstSlice.SegmentVolume = StructureCopyHelper.CopyStructureWithUniformMargin(_firstSlice, _structureToCorrect, 0);

                        for (int i2 = _sliceSlider2; i2 < _sliceSlider1; i2++)
                        {
                            if (i2 != i) { try { _firstSlice.ClearAllContoursOnImagePlane(i2); } catch { } }
                        }

                        if (_firstSlice.Volume > _largerSlice.Volume)
                        { _largerSlice.SegmentVolume = StructureCopyHelper.CopyStructureWithUniformMargin(_largerSlice, _firstSlice, 0); _planeOfTheLargerSlice = i; }
                    }
                    Structure _lastSlice = _selectedStructureSet.AddStructure("CONTROL", StructureUniqueIdHelper.CheckIfTheStructureIDisUnique("PDtoDel", _selectedStructureSet));
                    _lastSlice.SegmentVolume = StructureCopyHelper.CopyStructureWithUniformMargin(_lastSlice, _structureToCorrect, 0);
                    for (int i =_sliceSlider1; i > _sliceSlider2; i--)
                    {
                        if(Math.Abs(i)!= Math.Abs(_sliceSlider2) +1) try { _lastSlice.ClearAllContoursOnImagePlane(i); } catch { }
                    }
                    #endregion
                    #region Smoothing logic
                    Structure test = _selectedStructureSet.AddStructure("CONTROL", StructureUniqueIdHelper.CheckIfTheStructureIDisUnique("TEST", _selectedStructureSet));
                    test.SegmentVolume = StructureCopyHelper.CopyStructureWithUniformMargin(test, _largerSlice, 0);
                    test.SegmentVolume = test.Or(_firstSlice);
                    test.SegmentVolume = test.Or(_lastSlice);

                    #region Create slices in between slices: the first slice, the last slice and the larger slice
                    //Define volume difference and margin required for the smoothed slices
                    Structure checkSliceVolume = _selectedStructureSet.AddStructure(
                        "CONTROL", StructureUniqueIdHelper.CheckIfTheStructureIDisUnique("PDtoDel",_selectedStructureSet));
                    //For the first slice:
                    checkSliceVolume.SegmentVolume = StructureCopyHelper.CopyStructureWithUniformMargin(checkSliceVolume, _firstSlice, 0);
                    int marginForTheFirstSlice = 1;
                    for (int i=0;i<999;i++) 
                    {
                        checkSliceVolume.SegmentVolume = StructureCopyHelper.CopyStructureWithMargin(
                            checkSliceVolume, 
                            checkSliceVolume,
                            StructureMarginGeometry.Outer,1, 1,0,1,1,0);

                        marginForTheFirstSlice = marginForTheFirstSlice + i;

                        var checkVolume = _largerSlice.Volume / checkSliceVolume.Volume;

                        if (checkVolume > 0.9 && checkVolume < 1.1) break;
                    }

                    //For the last slice:
                    checkSliceVolume.SegmentVolume = StructureCopyHelper.CopyStructureWithUniformMargin(checkSliceVolume, _lastSlice, 0);
                    int marginForTheLastSlice = 1;
                    for (int i = 0; i < 999; i++)
                    {
                        checkSliceVolume.SegmentVolume = StructureCopyHelper.CopyStructureWithMargin(
                            checkSliceVolume,
                            checkSliceVolume,
                            StructureMarginGeometry.Outer, 1, 1, 0, 1, 1, 0);

                        marginForTheLastSlice = marginForTheLastSlice + i;
                        var volumeOfLargerSlice = _largerSlice.Volume;
                        var checkVolume = _largerSlice.Volume / checkSliceVolume.Volume;

                        if (checkVolume > 0.9 && checkVolume < 1.1) break;
                    }
                    _selectedStructureSet.RemoveStructure(checkSliceVolume);


                    int j1 = -1;
                    var overallNumberOfSlices = _sliceSlider1 - _sliceSlider2;
                    for (int i= _sliceSlider2; i< _sliceSlider1; i++) 
                    {
                        int j = i - _sliceSlider2;
                        
                        var imageZresolution = _selectedStructureSet.Image.ZRes;
                        Structure bottomToTop = _selectedStructureSet.AddStructure("CONTROL", StructureUniqueIdHelper.CheckIfTheStructureIDisUnique("PDtoDel", _selectedStructureSet));
                        Structure largerSliceToBottom = _selectedStructureSet.AddStructure("CONTROL", StructureUniqueIdHelper.CheckIfTheStructureIDisUnique("PDtoDel", _selectedStructureSet));

                        if (i < _planeOfTheLargerSlice)
                        {
                            bottomToTop.SegmentVolume = StructureCopyHelper.CopyStructureWithUniformMargin(bottomToTop, _lastSlice, 0);
                            var centerPointsDifferenceToApplyX = -j * ((bottomToTop.CenterPoint.x - _largerSlice.CenterPoint.x) / (_planeOfTheLargerSlice - _sliceSlider2));
                            var centerPointsDifferenceToApplyY = -j * ((bottomToTop.CenterPoint.y - _largerSlice.CenterPoint.y) / (_planeOfTheLargerSlice - _sliceSlider2));

                            bottomToTop.SegmentVolume = MoveXYZStructure(
                                centerPointsDifferenceToApplyX,
                                centerPointsDifferenceToApplyY,
                                imageZresolution + (j * imageZresolution),
                                _selectedStructureSet,
                                bottomToTop, bottomToTop);

                            var margin = j * (marginForTheLastSlice / (_planeOfTheLargerSlice - _sliceSlider2));
                            var margin2 = 0;
                            var margin3 = 0;
                            if (margin == 0) margin = 1;
                            if (margin > 50)
                            {
                                margin2 = margin - 50;
                                margin = 50;
                                if (margin2 > 50)
                                {
                                    margin3 = margin2 - 50;
                                    margin2 = 50;
                                }
                            }

                            bottomToTop.SegmentVolume = StructureCopyHelper.CopyStructureWithMargin(
                                bottomToTop, bottomToTop,
                                StructureMarginGeometry.Outer,
                                margin, margin, 0, margin, margin, 0);

                            bottomToTop.SegmentVolume = StructureCopyHelper.CopyStructureWithMargin(
                                bottomToTop, bottomToTop,
                                StructureMarginGeometry.Outer,
                                margin2, margin2, 0, margin2, margin2, 0);

                            bottomToTop.SegmentVolume = StructureCopyHelper.CopyStructureWithMargin(
                                bottomToTop, bottomToTop,
                                StructureMarginGeometry.Outer,
                                margin3, margin3, 0, margin3, margin3, 0);
                        }
                        if (i > _planeOfTheLargerSlice)
                        {
                            j1 = j1 + 1;
                            int j2 = j1;
                            if (j2 == 0) j2 = 1; ;
                            largerSliceToBottom.SegmentVolume = StructureCopyHelper.CopyStructureWithUniformMargin(largerSliceToBottom, _firstSlice, 0);
                            var centerPointsDifferenceToApplyX = -j2 * ((largerSliceToBottom.CenterPoint.x - _largerSlice.CenterPoint.x) / (_sliceSlider1 - _planeOfTheLargerSlice));
                            var centerPointsDifferenceToApplyY = -j2 * ((largerSliceToBottom.CenterPoint.y - _largerSlice.CenterPoint.y) / (_sliceSlider1 - _planeOfTheLargerSlice));

                            largerSliceToBottom.SegmentVolume = MoveXYZStructure(
                                centerPointsDifferenceToApplyX,
                                centerPointsDifferenceToApplyY,
                                -imageZresolution - (j1 * imageZresolution),
                                _selectedStructureSet,
                                largerSliceToBottom, largerSliceToBottom);

                            var margin = j2 * (marginForTheFirstSlice / (_sliceSlider1 - _planeOfTheLargerSlice));
                            if (margin == 0) margin = 1;
                            var margin2 = 0;
                            var margin3 = 0;
                            if (margin > 50)
                            {
                                margin2 = margin - 50;
                                margin = 50;
                                if (margin2 > 50)
                                {
                                    margin3 = margin2 - 50;
                                    margin2 = 50;
                                }
                            }

                            largerSliceToBottom.SegmentVolume = StructureCopyHelper.CopyStructureWithMargin(
                                largerSliceToBottom, largerSliceToBottom,
                                StructureMarginGeometry.Outer,
                                margin, margin, 0, margin, margin, 0);
                            
                        }


                        var testBottomVolume = bottomToTop.Volume;
                        var testUpperolume = largerSliceToBottom.Volume;

                        test.SegmentVolume = test.Or(bottomToTop);
                        test.SegmentVolume = test.Or(largerSliceToBottom);
                        var testVolumeTest = test.Volume;
                        _selectedStructureSet.RemoveStructure(bottomToTop);
                        _selectedStructureSet.RemoveStructure(largerSliceToBottom);
                    }



                    
                    Logger.LogInfo(string.Format("Volume of the larger slice in the ROI defined as: {0}cc", Math.Round(_largerSlice.Volume,2)));



                    #endregion
                    //Define structure to save ID
                    var newStructureId = _selectedStructure.Id;
                    if (newStructureId.ToLower().Contains("_auto")==false) newStructureId = newStructureId + "_auto";

                    _structureCorrected.Id = StructureUniqueIdHelper.CheckIfTheStructureIDisUnique(newStructureId, _selectedStructureSet);
                    if (_structureCorrected.IsHighResolution == true) test.ConvertToHighResolution();
                    _structureCorrected.SegmentVolume = _structureCorrected.Or(test);
                    //Get rid of a junk structures
                    _selectedStructureSet.RemoveStructure(test);
                    _selectedStructureSet.RemoveStructure(_lastSlice);
                    _selectedStructureSet.RemoveStructure(_structureToCorrect);
                    _selectedStructureSet.RemoveStructure(_firstSlice);
                    _selectedStructureSet.RemoveStructure(_largerSlice);
                    #endregion
                }

                catch (Exception exception)
                {
                    // Log any appeared issues
                    Logger.LogError(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                    MessageBox.Show(string.Format("{0}\r\n{1}\r\n{2}", exception.Message, exception.InnerException, exception.StackTrace));
                }
                _workIsInProgress = false;
            });
            GetAllStructureIDsFromStructureSet();
            CreateStructures3DModels();
        }
        /// <summary>
        /// This method movet a structure only in X,Y plane
        /// </summary>
        public SegmentVolume MoveXYZStructure(double x, double y,double z, StructureSet structureSet, Structure structureToMove, Structure structureToSave)
        {
            //Fields used for reporting in the logger
            var initialVolume = Math.Round(structureToMove.Volume, 2);
            var initialCenterPoint = structureToMove.CenterPoint;
            var displacement = new VVector(x, y, z);

            Logger.LogInfo(string.Format(
                $"Called a method to move " +
                $"the structure: '{structureToMove.Id}' " +
                $"with the displacement of: " +
                $"x={Math.Round(displacement.x, 2)}, " +
                $"y={Math.Round(displacement.y, 2)}, " +
                $"z={Math.Round(displacement.z, 2)}"));

            var segmentVolumeToReturn=structureToSave.SegmentVolume = new ESAPIvectorsHelper(displacement, structureToMove, structureSet).MoveStructure();


            Logger.LogInfo(string.Format($"The structure '{structureToMove.Id}' moved from " +
                $"the coordinate: " +
                $"x={Math.Round(initialCenterPoint.x, 2)}," +
                $"y={Math.Round(initialCenterPoint.y, 2)}," +
                $"z={Math.Round(initialCenterPoint.z, 2)} " +
                $"(initial structure volume={initialVolume}) " +
                $"to the coordinate: " +
                $"x={Math.Round(structureToMove.CenterPoint.x, 2)}," +
                $"y={Math.Round(structureToMove.CenterPoint.y, 2)}," +
                $"z={Math.Round(structureToMove.CenterPoint.z, 2)} " +
                $"(new structure volume={Math.Round(structureToMove.Volume, 2)}) " +
                $"with a displacement x={Math.Round(displacement.x, 2)}, y={Math.Round(displacement.y, 2)} "));

            return segmentVolumeToReturn;
        }

        private void Exit()
        {
            Logger.LogInfo("....Called a command to shut down the script.....");
            Script.mainWindow?.Close();
        }
        #endregion
    }
}


