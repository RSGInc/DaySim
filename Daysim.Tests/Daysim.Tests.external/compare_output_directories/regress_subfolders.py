import re
import argparse
import os
import sys
import traceback
import time
import regress_model
import glob
import logging
import run_process_with_realtime_output

def regress_subfolders(parameters):
    start_time = time.perf_counter()
    script_directory = os.path.split(os.path.realpath(__file__))[0] + '/'
    parser = argparse.ArgumentParser(description='Find all files ending with "_regress.xml" or "_regress.properties" and call regress_model.py on each as long as return value is true. Return true if all passed else false.')
    parser.add_argument('--regional_data_directory',
                        help='Directory containing region specific subfolders of data and run configuration files. It is expected that each one of these will be a separate github project',
                        default=script_directory + '../../../../regional_data')
    parser.add_argument("-v", "--verbose", help="increase output verbosity",
                        action="store_true")
    args = parser.parse_args(parameters)

    
    if args.verbose:
        logging.basicConfig(level=logging.DEBUG)

    if logging.getLogger().isEnabledFor(logging.DEBUG):
        print(args)

    if not os.path.isdir(args.regional_data_directory):
        raise Exception('regional_data_directory does not exist: ' + args.regional_data_directory)

    configuration_file_regex = re.compile(r'^.*_regress[.](xml|properties)$')

    regional_data_directory = os.path.normpath(os.path.abspath(args.regional_data_directory))

    regression_file_paths = glob.glob(regional_data_directory + r'/**/*_regress.*', recursive=True)
    regress_model_successful = True
    for regression_file_path in regression_file_paths:
        if os.path.isfile(regression_file_path):
            regression_file_folder, regression_filename = os.path.split(regression_file_path)
            if regression_filename.startswith('archive_'):
                continue
            print('Found file for regression: ' + regression_filename + ' in folder ' + regression_file_folder)
            regression_file_root, ext = os.path.splitext(regression_filename)

            if ext in ['.xml','.properties']:
                function_parameters = ['--configuration_file', regression_file_path
                                      ]
                if args.verbose:
                    function_parameters.append('-v')
                regress_model_successful = regress_model.regress_model(function_parameters)
            elif ext == '.py':
                return_code = run_process_with_realtime_output.run_process_with_realtime_output('python ' + regression_file_path)
                regress_model_successful = return_code == 0
            elif ext in ['.r','.R']:
                return_code = run_process_with_realtime_output.run_process_with_realtime_output('RScript ' + regression_file_path)
                regress_model_successful = return_code == 0
            else:
                raise Exception('File type not supported for regression due to unrecognized extension: ' + regression_file_path + ' in folder "' + regional_data_directory + '"')

            logging.debug('regress_model_successful after running file "' + regression_file_path + '": ' + str(regress_model_successful))
            if not regress_model_successful:
                break

    return regress_model_successful

if __name__ == "__main__":
    try:
        regress_subfolders_successful = regress_subfolders(sys.argv[1:])
        sys.exit(0 if regress_subfolders_successful else 1)
    except Exception as ex:
        print("Exception in user code:")
        print("-"*60)
        traceback.print_exc(file=sys.stdout)
        print("-"*60)
        sys.exit(ex)
