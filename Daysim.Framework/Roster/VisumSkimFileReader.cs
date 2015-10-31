using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Daysim.Framework.Roster {
	public class VisumSkimFileReader : ISkimFileReader{
		//#region ISkimFileReader Members

		private string _path;
		private Dictionary<int, int> _mapping;
		private string _file;
		//private BinaryReader _reader;

		private UInt16[][] _matrix = null;

		public VisumSkimFileReader(string path, Dictionary<int, int> mapping)
		{
			_path = path;
			_mapping = mapping;
		}

		public SkimMatrix Read(string filename, int field, float scale)
		{
			_file = Path.Combine(_path, filename);

			Console.WriteLine("Loading skim file: {0}", _file);
			
			GetVisumMatrix(_file, field, scale);
			return new SkimMatrix(_matrix);
		}

		private void GetVisumMatrix( string matPath, int matrixNumber, float scale)
		{
			const string dllPath = "C:\\Program Files\\Citilabs\\VoyagerFileAPI";

			/*SetDllDirectoryA(dllPath);

			var dllPtr = LoadLibraryA("VoyagerFileAccess.dll");

			var matReaderOpenPtr = GetProcAddress(dllPtr, "MatReaderOpen");
			var matReaderClosePtr = GetProcAddress(dllPtr, "MatReaderClose");
			var matReaderGetMatrixNamesPtr = GetProcAddress(dllPtr, "MatReaderGetMatrixNames");
			var matReaderGetNumMatsPtr = GetProcAddress(dllPtr, "MatReaderGetNumMats");
			var matReaderGetNumZonesPtr = GetProcAddress(dllPtr, "MatReaderGetNumZones");
			var matReaderGetRowPtr = GetProcAddress(dllPtr, "MatReaderGetRow");

			var matReaderOpen = (MatReaderOpen) Marshal.GetDelegateForFunctionPointer(matReaderOpenPtr, typeof (MatReaderOpen));
			var matReaderClose = (MatReaderClose) Marshal.GetDelegateForFunctionPointer(matReaderClosePtr, typeof (MatReaderClose));
			var matReaderGetMatrixNames = (MatReaderGetMatrixNames) Marshal.GetDelegateForFunctionPointer(matReaderGetMatrixNamesPtr, typeof (MatReaderGetMatrixNames));
			var matReaderGetNumMats = (MatReaderGetNumMats) Marshal.GetDelegateForFunctionPointer(matReaderGetNumMatsPtr, typeof (MatReaderGetNumMats));
			var matReaderGetNumZones = (MatReaderGetNumZones) Marshal.GetDelegateForFunctionPointer(matReaderGetNumZonesPtr, typeof (MatReaderGetNumZones));
			var matReaderGetRow = (MatReaderGetRow) Marshal.GetDelegateForFunctionPointer(matReaderGetRowPtr, typeof (MatReaderGetRow));

			var errorBuffer = Marshal.AllocHGlobal(256);
			var state = IntPtr.Zero;

			try {
				state = matReaderOpen(matPath, errorBuffer, 256);
			}
			catch (Exception ex) {
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
			}
			finally {
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
			for (int i = 0; i < count; i++)
			{
				_matrix[i] = new ushort[count];
			}

					try {
						for (var i = 1; i <= zonesLength; i++) {
							matReaderGetRow(state, matrixNumber, i, buffer);
							Marshal.Copy(buffer, zones, 0, zonesLength);

							for (var j = 1; j <= zonesLength; j++) {
								var rawValue = Convert.ToSingle(zones[j-1]);

								if (_mapping.ContainsKey(i) && _mapping.ContainsKey(j))
								{
									if (rawValue > short.MaxValue / scale) {
										rawValue = short.MaxValue / scale;
									}
									else if (rawValue < 0) {
										rawValue = 0;
									}

									var value = Convert.ToUInt16(rawValue * scale);

									_matrix[_mapping[i]][_mapping[j]] = value;
								}
							}
						}
					}
					finally {
						Marshal.FreeHGlobal(buffer);
					}
			// closes matrix file
			matReaderClose(state);*/
		}

	}
}
