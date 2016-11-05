set wixdir=C:\Program Files\Windows Installer XML v3.5\bin
"%wixdir%\candle.exe" -ext "%wixdir%\WixUtilExtension.dll" wixsetup2.wxs
"%wixdir%\light.exe" -ext "%wixdir%\WixUtilExtension.dll" -out wixsetup2.msi wixsetup2.wixobj