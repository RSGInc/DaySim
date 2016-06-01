
import argparse
import os
import filecmp
import sys
import traceback
import collections
import time
from enum import Enum
import shutil
import xml.etree.ElementTree as ET
import logging
import subprocess
import run_process_with_realtime_output
import utilities

class Const(Enum):
    REGRESSION_TEST_DIR = "regression_test_data"

def parse_bool(v):
  return str(v[:1]).lower() in ("y", "t", "1")
    
def regress_model(parameters):
    """Passed a DaySim configuration file, this this renames the existing output directory, runs DaySim and compares the exisiting outputs directory to the new one using compare_output_directories.py"""
    start_time = time.perf_counter()
    script_directory = os.path.split(os.path.realpath(__file__))[0] + '/'
    parser = argparse.ArgumentParser(description='Run Daysim regression tests for specified model')
    parser.add_argument('--daysim_exe',
                        help='location of Daysim executable[default: %(default)s}', default= script_directory + '../../../Daysim/bin/x64/Debug/Daysim.exe')
    parser.add_argument('--configuration_file',
                        help='path to configuration file to send to Daysim', default='configuration_regression.xml')
    parser.add_argument('--run_if_needed_to_create_baseline',
                        help='if the output folder does not exist stting this to true will run it to create the baseline', default=True)
    parser.add_argument("-v", "--verbose", help="increase output verbosity",
                        action="store_true")

    args = parser.parse_args(parameters)

    if args.verbose:
        logging.basicConfig(level=logging.DEBUG)

    if logging.getLogger().isEnabledFor(logging.DEBUG):
        print(args)
    
    logging.debug('Current working directory: ' + os.getcwd())
    logging.debug('script_directory: ' + script_directory)

    daysim_exe =  os.path.normpath(os.path.abspath(args.daysim_exe))
    logging.debug('daysim_exe: ' + daysim_exe)

    if not os.path.isfile(daysim_exe):
        raise Exception('daysim_exe is not a file: ' + daysim_exe)

    configuration_file = os.path.normpath(os.path.abspath(args.configuration_file))
    logging.debug('configuration_file: ' + configuration_file)
    if not os.path.isfile(configuration_file):
        raise Exception('configuration_file is not a file: ' + configuration_file)

    configuration_file_folder, configuration_filename = os.path.split(configuration_file)
    logging.debug('configuration_file_folder: ' + configuration_file_folder)

    #parse config file to know where output and working are being written to
    configuration_file_root, ext = os.path.splitext(configuration_filename)

    if ext != '.xml':
        raise Exception('configuration_file does not end in ".xml" so unlikely to be what you mean: ' + configuration_filename)

    tree = ET.parse(configuration_file)
    root = tree.getroot()
    
    configuration_base_path = root.get('BasePath')
    if configuration_base_path is None:
        configuration_base_path = configuration_file_folder
    else:
        configuration_base_path = os.path.normpath(os.path.join(configuration_file_folder, configuration_base_path))

    output_subpath = root.get('OutputSubpath')
    configured_outpath = os.path.normpath(os.path.join(configuration_base_path, output_subpath))
    logging.debug('configured_outpath: ' + configured_outpath)

    if not os.path.isdir(configured_outpath):
        if args.run_if_needed_to_create_baseline:
            print('configuration_file "' + configuration_file + '" specifies output subpath "' + output_subpath + '" which does not exist. --run_if_needed_to_create_baseline is true so will run now...')
            try:
                #due to bug Daysim needs to have the cwd be set to configuration_file dir https://github.com/RSGInc/Daysim/issues/52
                old_cwd = os.getcwd()
                os.chdir(configuration_file_folder)
                return_code = run_process_with_realtime_output.run_process_with_realtime_output(daysim_exe + ' --configuration "' + configuration_file + '"')
            finally:
                os.chdir(old_cwd)
        raise Exception('configuration_file "' + configuration_file + '" specifies output subpath "' + output_subpath + '" but that folder does not exist so cannot be used for regression.')

    today_regression_results_dir = os.path.join(configuration_base_path, utilities.get_formatted_date() + '_regression_results')
    current_configuration_results_dir_name = utilities.get_formatted_time() + ' ' + configuration_filename
    regression_results_dir = os.path.join(today_regression_results_dir, 'RUNNING_' + current_configuration_results_dir_name)

    os.makedirs(regression_results_dir)
    outputs_new_basename = os.path.basename(configured_outpath)
    outputs_new_dir = os.path.join(regression_results_dir, outputs_new_basename)

    override_parameters = [
                            'OutputSubpath=' + outputs_new_dir,
]
    working_directory = root.get('WorkingDirectory')
    if working_directory is not None:
         raise Exception('configuration_file has WorkingDirectory which is deprecated and not supported for regression testing. Use WorkingSubpath instead')

    working_subpath = root.get('WorkingSubpath')
    working = os.path.normpath(os.path.join(configuration_base_path, working_subpath))
    logging.debug('working: ' + working)
    """if args.delete_existing_working and os.path.isdir(working):
        print('Deleting existing working directory: ' + working)
        shutil.rmtree(working)"""

    try:
        #due to bug Daysim needs to have the cwd be set to configuration_file dir https://github.com/RSGInc/Daysim/issues/52
        old_cwd = os.getcwd()
        os.chdir(configuration_file_folder)

        return_code = run_process_with_realtime_output.run_process_with_realtime_output(daysim_exe + ' --configuration "' + configuration_file + '" --overrides="' + ','.join(override_parameters) + '"')
    finally:
        os.chdir(old_cwd)
    
    regression_passed = return_code == 0
    if regression_passed:
        import compare_output_directories
        function_parameters = ['--outputs_reference', outputs_new_dir
                              ,'--outputs_new', configured_outpath
                              ]

        if args.verbose:
            function_parameters.append('-v')

        outputs_are_equal = compare_output_directories.are_outputs_equal_func(function_parameters)
        regression_passed = outputs_are_equal
    results_label = 'PASSED' if regression_passed else 'FAILED'
    os.rename(regression_results_dir, os.path.join(today_regression_results_dir, results_label + '_' + current_configuration_results_dir_name))
    print('Regression test using configuration file "', configuration_filename, '": ' + results_label)
    return regression_passed
        
if __name__ == "__main__":
     try:
        model_regression_successful = regress_model(sys.argv[1:])
        sys.exit(0 if model_regression_successful else 1)
     except Exception as ex:
        print("Exception in user code:")
        print("-"*60)
        traceback.print_exc(file=sys.stdout)
        print("-"*60)
        sys.exit(ex)

