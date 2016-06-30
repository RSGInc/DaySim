import argparse
import os
import filecmp
import sys
import traceback
import collections
import time
from enum import Enum
from utilities import *
import logging

#ignore some file extensions
def removeIrrelevantFiles(listOfFiles):
    return [file for file in listOfFiles if not (   file.endswith('.log')
                                                 or file.endswith('.RData'))]

def are_all_files_common_func(dcmp, ignoreLogFiles = True):
    """This will return true if the dcmp object passed in shows
    that both directories had the same files and subfolders (recursively)"""
    if len(dcmp.left_only) > 0:
        if not ignoreLogFiles or len(removeIrrelevantFiles(dcmp.left_only)) > 0:
            return False 
    if len(dcmp.right_only) > 0:
        if not ignoreLogFiles or len(removeIrrelevantFiles(dcmp.right_only)) > 0:
            return False 

    for sub_dcmp in dcmp.subdirs.values():
        return are_all_files_common_func(sub_dcmp)

    return True

def get_all_common_different_files(dcmp):
    """This will return list of all files which are different or funny
    from the folders and subfolders (recursively)"""

    diff_files = list(dcmp.diff_files)
    diff_files.extend(dcmp.funny_files)

    for sub_dcmp in dcmp.subdirs.values():
        diff_files.extend(get_all_common_different_files(sub_dcmp))

    return diff_files

def get_hash_sum_of_lines(filename):
    """this can be used to get a nearly unique identifier for the content of a file
    where order does not matter. Two files with identical linesin different order should have the same hash sum"""
    with open(filename) as infile:
        hash_sum = sum(hash(l) for l in infile)
    return hash_sum



def print_diff_files(dcmp):
    for name in dcmp.diff_files:
        print("diff_file %s found in %s and %s" % (name, dcmp.left,
              dcmp.right))
    for sub_dcmp in dcmp.subdirs.values():
        print_diff_files(sub_dcmp)

