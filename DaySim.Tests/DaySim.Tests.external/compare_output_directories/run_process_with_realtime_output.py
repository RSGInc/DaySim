"""from http://sharats.me/the-ever-useful-and-neat-subprocess-module.html#watching-both-stdout-and-stderr"""
from subprocess import Popen, PIPE
from threading import Thread
from queue import Queue, Empty
import sys

if sys.version_info < (3,0):
    print("Sorry, requires Python 3.x, not Python 2.x")
    sys.exit(1)

def run_process_with_realtime_output(cmd):
    io_q = Queue()

    def stream_watcher(identifier, stream):
        for line in stream:
            tuple_to_store = (identifier, line)
            io_q.put(tuple_to_store)

        if not stream.closed:
            stream.close()

    print("In run_process_with_realtime_output. cmd='", cmd, "'" )

    proc = Popen(cmd, stdout=PIPE, stderr=PIPE)

    Thread(target=stream_watcher, name='stdout-watcher',
            args=('STDOUT', proc.stdout)).start()
    Thread(target=stream_watcher, name='stderr-watcher',
            args=('STDERR', proc.stderr)).start()

    def printer():
        while True:
            try:
                # Block for 1 second.
                identifier, line = io_q.get(True, 1)
            except Empty:
                # No output in either streams for a second. Are we done?
                if proc.poll() is not None:
                    break
            else:
                 print(identifier + ':', line.decode("utf-8").rstrip())

    print_thread = Thread(target=printer, name='printer')
    print_thread.start()
    print_thread.join()
    return proc.returncode