// Copyright Koninklijke Philips N.V. 2021

using Microsoft.Build.Locator;

namespace Philips.Tools.UpdateCsProject {
    /// <summary>
    /// Entry point
    /// </summary>
    internal static class Program {
        static void Main(string[] args) {
            var arguments = new Arguments(args);
            if (!arguments.Validate()) {
                Arguments.PrintSupportedArguments();
                return;
            }

            MSBuildLocator.RegisterDefaults();

            var factory = new ICsProjectUpdaterFactory(arguments);
            var csProjectUpdater = factory.Create();
            var csProjectsUpdater = new BulkCsProjectUpdater(arguments, csProjectUpdater);
            csProjectsUpdater.Update();
            var csProjectsUpdaterSummaryPrinter = new CsProjectsUpdaterSummaryPrinter(csProjectsUpdater);
            csProjectsUpdaterSummaryPrinter.Print();
        }
    }
}
