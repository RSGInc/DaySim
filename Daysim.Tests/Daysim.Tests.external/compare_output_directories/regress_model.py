"""This is passed a directory containing Daysim model data. The directory must have a folder called 'regression_test_data' in it with a configuration_regression.xml.
It will run Daysim and if it finishes successfully it will call compare_output_directories.py"""

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

class Const(Enum):
    REGRESSION_TEST_DIR = "regression_test_data"

def parse_bool(v):
  return v[:1].lower() in ("y", "t", "1")
    
def regress_model():
    start_time = time.perf_counter()
    script_directory = os.path.split(os.path.realpath(__file__))[0] + '/'
    parser = argparse.ArgumentParser(description='Run Daysim regression tests for specified model')
    parser.add_argument('--daysim_exe',
                        help='location of Daysim executable[default: %(default)s}', default= script_directory + '../../../Daysim/bin/x64/Debug/Daysim.exe')
    parser.add_argument('--configuration_file',
                        help='path to configuration file to send to Daysim', default='configuration_regression.xml')
    parser.add_argument('--outputs_reference', help='Path to folder of expected results. Can be relative to folder containing configuration file [default: %(default)s}', default='outputs_reference')
    parser.add_argument('--outputs', help='This does change where data is stored but is the subdirectory where the configuration file specified to store. Existing data will be deleted [default: %(default)s}', default='outputs')
    parser.add_argument('--working', help='This does change where data is stored but is the subdirectory where data is expected to found [default: %(default)s}', default='working')
    parser.add_argument('--delete_existing_working', help='Completely delete the existing working directory before calling Daysim [default: %(default)s}', type=parse_bool, default=False)
    parser.add_argument("-v", "--verbose", help="increase output verbosity",
                        action="store_true")

    args = parser.parse_args()

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

    base_dir, configuration_filename = os.path.split(configuration_file)
    logging.debug('base_dir: ' + base_dir)

    outputs_reference = os.path.join(base_dir, args.outputs_reference)
    logging.debug('outputs_reference: ' + outputs_reference)
    if not os.path.isdir(outputs_reference):
        raise Exception('outputs_reference directory does not exist: ' + outputs_reference)

    #parse config file to know where output and working are being written to
    configuration_file_root, ext = os.path.splitext(configuration_filename)

    if ext != '.xml':
        raise Exception('configuration_file does not end in ".xml" so unlikely to be what you mean: ' + configuration_filename)

    tree = ET.parse(configuration_file)
    root = tree.getroot()
    
    for name, value in root.items():
        if value == 'true' and name.startswith('ShouldUse') and 'ShadowPricing' in name:
            raise Exception('configuration_file has "' + name + '"=' + value + ' but shadow pricing is not yet supported for regression testing.')

    configuration_base_path = root.get('BasePath')
    if configuration_base_path is None:
        configuration_base_path = base_dir
    else:
        configuration_base_path = os.path.normpath(os.path.join(base_dir, configuration_base_path))

    output_subpath = root.get('OutputSubpath')
    outputs_new = os.path.normpath(os.path.join(configuration_base_path, output_subpath))
    logging.debug('outputs_new: ' + outputs_new)
    if os.path.isdir(outputs_new):
        print('Deleting existing outputs directory: ' + outputs_new)
        shutil.rmtree(outputs_new)

    working_directory = root.get('WorkingDirectory')
    if working_directory is not None:
         raise Exception('configuration_file has WorkingDirectory which is deprtecated and not supported for regression testing. Use WorkingSubpath instead')

    working_subpath = root.get('WorkingSubpath')
    working = os.path.normpath(os.path.join(configuration_base_path, working_subpath))
    logging.debug('working: ' + working)
    if args.delete_existing_working and os.path.isdir(working):
        print('Deleting existing working directory: ' + working)
        shutil.rmtree(working)

    try:
        #due to bug Daysim needs to have the cwd be set to configuration_file dir https://github.com/RSGInc/Daysim/issues/52
        old_cwd = os.getcwd()
        os.chdir(base_dir)

        if True:

            import run_process_with_realtime_output
            return_code = run_process_with_realtime_output.run_process_with_realtime_output(daysim_exe + ' --configuration "' + configuration_file + '"')
        else:
            completed_run = subprocess.run(
                 args = [ daysim_exe, '--configuration', configuration_file ],
                 universal_newlines = True,
                 stdout = subprocess.PIPE)
            return_code = completed_run.returncode
            if return_code != 0 or logging.getLogger().isEnabledFor(logging.DEBUG):
                print('Output from Daysim:\n' + completed_run.stdout)

    finally:
        os.chdir(old_cwd)
    
    success = return_code == 0
    regression_passed = success
    if success:
        import compare_output_directories
        outputs_are_equal = compare_output_directories.are_outputs_equal_func(['--model_directory',base_dir
                                                   ,'--outputs_reference', outputs_reference
                                                   ,'--outputs', outputs_new
                                                   , '-v' if args.verbose else ''
                                                  ])
        regression_passed = outputs_are_equal
        if outputs_are_equal:
            logging.debug('Yay! regression test passed!')
        else:
            logging.debug('Boo hoo! regression test failed!')
    return regression_passed
        
"""
   parser.add_argument('--model_directory',
                        help='Directory containing both the "outputs_reference" (expected)  and "outputs" (current) output directories')
    parser.add_argument('--outputs_reference', help='The reference saved outputs from a successful run [default: %(default)s}', default='outputs_reference')
    parser.add_argument('--outputs', help='Newly generated result to be compared to reference [default: %(default)s}', default='outputs')
    parser.add_argument('--max_different_lines_to_show', help='When files differ, how many lines that are different should be output to console?  [default: %(default)s}', type= int, default=5)
    parser.add_argument("-v", "--verbose", help="increase output verbosity",
                        action="store_true")

"""

if __name__ == "__main__":
     try:
        model_regression_successful = regress_model()
        sys.exit(0 if model_regression_successful else 1)
     except Exception as ex:
        print("Exception in user code:")
        print("-"*60)
        traceback.print_exc(file=sys.stdout)
        print("-"*60)
        sys.exit(ex)

