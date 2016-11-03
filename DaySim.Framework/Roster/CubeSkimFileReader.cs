// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.using System;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using DaySim.Framework.Exceptions;
using Microsoft.Win32;

namespace DaySim.Framework.Roster {
    public class CubeSkimFileReader : ISkimFileReader {

        //		[DllImport("VoyagerFileAccess.Dll", CharSet = CharSet.Auto, EntryPoint = "MatReaderOpen", CallingConvention = CallingConvention.Cdecl)]
        //		public static extern IntPtr Open(string filename, StringBuilder buffer, int bufferLength);

        /// Return Type: BOOL->int
        /// lpPathName: LPCSTR->CHAR*
        [DllImport("kernel32.dll", EntryPoint = "SetDllDirectoryA")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetDllDirectoryA([In] [MarshalAs(UnmanagedType.LPStr)] string lpPathName);

        /// Return Type: HMODULE->HINSTANCE->HINSTANCE__*
        /// lpLibFileName: LPCSTR->CHAR*
        [DllImport("kernel32.dll", EntryPoint = "LoadLibraryA")]
        private static extern IntPtr LoadLibraryA([In] [MarshalAs(UnmanagedType.LPStr)] string lpLibFileName);

        /// Return Type: FARPROC
        /// hModule: HMODULE->HINSTANCE->HINSTANCE__*
        /// lpProcName: LPCSTR->CHAR*
        [DllImport("kernel32.dll", EntryPoint = "GetProcAddress")]
        private static extern IntPtr GetProcAddress([In] IntPtr hModule, [In] [MarshalAs(UnmanagedType.LPStr)] string lpProcName);

        /// Return Type: void*
        /// filename: char*
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate IntPtr MatReaderOpen([In] [MarshalAs(UnmanagedType.LPStr)] string filename, IntPtr errBuf, int errBufLen);

        /// Return Type: void
        /// state: void*
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void MatReaderClose(IntPtr state);

        /// Return Type: int
        /// state: void*
        /// names: char**
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MatReaderGetMatrixNames(IntPtr state, IntPtr names);

        /// Return Type: int
        /// state: void*
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MatReaderGetNumMats(IntPtr state);

        /// Return Type: int
        /// state: void*
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MatReaderGetNumZones(IntPtr state);

        /// Return Type: int
        /// state: void*
        /// mat: int
        /// row: int
        /// buffer: double*
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int MatReaderGetRow(IntPtr state, int mat, int row, IntPtr buffer);

        private string _path;
        private Dictionary<int, int> _mapping;
        private string _file;
        //private BinaryReader _reader;

        private UInt16[][] _matrix = null;

        public CubeSkimFileReader(string path, Dictionary<int, int> mapping) {
            _path = path;
            _mapping = mapping;
        }

        public SkimMatrix Read(string filename, int field, float scale) {
            _file = Path.Combine(_path, filename);

            Console.WriteLine("Loading skim file: {0}", _file);

            GetCubeMatrix(_file, field, scale);
            return new SkimMatrix(_matrix);
        }

        private void GetCubeMatrix(string matPath, int matrixNumber, float scale) {

            var name = "VoyagerFileAPI";
            var url = "ftp://citilabsftp.com/outgoing/VoyagerFileAPIInstaller.zip";
            if (!IsInstalled(name)) {
                throw new MissingInstallationException(string.Format("{0} installation is missing. The installer package can be downloaded from {1}", name, url));
            }

            const string dllPath = "C:\\Program Files\\Citilabs\\VoyagerFileAPI";

            SetDllDirectoryA(dllPath);

            var dllPtr = LoadLibraryA("VoyagerFileAccess.dll");

            var matReaderOpenPtr = GetProcAddress(dllPtr, "MatReaderOpen");
            var matReaderClosePtr = GetProcAddress(dllPtr, "MatReaderClose");
            var matReaderGetMatrixNamesPtr = GetProcAddress(dllPtr, "MatReaderGetMatrixNames");
            var matReaderGetNumMatsPtr = GetProcAddress(dllPtr, "MatReaderGetNumMats");
            var matReaderGetNumZonesPtr = GetProcAddress(dllPtr, "MatReaderGetNumZones");
            var matReaderGetRowPtr = GetProcAddress(dllPtr, "MatReaderGetRow");

            var matReaderOpen = (MatReaderOpen)Marshal.GetDelegateForFunctionPointer(matReaderOpenPtr, typeof(MatReaderOpen));
            var matReaderClose = (MatReaderClose)Marshal.GetDelegateForFunctionPointer(matReaderClosePtr, typeof(MatReaderClose));
            var matReaderGetMatrixNames = (MatReaderGetMatrixNames)Marshal.GetDelegateForFunctionPointer(matReaderGetMatrixNamesPtr, typeof(MatReaderGetMatrixNames));
            var matReaderGetNumMats = (MatReaderGetNumMats)Marshal.GetDelegateForFunctionPointer(matReaderGetNumMatsPtr, typeof(MatReaderGetNumMats));
            var matReaderGetNumZones = (MatReaderGetNumZones)Marshal.GetDelegateForFunctionPointer(matReaderGetNumZonesPtr, typeof(MatReaderGetNumZones));
            var matReaderGetRow = (MatReaderGetRow)Marshal.GetDelegateForFunctionPointer(matReaderGetRowPtr, typeof(MatReaderGetRow));

            var errorBuffer = Marshal.AllocHGlobal(256);
            var state = IntPtr.Zero;

            try {
                state = matReaderOpen(matPath, errorBuffer, 256);
            } catch (Exception ex) {
                Console.WriteLine(ex.Message);
            }

            if (state == IntPtr.Zero) {
                Console.WriteLine(Marshal.PtrToStringAnsi(errorBuffer));
            }

            Marshal.FreeHGlobal(errorBuffer);

            // gets matrix names
            var coresLength = matReaderGetNumMats(state);
            var namesPtr = new IntPtr[coresLength];
            var names = new List<string>();
            var buffer = Marshal.AllocHGlobal(Marshal.SizeOf(namesPtr[0]) * coresLength);

            try {
                matReaderGetMatrixNames(state, buffer);
                Marshal.Copy(buffer, namesPtr, 0, namesPtr.Length);

                names.AddRange(namesPtr.Select(item => Marshal.PtrToStringAnsi(item)));
            } finally {
                Marshal.FreeHGlobal(buffer);
            }

            // outputs zones
            //var path = Path.Combine(binPath, names[matrixNumber] + ".bin");
            //var file = new FileInfo(path);

            var zonesLength = matReaderGetNumZones(state);
            var zones = new double[zonesLength];
            var size = Marshal.SizeOf(zones[0]) * zonesLength;

            buffer = Marshal.AllocHGlobal(size);

            int count = _mapping.Count;

            _matrix = new ushort[count][];
            for (int i = 0; i < count; i++) {
                _matrix[i] = new ushort[count];
            }

            try {
                for (var i = 1; i <= zonesLength; i++) {
                    matReaderGetRow(state, matrixNumber, i, buffer);
                    Marshal.Copy(buffer, zones, 0, zonesLength);

                    for (var j = 1; j <= zonesLength; j++) {
                        var rawValue = Convert.ToSingle(zones[j - 1]) * scale;

                        if (_mapping.ContainsKey(i) && _mapping.ContainsKey(j)) {
                            if (rawValue > ushort.MaxValue - 1) {
                                rawValue = ushort.MaxValue - 1;
                            } else if (rawValue < 0) {
                                rawValue = 0;
                            }

                            var value = Convert.ToUInt16(rawValue);

                            _matrix[_mapping[i]][_mapping[j]] = value;
                        }
                    }
                }
            } finally {
                Marshal.FreeHGlobal(buffer);
            }
            // closes matrix file
            matReaderClose(state);
        }

        public static bool IsInstalled(string name) {
            RegistryKey[] keys = {
                Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"), //current user
				Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"), //local machine
				Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\Windows\CurrentVersion\Uninstall") //inside local machine
			};

            foreach (var key in keys) {
                foreach (String keyName in key.GetSubKeyNames()) {
                    RegistryKey subkey = key.OpenSubKey(keyName);
                    string displayName = subkey.GetValue("DisplayName") as string;
                    if (name.Equals(displayName, StringComparison.OrdinalIgnoreCase) == true) {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
