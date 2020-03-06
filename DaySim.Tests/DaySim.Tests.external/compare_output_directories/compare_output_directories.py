import argparse
import os
import filecmp
import sys
import traceback
import collections
import logging

if sys.version_info < (3,0):
    print("Sorry, requires Python 3.x, not Python 2.x")
    sys.exit(1)
sys.path.append(os.path.realpath(os.path.dirname(sys.argv[0])) + '.utilities')
from utilities import *

#ignore some file extensions
def remove_irrelevant_files(listOfFiles):
    return [file for file in listOfFiles if not (   file.endswith('.log')
                                                 or file.endswith('.RData')
                                                 or file.endswith('.Rdata')
                                                 or file.startswith('.git'))]

#modifies the passed in dcmp object recursively to remove all files we don't care about
def remove_irrelevant_files_from_dcmp(dcmp, filter_function=remove_irrelevant_files):
    """This will remove any files we don care about from dcmp results. Needed this because the ignore is does not take wildcard"""
    dcmp.left_list = filter_function(dcmp.left_list)
    dcmp.right_list = filter_function(dcmp.right_list)
    dcmp.left_only = filter_function(dcmp.left_only)
    dcmp.right_only = filter_function(dcmp.right_only)
    dcmp.diff_files = filter_function(dcmp.diff_files)
    dcmp.funny_files = filter_function(dcmp.funny_files)
    dcmp.common_files = filter_function(dcmp.common_files)
    dcmp.common_funny = filter_function(dcmp.common_funny)

    for sub_dcmp in dcmp.subdirs.values():
        remove_irrelevant_files_from_dcmp(sub_dcmp)

def are_all_files_common_func(dcmp):
    """This will return true if the dcmp object passed in shows
    that both directories had the same files and subfolders (recursively)"""
    if len(dcmp.left_only) > 0:
        return False
    if len(dcmp.right_only) > 0:
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
    where order does not matter. Two files with identical lines in different order should have the same hash sum"""
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
    parser = argparse.ArgumentParser(description='Compare two DaySim output directories')
    parser.add_argument('--outputs_reference', help='The reference saved outputs from a successful run [default: %(default)s}')
    parser.add_argument('--outputs_new', help='Newly generated result to be compared to reference [default: %(default)s}')
    parser.add_argument('--max_different_lines_to_show', help='When files differ, how many lines that are different should be output to console?  [default: %(default)s}', type= int, default=5)
    parser.add_argument('-v', '--verbose', help='increase output verbosity',
                        action='store_true')
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

    print('python ' + os.path.realpath(__file__) + ' --outputs_reference "' + os.path.realpath(args.outputs_reference) +  '" --outputs_new "' + os.path.realpath(args.outputs_new) + '"')
    dcmp = filecmp.dircmp(args.outputs_reference, args.outputs_new)
    remove_irrelevant_files_from_dcmp(dcmp)

    are_all_files_common = are_all_files_common_func(dcmp)

    if not are_all_files_common:
        result = False
        print("Folders do not have all of the same files so regression fails.")
        dcmp.report_full_closure()
    else:
        all_common_different_files = get_all_common_different_files(dcmp)
        result = True   #this will be changed to false if any individual file is different in an important way (other than order)
        logging.debug('There are #' + str(len(all_common_different_files)) + ' files which are not binary identical. Will look more deeply.')

        actuallyDifferentFiles = []
        for different_file in all_common_different_files:
            #some DaySim files are identical in content but are output in a different line order
            reference_file = os.path.join(args.outputs_reference, different_file)
            assert os.path.isfile(reference_file), "reference_file is not a file: " + reference_file
            filename, file_extension = os.path.splitext(reference_file)
            allow_text_comparison = file_extension in ['.csv','.dat','.tsv','.txt']
            new_file = os.path.join(args.outputs_new, different_file)
            assert os.path.isfile(reference_file), "new_file is not a file: " + new_file
            #could check file size here with os.path.getsize is concerned about speed but don't bother because want to give more detailed diff if possible
            filesAreDifferent = not allow_text_comparison
            if filesAreDifferent:
                print('Files are different: "' + different_file + '" but do not know how to examine this type of file line by line so must assume different in a significant way!')
            else:
                #quickest and least memory method is to sum the hash of each line and then compare
                hash_sum_reference = get_hash_sum_of_lines(reference_file)
                hash_sum_new_file = get_hash_sum_of_lines(new_file)

                filesAreDifferent = hash_sum_reference != hash_sum_new_file
                if not filesAreDifferent:
                    logging.debug('File "' + different_file + '" has identical content just in different order.')
                else: #files are different in more than just sort order!
                    #print('hash_sum of files is different so going to compare lines. File "' + different_file + '".')
                    #if the files do not have identical lines get more detailed information of differences

                    with open(reference_file, encoding='latin-1') as infile:
                        reference_header = infile.readline()
                        counts = collections.Counter(l for l in infile)

                    logging.debug('Finished counting lines in reference folder copy of "' + different_file + '". There are '
                    + str(len(counts)) + ' distinct lines')

                    with open(new_file, encoding='latin-1') as infile:
                        new_header = infile.readline()
                        counts.subtract(l for l in infile)
                    logging.debug('Finished checking new version of "' + different_file + '".')

                    if reference_header != new_header:
                        print('File headers are different!\nref: ' + reference_header + '\nnew: ' + new_header)
                    else:
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

                        #sort list and only keep top few lines
                        missing_from_reference.sort(key=lambda line_count_tuple :  line_count_tuple[0])
                        missing_from_reference = missing_from_reference[:args.max_different_lines_to_show]

                        missing_from_new.sort(key=lambda line_count_tuple :  line_count_tuple[0])
                        missing_from_new = missing_from_new[:args.max_different_lines_to_show]

                        print('hdr: ' + reference_header.strip('\n'))
                        for missing_line_index in range(0, min(len(missing_from_reference), len(missing_from_new))):
                            print('ref: ' + missing_from_reference[missing_line_index][0].strip('\n') + '\tmissing count: ' +  str(abs(missing_from_reference[missing_line_index][1])))
                            print('new: ' + missing_from_new[missing_line_index][0].strip('\n') + '\tmissing count: ' +  str(abs(missing_from_new[missing_line_index][1])))
                            print('------')
            if filesAreDifferent:
                actuallyDifferentFiles.append(different_file)
            result = result and not filesAreDifferent
            #print('Is "' + different_file + '" actually different?: ' + str(filesAreDifferent) + '. Is regression still passing?: ' + str(result))

        print('There were ' + str(len(all_common_different_files)) + ' that were binary different. Of those, ' + str(len(actuallyDifferentFiles)) + ' files differed in ways that mattered: ' + str(actuallyDifferentFiles))
    if result:
        print('PASSED! :-)')
    else:
        print('FAILED! :-(')
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
