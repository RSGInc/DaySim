REM this script called by a Jenkins Build Step using command line arguments: "Daysim.Tests\run_tests.bat"
rem return status from python http://stackoverflow.com/a/1013293/283973
python Daysim.Tests\Daysim.Tests.external\compare_output_directories\regress_model.py --configuration_file "C:\DaySim_test_model\regression_test_data\configuration_regression.xml" -v

if %ERRORLEVEL% NEQ 0 {
	goto build_fail
)

REM do more tests here and check error level after each...

if %ERRORLEVEL% NEQ 0 {
	goto build_fail
)

echo ********** BUILD PASSED **********

EXIT /B %ERRORLEVEL%


:build_fail
echo ********** BUILD FAILURE **********
EXIT /B %ERRORLEVEL%
