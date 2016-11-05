Demonstrates using XmlPreprocess as a custom action in WiX. (Note that WiX now contains XmlFile and XmlConfig which can often be used to do the same thing for simple things.

Sample 1 - Wixsetup.msi

Build wixsetup.msi by running createsetup.bat

Install the application passing in SERVICELOCATION=somevalue to the installer to see the value processed into the config file.

Here is a sample install command line (including detailed logging)
  wixsetup.msi /l*v wixsetup.log SERVICELOCATION=somevalue

Installs by default to:
  C:\Program Files\XMLPreprocess WiX Sample

You can quickly uninstall with this command
  msiexec /x wixsetup.msi




Sample 2 - Wixsetup2.msi

Is probably a better way using a Custom Action built-in to WiX, this sample does distribute the spreadsheet and XMLPreprocess as well.

Build wixsetup2.msi by running createsetup2.bat

Install the application passing in ENVIRONMENT=Production to the installer to see the value processed into the config file.

Here is a sample install command line (including detailed logging)
  wixsetup2.msi /l*v wixsetup2.log ENVIRONMENT=Production

Installs by default to:
  C:\Program Files\XMLPreprocess WiX Sample2

You can quickly uninstall with this command
  msiexec /x wixsetup2.msi