// Copyright Koninklijke Philips N.V. 2021

namespace Philips.Tools.UpdateCsProject {
    /// <summary>
    /// Gets <see cref="ICsProjectUpdater"/> instance based on command
    /// Command is returned by <see cref="Arguments.Command"/>
    /// </summary>
    internal class ICsProjectUpdaterFactory {
        private Arguments _arguments;

        public ICsProjectUpdaterFactory(Arguments arguments) {
            _arguments = arguments;

        }

        public ICsProjectUpdater Create() {
            switch (_arguments.Command) {
                case Arguments.AnalyzersCommand:
                    return new CsProjectCustomAnalyzersUpdater();
                case Arguments.SuppressCommand:
                    return new CsProjectCodeAnalysisSuppressor();
                case Arguments.DummyCommand:
                    return new CsProjectNullUpdater();
                case Arguments.UpdateInvalidSuppressionFilePathCommand:
                    return new CsProjectSuppressionFilePathUpdater();
                case Arguments.UpdateAnalyzerReferenceCommand:
                    return new CsProjectAnalyzerReferenceUpdater();
                case Arguments.RemoveProjectPropertyCommand:
                    return new CsProjectRemoveProjectProperty();
            }
            return new CsProjectNullUpdater();
        }
    }
}