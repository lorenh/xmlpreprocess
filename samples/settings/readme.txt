Settings sample

This is a sample that demonstrates how to perform XML
substitutions using multiple external settings files. Execute
run.bat and compare input.xml to output-*.xml to see the changes
that were performed for each environment.

Execute run-clean.bat to generate clean output files without
any markup.

A tool such as Microsoft Excel can be used to manage the settings files.
Open SettingsFileGenerator.xls change a value and press Ctrl+W to
regenerate the settings files.

Make a change to any of the settings-*.xml file and execute
run-reprocess.bat to "reconfigure" your xml files in place. This
is useful for reconfiguring an environment without redeploying.
For example if you want to change trace levels across a whole
set of *.config files.

One advantage of this method is that the input file stays relatively
clean, and doesn't need to be modified when new environments need
to be targeted.