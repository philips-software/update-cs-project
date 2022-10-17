// Copyright Koninklijke Philips N.V. 2021

namespace Philips.Tools.UpdateCsProject {
    /// <summary>
    /// Result of update
    /// See <see cref="ICsProjectUpdater.Update"/>
    /// </summary>
    internal enum UpdateStatus {
        Success,
        Warn,
        Fail,
        Skipped
    }
}
