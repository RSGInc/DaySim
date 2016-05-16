import argparse
import os
import filecmp
import sys
import traceback

def are_all_files_common_func(dcmp):
    """This will return true if the dcmp object passed in shows
    that both directories had the same files and subfolders (recursively)"""
    if len(dcmp.left_only) > 0:
        return False
    elif len(dcmp.right_only) > 0:
       return False

    for sub_dcmp in dcmp.subdirs.values():
        return are_all_files_common(sub_dcmp)

    return True

def get_all_common_different_files(dcmp):
    """This will return list of all files which are different or funny
    from the folders and subfolders (recursively)"""

    diff_files = list(dcmp.diff_files)
    diff_files.extend(dcmp.funny_files)

    for sub_dcmp in dcmp.subdirs.values():
        diff_files.extend(get_all_common_different_files(sub_dcmp))

    return True


def print_diff_files(dcmp):
    for name in dcmp.diff_files:
        print("diff_file %s found in %s and %s" % (name, dcmp.left,
              dcmp.right))
    for sub_dcmp in dcmp.subdirs.values():
        print_diff_files(sub_dcmp)

def are_outputs_equal_func():
    parser = argparse.ArgumentParser(description='Compare two Daysim output directories')
    parser.add_argument('model_directory',
                        help='Directory containing both the "gold" (expected)  and "test" (current) output directories')
    parser.add_argument('--outputs_reference', help='The reference saved outputs from a successful run [default: %(default)s}', default='outputs_reference')
    parser.add_argument('--outputs', help='Newly generated result to be compared to reference [default: %(default)s}', default='outputs')
    args = parser.parse_args()

    if not os.path.isdir(args.model_directory):
        raise Exception('Model directory does not exist: ' + args.model_directory)

    outputs_reference = os.path.join(args.model_directory, args.outputs_reference)

    outputs_new = os.path.join(args.model_directory, args.outputs)

    dcmp = filecmp.dircmp(outputs_reference, outputs_new) 

    are_all_files_common = are_all_files_common_func(dcmp)

    result = True
    if not are_all_files_common:
        result = False
    else:
        all_common_different_files = get_all_common_different_files(dcmp)

    if not result:
        dcmp.report()
    return result
    
if __name__ == "__main__":
    try:
        outputs_are_equal = are_outputs_equal_func()
        sys.exit(0 if outputs_are_equal else 1)
    except Exception as ex:
        sys.exit(ex)
