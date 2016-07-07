// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using System.Threading;

namespace DaySim.Framework.Persistence {
	public sealed class ThreadQueue {
		private readonly Queue<ISavable> _queue = new Queue<ISavable>();
		private Thread _thread;
		private bool _shutdown;

		public ThreadQueue() {
			Start();
		}

		public bool IsRunning {
			get { return _thread != null; }
		}

		public void Shutdown() {
			if (_thread == null) {
				throw new Exception("ThreadQueue already shutdown.");
			}

			lock (_queue) {
				_shutdown = true;

				Monitor.Pulse(_queue);
			}

			if (_thread != null) {
				_thread.Join();
			}
		}

		public void Start() {
			if (_thread != null) {
				throw new Exception("ThreadQueue already running.");
			}

			_thread = new Thread(BeginSave);
			_thread.Start();
		}

		public void Stop() {
			if (_thread == null) {
				throw (new Exception("ThreadQueue already stopped."));
			}

			_thread.Abort();
			_thread = null;
		}

		private void BeginSave() {
			Save();

			_thread = null;
		}

		public void Add(ISavable obj) {
			lock (_queue) {
				_queue.Enqueue(obj);

				Monitor.Pulse(_queue);
			}
		}

		private void Save() {
			while (true) {
				lock (_queue) {
					if (_queue.Count == 0 && !_shutdown) {
						Monitor.Wait(_queue);
					}

					if (_queue.Count > 0) {
						var item = _queue.Dequeue();

						item.Save();
					}
					else if (_shutdown) {
						break;
					}
				}
			}
		}
	}
}