using System.Windows.Media;
using System.Windows.Media.Media3D;
using PD_ScriptTemplate.Helpers;

namespace PD_ScriptTemplate.Models
{
    public class Structure3Dmodel
    {
        public string StructureID { get; set; }
        public Color StructureColor { get; set; }
        public MeshGeometry3D MeshGeometry3D { get;set;}
        public Structure3Dmodel(string structureID, Color structureColor, MeshGeometry3D meshGeometry3D)
        {
            StructureID = structureID;
            StructureColor = structureColor;

            if (meshGeometry3D != null)
                MeshGeometry3D = CopyMeshGeometry.CopyMeshGeometryHelper(meshGeometry3D);
            else MeshGeometry3D = new MeshGeometry3D();
        }
    }
}
