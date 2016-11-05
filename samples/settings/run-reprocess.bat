REM
REM This sample "reprocesses" the output files in place. The may be useful
REM if you want to make configuration changes across an environment
REM without redeploying. The files must NOT have been preprocessed
REM with the /c switch for this to work or there will be no markup
REM remaining. You can make changes to the settings files and run
REM this batch file to incorporate the changes.
REM
..\..\bin\XmlPreprocess.exe /i output-development.xml /s settings-development.xml
..\..\bin\XmlPreprocess.exe /i output-testing.xml /s settings-testing.xml
..\..\bin\XmlPreprocess.exe /i output-integration.xml /s settings-integration.xml
..\..\bin\XmlPreprocess.exe /i output-production.xml /s settings-production.xml