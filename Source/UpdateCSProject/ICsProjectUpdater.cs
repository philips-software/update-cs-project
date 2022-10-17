// Copyright Koninklijke Philips N.V. 2021

namespace Philips.Tools.UpdateCsProject {
    /// <summary>
    /// Interface to update a csproj
    /// </summary>
    internal interface ICsProjectUpdater {
        UpdateStatus Update(CsProject csProject, Arguments arguments);
    }
}