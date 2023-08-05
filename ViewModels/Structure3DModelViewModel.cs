using PD_ScriptTemplate.Helpers;
using PD_ScriptTemplate.Models;
using System;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace PD_ScriptTemplate.ViewModels
{
    public class Structure3DModelViewModel: BaseViewModel
    {
        Structure3Dmodel _structure3Dmodel;
        public string StructureID { get { return _structure3Dmodel.StructureID; } set { _structure3Dmodel.StructureID = value; } }
        public Color StructureColor { get { return _structure3Dmodel.StructureColor; } set { _structure3Dmodel.StructureColor = value; } }
        public MeshGeometry3D MeshGeometry3D { get { return _structure3Dmodel.MeshGeometry3D; } set { _structure3Dmodel.MeshGeometry3D = value; } }
        public Structure3DModelViewModel(Structure3Dmodel structure3Dmodel) 
        {
            _structure3Dmodel = structure3Dmodel ?? throw new ArgumentNullException(nameof(structure3Dmodel));
        }
    }
}
