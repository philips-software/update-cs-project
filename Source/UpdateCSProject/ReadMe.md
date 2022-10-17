# How To:

## Add a new command line switch
- Add a new command string in `Arguments` class and add same to list of known commands in constructor
-- Update help about new command line switch in `PrintSupportedArguments` method of `Arguments` class
-- Optional: Update `Validate` method of `Arguments` class. Required only if your command requries additional command line inputs (Other than root folder containing path)
-- _eg. Refer `DummyCommand` field & its usage in `Arguments` class_
- Introduce a new internal class which implements `ICsProjectUpdater` interface and update `Create` method of `ICsProjectUpdaterFactory` class to return same
-- eg. Refer `CsProjectNullUpdater` class and check its usage in `ICsProjectUpdaterFactory` class

## Debug startup
If you introudeced a new command like --dummyCommand and want to debug startup, the please use below command line
```
UpdateCSProject.exe c:\views\trunk --debug --dummyCommand additionalInput1 additionalInput2
```