def are_outputs_equal(parameters):
    start_time = time.perf_counter()
    parser = argparse.ArgumentParser(description='Compare two Daysim output directories')
    parser.add_argument('--outputs_reference', help='The reference saved outputs from a successful run [default: %(default)s}')
    parser.add_argument('--outputs_new', help='Newly generated result to be compared to reference [default: %(default)s}')
    parser.add_argument('--max_different_lines_to_show', help='When files differ, how many lines that are different should be output to console?  [default: %(default)s}', type= int, default=5)
    parser.add_argument("-v", "--verbose", help="increase output verbosity",
                        action="store_true")
    args = parser.parse_args(parameters)

    if args.verbose:
        logging.basicConfig(level=logging.DEBUG)

    if logging.getLogger().isEnabledFor(logging.DEBUG):
        print(args)

    if not os.path.isdir(args.outputs_reference):
        if not os.path.isdir(args.outputs_new):
            #if neither directory exists then consider them equal
            return True
        raise Exception('outputs_new "' + args.outputs_new + '" exists but not outputs_reference "' + args.outputs_reference + '"')
    elif not os.path.isdir(args.outputs_new):
        raise Exception('outputs_reference "' + args.outputs_reference + '" exists but not outputs_new "' + args.outputs_new + '"')


    dcmp = filecmp.dircmp(args.outputs_reference, args.outputs_new) 

    #logging.debug('dcmp finished')
    #logging.debug('perf_time(): ' + str(time.perf_counter() - start_time))

    are_all_files_common = are_all_files_common_func(dcmp)
    #logging.debug('are_all_files_common finished: ' + str(are_all_files_common))
    #logging.debug('perf_time(): ' + str(time.perf_counter() - start_time))

    if not are_all_files_common:
        result = False
        print("Folders do not have all of the same files so regression fails.")
    else:
        all_common_different_files = get_all_common_different_files(dcmp)
        result = len(all_common_different_files) == 0 #result is good if all common files are the same
        logging.debug('There are #' + str(len(all_common_different_files)) + ' files which are not binary identical. Will look more deeply.')
        #logging.debug('perf_time(): ' + str(time.perf_counter() - start_time))

        for different_file in all_common_different_files:
            result = False #since files are different assume failure unless changed again
            #some Daysim files are identical in content but are output in a different line order
            reference_file = os.path.join(args.outputs_reference, different_file)
            assert os.path.isfile(reference_file), "reference_file is not a file: " + reference_file
            filename, file_extension = os.path.splitext(reference_file)
            allow_text_comparison = file_extension in ['.tsv','dat','.txt']
            new_file = os.path.join(args.outputs_new, different_file)
            assert os.path.isfile(reference_file), "new_file is not a file: " + new_file
            if os.path.getsize(reference_file) != os.path.getsize(new_file):
                logging.debug('length of common file: ' + different_file + ' differs so difference must be more than different sort order!')
            else:
                logging.debug('Common_file that is binary different at least has same file size so, if suitable text file, will check to see if same contents in different order. File: ' + different_file)
                if allow_text_comparison:
                    #since same size need to check if same lines but in different order

                    #quickest and least memory method is to sum the hash of each line and then compare
                    hash_sum_reference = get_hash_sum_of_lines(reference_file)
                    #logging.debug('hash_sum of reference: ' + str(hash_sum_reference))
                    #logging.debug('perf_time(): ' + str(time.perf_counter() - start_time))
                    hash_sum_new_file = get_hash_sum_of_lines(new_file)
                    #logging.debug('hash_sum of new file: ' + str(hash_sum_new_file))
                    #logging.debug('perf_time(): ' + str(time.perf_counter() - start_time))

                    if hash_sum_reference == hash_sum_new_file:
                        print('File "' + different_file + '" has identical content just in different order.')
                        result = True #files count as same despite different order    
                    #else files are different in more than just sort order!

            if result == False:
                if not allow_text_comparison:
                    logging.debug('Files are different but unhandled extension "' + file_extension + '" so cannot check if differ only by line order. Therefore regression fails.')
                else:
                    logging.debug('hash_sum of files is different so going to compare lines. reference_file "' + reference_file + '".')
                    #if the files do not have identical lines get more detailed information of differences
                    with open(reference_file, encoding='latin-1') as infile:
                        counts = collections.Counter(l for l in infile)

                    logging.debug('Finished counting lines in reference folder copy of "' + different_file + '". There are '
                    + str(len(counts)) + ' distinct lines')
                    #logging.debug('perf_time(): ' + str(time.perf_counter() - start_time))
 
                    #logging.debug('deep_getsizeof(counts): ' + human_readable_bytes(deep_getsizeof(counts, set())))
                    #logging.debug('perf_time(): ' + str(time.perf_counter() - start_time))

                    with open(new_file, encoding='latin-1') as infile:
                        counts.subtract(l for l in infile)
                    logging.debug('Finished checking new version of "' + different_file + '".')
                    #logging.debug('perf_time(): ' + str(time.perf_counter() - start_time))

                    missing_from_reference = []
                    missing_from_new = []
                    for line, count in counts.items():
                        if count < 0:
                            missing_from_reference.append((line,count))
                        elif count > 0:
                            missing_from_new.append((line,count))

                    assert len(missing_from_reference) != 0 or len(missing_from_new) != 0, "hash_sum was different but the counts of each distinct are identical!"

                    print('File "' + different_file + '" with ' + str(len(counts)) + ' distinct lines has '
                            + str(len(missing_from_new)) + ' distinct lines that were not found in the new and '
                            + str(len(missing_from_reference)) + ' distinct lines that were not found in the reference file')

                    def print_line_and_counts_to_string(identifier, counted_strings):
                        #sort the missing lines so that the ones shown in reference and new will likely be similar which will make differences easier to spot
                        counted_strings.sort(key=lambda line_count_tuple :  line_count_tuple[0])
                        if len(counted_strings) > 0:
                            message = ('All ' if len(counted_strings) <= args.max_different_lines_to_show else (' Sample ' + str(args.max_different_lines_to_show))) + ' lines that are ' + identifier + '.\n'
                            message += '\n'.join(str(abs(count)) + ': ' + str(line) for line, count in counted_strings[:args.max_different_lines_to_show])
                            print(message)

                    print_line_and_counts_to_string('missing from new file', missing_from_new)
                    print_line_and_counts_to_string('missing from reference', missing_from_reference)

                #logging.debug('perf_time(): ' + str(time.perf_counter() - start_time))
                #STOP!
                break

    if result:
        print('Tests passed. Number of order different files: ' + str(len(all_common_different_files)))
    else:
        dcmp.report()
    return result
    
if __name__ == "__main__":
    try:
        outputs_are_equal = are_outputs_equal(sys.argv[1:])
        sys.exit(0 if outputs_are_equal else 1)
    except Exception as ex:
        print("Exception in user code:")
        print("-"*60)
        traceback.print_exc(file=sys.stdout)
        print("-"*60)
        sys.exit(ex)
