using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PD_ScriptTemplate.Helpers
{
    /// <summary>
    /// This Class is required to enable communication between variables stored in separate classes.
    /// For example: To update a collection of StructureViewModels when a parameter of one of the StructureViewModel stored in a StructureModel changed
    /// </summary>

    public static class Communicator
    {
        public static event EventHandler<string> StringChanged;

        public static void PublishStringChanged(string newValue)
        {
            StringChanged?.Invoke(null, newValue);
        }
    }
}
