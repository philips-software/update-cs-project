using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;

namespace Philips.Tools.UpdateCsProject
{
    class CsProjectRemoveProjectProperty : ICsProjectUpdater {
        public UpdateStatus Update(CsProject csProject, Arguments arguments) {
            UpdateStatus result = UpdateStatus.Warn;
            if (csProject.CanLoad())  {
                csProject.Load();

                if (arguments.Input.Length < 1) {
                    return UpdateStatus.Fail;
                }

                var propertyName = arguments.Input[0];
                var propertyValue = (arguments.Input.Length == 2) ? arguments.Input[1] : null;

                if (csProject.HasPropertyInProject(propertyName)) {
                    if (string.IsNullOrWhiteSpace((propertyValue))) {
                        csProject.RemovePropertyInProject(propertyName);
                    }
                    else {
                        csProject.RemovePropertyInProjectWithValue(propertyName, propertyValue);
                    }
                    
                }
                else {
                    result = UpdateStatus.Skipped;
                }
            

                if (csProject.CanSave() && result != UpdateStatus.Skipped) {
                    result = csProject.Save() ? UpdateStatus.Success : UpdateStatus.Fail;
                }
            }

            return result;
        }
    }
}
