// Copyright Koninklijke Philips N.V. 2021

namespace Philips.Tools.UpdateCsProject {
    /// <summary>
    /// NULL update. Does nothing
    /// </summary>
    internal class CsProjectNullUpdater : ICsProjectUpdater {
        public UpdateStatus Update(CsProject csProject, Arguments arguments) {
            ConsoleWriter.WriteInfo($"Skipping update of {csProject.Path} as no/dummy command specified");
            return UpdateStatus.Skipped;
        }
    }
}