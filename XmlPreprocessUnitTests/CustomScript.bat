@echo off
echo Settings,Defaults,Local,Test,Integration,Production
echo Setting1,setting1default,,,,
echo Setting2,setting2default,,,,
echo Setting3,setting3default,,,,
echo Setting4,"setting4,default",localhost,testserver,intgserver,prodserver
echo Setting5,,${Setting1},"${Setting1}",${Setting1},${Setting1}
