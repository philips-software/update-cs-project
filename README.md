# Update CS Project

Welcome to the Update CS Project repository.

## Overview

Update CS Project allows batch updates to C# project files. This ensures consistency in enterprise archives, without the need for active checking / gating.

## State

This repository is under Active development and used to maintain an enterprise archive with over 250 csproj files.

## Usage

Update CS Project provides a extensible mechanism, which can be configured with the following command line options:

| Command switch                 | Description                                                                    |
|--------------------------------|--------------------------------------------------------------------------------|
| --suppress                     | Suppress all existing Code Analyzer errors in the GlobalSuppressions.cs file.  |
| --analyzers                    | Add a predefined list of mandatory Code Analyzer references.                   |
| --updateinvalidsuppressionpath | Delete or moves the GlobalSuppressions file from Properties folder to the root.|
| --updateanalyzerreference      | Adds or updates the References to Roslyn Analyzers.                            |
| --removeproperty               | Remove the specified property.                                                 |

## Support

This project is maintained by (in alphabetical order):
- [Akshat Agrawal](mailto:akshat.agrawal@philips.com)
- [Mark Venbrux](mailto:mark.venbrux@philips.com)
- [Kiran Ramaiah](mailto:kiran.ramaiah@philips.com)
- [Jan Zwanenburg](mailto:jan.zwanenburg@philips.com)
- [Paul de Feyter](mailto:paul.de.feyter@philips.com)
- [Vishal Srivastava](mailto:vishal.srivastava@philips.com)
- [Ynse Hoornenborg](mailto:ynse.hoornenborg@philips.com)

## Community

This project uses the [CODE_OF_CONDUCT](./CODE_OF_CONDUCT.md) to define expected conduct in our community. Instances of
abusive, harassing, or otherwise unacceptable behavior may be reported by contacting a project [CODEOWNER](./.github/CODEOWNERS)

## Contributing

See [CONTRIBUTING](./CONTRIBUTING.md)

## Licenses

See [LICENSE](./LICENSE)
