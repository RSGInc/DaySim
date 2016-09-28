from collections import Mapping, Container
from sys import getsizeof
from datetime import datetime
import time
import os
import re

if sys.version_info < (3,0):
    print("Sorry, requires Python 3.x, not Python 2.x")
    sys.exit(1)

def delete_matching_files(directory, pattern):
    for root, dirs, files in os.walk(directory):
        for file in filter(lambda x: re.match(pattern, x), files):
            os.remove(os.path.join(root, file))

def get_formatted_date_time(date_time_to_format=None):
    if date_time_to_format is None:
        date_time_to_format = datetime.now()
    date_time_str = get_formatted_date(date_time_to_format) + '_' + get_formatted_time(date_time_to_format)
    return date_time_str

def get_formatted_date(date_time_to_format=None):
    if date_time_to_format is None:
        date_time_to_format = datetime.now()
    date_str = date_time_to_format.strftime("%Y-%m-%d")
    return date_str

def get_formatted_time(date_time_to_format=None):
    if date_time_to_format is None:
        date_time_to_format = datetime.now()
    time_str = date_time_to_format.strftime("%Hh%Mm%Ss")
    return time_str

def properties_file_to_dict(property_file_path):
    myprops = {}
    with open(property_file_path, 'r') as f:
        for line in f:
            line = line.rstrip() #removes trailing whitespace and '\n' chars

            if "=" not in line: continue #skips blanks and comments w/o =
            if line.startswith("#"): continue #skips comments which contain =

            k, v = line.split("=", 1)
            v = v.strip()
            if v.startswith('"') and v.endswith('"'):
                v = v[1:-1]
            myprops[k.strip()] = v
    return myprops

"""from http://code.tutsplus.com/tutorials/understand-how-much-memory-your-python-objects-use--cms-25609"""
def deep_getsizeof(o, ids, human_readable=True):
    """Find the memory footprint of a Python object
 
    This is a recursive function that drills down a Python object graph
    like a dictionary holding nested dictionaries with lists of lists
    and tuples and sets.
 
    The sys.getsizeof function does a shallow size of only. It counts each
    object inside a container as pointer only regardless of how big it
    really is.
 
    :param o: the object
    :param ids:
    :return:
    """
    d = deep_getsizeof
    if id(o) in ids:
        return 0
 
    r = getsizeof(o)
    ids.add(id(o))
 
    if isinstance(o, str) or isinstance(0, str):
        return r
 
    if isinstance(o, Mapping):
        return r + sum(d(k, ids) + d(v, ids) for k, v in o.items())
 
    if isinstance(o, Container):
        return r + sum(d(x, ids) for x in o)
 
    return r 

"""from http://stackoverflow.com/a/1094933/283973 """
def sizeof_fmt(num):
    for unit in ['bytes', 'kB', 'MB', 'GB', 'TB', 'PB']:
        if abs(num) < 1024.0:
            return "%3.1f%s" % (num, unit)
        num /= 1024.0
    return "%.1f%s%s" % (num, 'Yi', suffix)

from math import log
def human_readable_bytes(x):
    # from http://stackoverflow.com/a/17754143/283973
    # hybrid of http://stackoverflow.com/a/10171475/2595465
    #      with http://stackoverflow.com/a/5414105/2595465
    if x == 0: return '0'
    magnitude = int(log(abs(x),10.24))
    if magnitude > 16:
        format_str = '%iP'
        denominator_mag = 15
    else:
        float_fmt = '%2.1f' if magnitude % 3 == 1 else '%1.2f'
        illion = (magnitude + 1) // 3
        format_str = float_fmt + ['', 'K', 'M', 'G', 'T', 'P'][illion]
    return (format_str % (x * 1.0 / (1024 ** illion))).lstrip('0')