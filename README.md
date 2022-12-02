# Update CS Project

Welcome to the Update CS Project repository.

## Overview

Update CS Project allows batch updates to C# project files. 
There are a few main use cases, that all work in batch mode so you can update 100s of project files in one go
1. Adding a code analyzer, that performs static code analysis checks on a project
2. Updating a code analyzer to the latest version
3. Generating a GlobalSuppressions file. This may be required after introducing or updating an analyzer, since that may lead to 100s or 1000s of violations that break the build and cannot be addressed immediately.

Using this tool ensures consistency in enterprise archives, without the need for active checking / gating.
## State

This repository is under Active development and used to maintain an enterprise archive with over 250 csproj files.

## Usage

### Example (WIP)
* Ensure this project is built (see below)
* Add the FxCop analyzers with the command 
```UpdateCsProject.exe . --addanalyzer Microsoft.CodeAnalysis.NetAnalyzers 6.0.0```
* Please build your projects. Most likely it will fail to compile, because the analyzers flag issues
* Add the GlobalSuppressions with the command
```UpdateCsProject.exe . --suppress```

### Command line options
Update CS Project provides a extensible mechanism, which can be configured with the following command line options:

| Command switch                 | Description                                                                    |
|--------------------------------|--------------------------------------------------------------------------------|
| --addanalyzer                  | Add a given analyzer to the references                                         |
| --suppress                     | Suppress all existing Code Analyzer errors in the GlobalSuppressions.cs file.  |
| --updateinvalidsuppressionpath | Delete or moves the GlobalSuppressions file from Properties folder to the root.|
| --removeproperty               | Remove the specified property.                                                 |

## How to Build
* Ensure you have a recent version of Visual Studio
* `dotnet build Source\UpdateCSProject.sln`
* The path to the executable is: 'Source\UpdateCSProject\bin\debug\net472\UpdateCsProject.exe'

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
