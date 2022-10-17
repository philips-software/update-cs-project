// Copyright Koninklijke Philips N.V. 2022

using System;

namespace Philips.Tools.UpdateCsProject.Extensions {
    public static class StringExtensions {
        public static bool EqualsIgnoreCase(this string inputString, string compareWith) {
            if (string.IsNullOrWhiteSpace(inputString)) {
                return string.IsNullOrWhiteSpace(compareWith);
            }

            return inputString.Equals(compareWith, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}
