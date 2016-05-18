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
    parser = argparse.ArgumentParser(description='Run Daysim regression tests for specified model')
    parser.add_argument('--daysim_exe',
                        help='location of Daysim executable[default: %(default)s}', default='./../../../Daysim/bin/x64/Debug/Daysim.exe')
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

    logging.debug(args)
    
    logging.debug('Current working directory: ' + os.getcwd())
    logging.debug('os.path.abspath(args.daysim_exe): ' + os.path.abspath(args.daysim_exe))

    daysim_exe =  os.path.abspath(args.daysim_exe)
    if not os.path.isfile(daysim_exe):
        raise Exception('daysim_exe is not a file: ' + daysim_exe)

    configuration_file = os.path.abspath(args.configuration_file)
    logging.debug('os.path.abspath(args.configuration_file): ' + os.path.abspath(args.configuration_file))
    if not os.path.isfile(configuration_file):
        raise Exception('configuration_file is not a file: ' + aconfiguration_file)

    base_dir, configuration_filename = os.path.split(configuration_file)

    outputs_reference = os.path.join(base_dir, args.outputs_reference)

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

    output_subpath = root.get('OutputSubpath')

    outputs_new = os.path.join(base_dir, output_subpath)
    if os.path.isdir(outputs_new):
        print('Deleting existing outputs directory: ' + outputs_new)
        shutil.rmtree(outputs_new)

    working_directory = root.get('WorkingDirectory')
    if working_directory is not None:
         raise Exception('configuration_file has WorkingDirectory which is deprtecated and not supported for regression testing. Use WorkingSubpath instead')

    working_subpath = root.get('WorkingSubpath')

    working = os.path.join(base_dir, working_subpath)
    if args.delete_existing_working and os.path.isdir(working):
        print('Deleting existing working directory: ' + working)
        shutil.rmtree(working)

    completed_run = subprocess.run(
         args = [ daysim_exe, '--configuration', configuration_file ],
         universal_newlines = True,
         stdout = subprocess.PIPE)
    
    success = completed_run.returncode == 0
    if not success or logging.getLogger().isEnabledFor(logging.DEBUG):
        print('Output from Daysim:\n' + completed_run.stdout)

    if success:
        print('More to do!') 


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

