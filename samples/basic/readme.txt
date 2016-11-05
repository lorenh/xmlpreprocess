Basic sample

This is a simple sample that demonstrates how to perform
substitutions using a single environment switch property which is
passed on the command line. Execute run.bat and compare input.xml
to output-*.xml to see the changes that were performed for each
environment.

One advantage of this method is that all settings are completely
self-contained within the master XML file. No other settings
files are needed.

A disadvantage is that the file gets bloated with lots of comments,
and if you want to target additional environments it involves 
modifying the master input XML file itself. Externalizing the
settings in a settings file is another option. See the "settings"
sample for an example.