REM
REM This sample strips all markup from emitted files. This makes
REM for cleaner output files, but prevents being able to "reprocess"
REM them in place.
REM
..\..\bin\XmlPreprocess.exe /c /i input.xml /o output-development-clean.xml /s settings-development.xml
..\..\bin\XmlPreprocess.exe /c /i input.xml /o output-testing-clean.xml /s settings-testing.xml
..\..\bin\XmlPreprocess.exe /c /i input.xml /o output-integration-clean.xml /s settings-integration.xml
..\..\bin\XmlPreprocess.exe /c /i input.xml /o output-production-clean.xml /s settings-production.xml