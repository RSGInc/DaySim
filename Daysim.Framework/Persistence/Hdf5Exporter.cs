using System.Collections.Generic;
using System.IO;
using DaySim.Framework.Core;
using HDF5DotNet;

namespace DaySim.Framework.Persistence {
	public class Hdf5Exporter<TModel> {
		private readonly List<TModel> _buffer = new List<TModel>();

		public void Export(TModel model) {
			_buffer.Add(model);
		}


        public void Flush()
        {
            if (_buffer.Count == 0)
            {
                return;
            }

            Write(_buffer);

            _buffer.Clear();
        }


		private static void WriteHdf5Data(double[] values, string className, string p) {
			var fileId = GetFileId();
			var setName = GetNamePrefix() + className + p;
			var dim1 = GetChunkSize();
			var writeSize = values.Length;

			var dataType = new H5DataTypeId(H5T.H5Type.NATIVE_DOUBLE);
			var dataSetId = CreateDatasetIfNoneExists(fileId, setName, dim1, dataType);

			var wrapArray = new H5Array<double>(values);
			var fileSpaceId = new H5DataSpaceId(H5S.H5SType.ALL);
			var all = new H5DataSpaceId(H5S.H5SType.ALL);
			var xferProp = H5P.create(H5P.PropertyListClass.DATASET_XFER);

			var newSize = new long[] {
				writeSize
			};

			H5D.setExtent(dataSetId, newSize);

			H5D.write(dataSetId, dataType, all, fileSpaceId, xferProp, wrapArray);
			H5F.close(fileId);
		}

		private static void WriteHdf5Data(int[] values, string className, string p) {
			var fileId = GetFileId();
			var setName = GetNamePrefix() + className + p;
			var dim1 = GetChunkSize();
			var writeSize = values.Length;
			var dataType = new H5DataTypeId(H5T.H5Type.NATIVE_INT);
			var dataSetId = CreateDatasetIfNoneExists(fileId, setName, dim1, dataType);

			var wrapArray = new H5Array<int>(values);
			var fileSpaceId = new H5DataSpaceId(H5S.H5SType.ALL);
			var all = new H5DataSpaceId(H5S.H5SType.ALL);
			var xferProp = H5P.create(H5P.PropertyListClass.DATASET_XFER);

			var newSize = new long[] {
				writeSize
			};

			H5D.setExtent(dataSetId, newSize);

			H5D.write(dataSetId, dataType, all, fileSpaceId, xferProp, wrapArray);
			H5F.close(fileId);
		}

		private static H5GroupId CreateGroupIfNoneExists(H5FileId fileId, string path) {
			H5GroupId group = null;
			if (H5L.Exists(fileId, path)) {
				group = H5G.open(fileId, path);
			}
			else {
				group = H5G.create(fileId, path);
			}
			return group;
		}

		private static string GetNamePrefix() {
			return "";
		}

		private static H5FileId GetFileId() {
			return
				File.Exists(Global.GetOutputPath(Global.Configuration.HDF5Path))
					? H5F.open(Global.GetOutputPath(Global.Configuration.HDF5Path), H5F.OpenMode.ACC_RDWR)
					: H5F.create(Global.GetOutputPath(Global.Configuration.HDF5Path), H5F.CreateMode.ACC_EXCL);
		}

		private static int GetChunkSize() {
			return 500000;
		}

		private static H5DataSetId CreateDatasetIfNoneExists(H5FileId fileId, string setName, int dim1, H5DataTypeId dataType) {
			H5DataSetId dataSetId = null;

			char[] sep = {'/'};
			var strings = setName.Split(sep);
			var path = "/";
			for (var x = 0; x < strings.Length - 1; x++) {
				path += strings[x] + "/";
				var groupId = CreateGroupIfNoneExists(fileId, path);
			}

			var dims = new long[] {
				dim1
			};
			var maxDims = new long[] {
				-1
			};

			if (H5L.Exists(fileId, setName)) {
				dataSetId = H5D.open(fileId, setName);
				H5D.setExtent(dataSetId, dims);
			}
			else {
				var linkp = H5P.create(H5P.PropertyListClass.LINK_CREATE);
				var accessp = H5P.create(H5P.PropertyListClass.DATASET_ACCESS);
				var createp = H5P.create(H5P.PropertyListClass.DATASET_CREATE);
				H5P.setChunk(createp, dims);
				H5P.setDeflate(createp, 1);

				var sId = H5S.create_simple(1, dims, maxDims);

				dataSetId = H5D.create(fileId, setName, dataType, sId, linkp, createp, accessp);
			}

			return dataSetId;
		}

		private static void Write(IList<TModel> list) {
			var count = list.Count;
			var intArrays = new Dictionary<string, int[]>();
			var doubleArrays = new Dictionary<string, double[]>();
			var props = list[0].GetType().GetProperties();

			foreach (var propertyInfo in props) {
				if (propertyInfo.PropertyType == typeof (int)) {
					intArrays.Add(propertyInfo.Name, new int[count]);
				}
				else if (propertyInfo.PropertyType == typeof (double)) {
					doubleArrays.Add(propertyInfo.Name, new double[count]);
				}
			}

			var i = 0;

			foreach (var model in list) {
				foreach (var propertyInfo in props) {
					if (propertyInfo.PropertyType == typeof (int)) {
						var value = (int) propertyInfo.GetValue(model, null);

						intArrays[propertyInfo.Name][i] = value;
					}
					else if (propertyInfo.PropertyType == typeof (double)) {
						var value = (double) propertyInfo.GetValue(model, null);

						doubleArrays[propertyInfo.Name][i] = value;
					}
				}

				i++;
			}

			var className = typeof (TModel).Name + "/";

			foreach (var propertyInfo in props) {
				var attribute = ((ColumnNameAttribute) propertyInfo.GetCustomAttributes(typeof (ColumnNameAttribute), true)[0]).ColumnName;

				if (propertyInfo.PropertyType == typeof (int)) {
					WriteHdf5Data(intArrays[propertyInfo.Name], className, attribute);
				}
				else if (propertyInfo.PropertyType == typeof (double)) {
					WriteHdf5Data(doubleArrays[propertyInfo.Name], className, attribute);
				}
			}
		}

		//		public static H5DataSpaceId GetMemSpace(long dim1) {
//			var dims = new long[] {
//				dim1
//			};
//			var maxDims = new long[] {
//				-1
//			};
//			return H5S.create_simple(1, dims, maxDims);
//		}
//
//		public static H5PropertyListId GetDataSetProp(long dim1) {
//			var chunkDims = new long[] {
//				dim1
//			};
//			var prop = H5P.create(H5P.PropertyListClass.DATASET_CREATE);
//			H5P.setChunk(prop, chunkDims);
//			return prop;
//		}
	}
}