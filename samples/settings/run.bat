REM
REM This sample leaves all markup in emitted files. This allows
REM them to be "reprocessed" in place.
REM
..\..\bin\XmlPreprocess.exe /i input.xml /o output-development.xml /s settings-development.xml
..\..\bin\XmlPreprocess.exe /i input.xml /o output-testing.xml /s settings-testing.xml
..\..\bin\XmlPreprocess.exe /i input.xml /o output-integration.xml /s settings-integration.xml
..\..\bin\XmlPreprocess.exe /i input.xml /o output-production.xml /s settings-production.xml