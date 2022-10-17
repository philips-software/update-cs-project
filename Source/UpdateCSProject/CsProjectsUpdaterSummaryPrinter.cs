// Copyright Koninklijke Philips N.V. 2021

namespace Philips.Tools.UpdateCsProject {
    /// <summary>
    /// Prints summary of update operation to console
    /// Update details are fetched from <see cref="BulkCsProjectUpdater"/>
    /// Printing is done via <see cref="ConsoleWriter"/>
    /// </summary>
    internal class CsProjectsUpdaterSummaryPrinter {
        private readonly BulkCsProjectUpdater _csProjectsUpdater;

        public CsProjectsUpdaterSummaryPrinter(BulkCsProjectUpdater csProjectsUpdater) {
            this._csProjectsUpdater = csProjectsUpdater;
        }

        public void Print() {
            ConsoleWriter.WriteMessage("Summary:");
            WriteSkipped();
            WriteCompleted();
            WriteWarnings();
            WriteFailures();
        }

        private void WriteCompleted() {
            ConsoleWriter.WriteMessage("Success:");
            _csProjectsUpdater.Completed.ForEach(ConsoleWriter.WriteSuccess);
        }

        private void WriteFailures() {
            ConsoleWriter.WriteMessage("Failures:");
            _csProjectsUpdater.Failures.ForEach(ConsoleWriter.WriteError);
        }

        private void WriteWarnings() {
            ConsoleWriter.WriteMessage("Warnings:");
            _csProjectsUpdater.Warnings.ForEach(ConsoleWriter.WriteWarning);
        }

        private void WriteSkipped() {
            ConsoleWriter.WriteMessage("Skipped:");
            _csProjectsUpdater.Skipped.ForEach(ConsoleWriter.WriteInfo);
        }
    }
}