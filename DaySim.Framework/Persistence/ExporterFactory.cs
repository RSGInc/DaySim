﻿// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using DaySim.Framework.DomainModels.Models;

namespace DaySim.Framework.Persistence {
  public sealed class ExporterFactory {
    private readonly ModuleBuilder _moduleBuilder;

    public ExporterFactory() {
      AppDomain appDomain = AppDomain.CurrentDomain;
      AssemblyName assemblyName = new AssemblyName("DaySim.Dynamic.Exporter");
      AssemblyBuilder assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

      _moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
    }

    public IExporter<TModel> GetExporter<TModel>(string outputPath, char delimiter) where TModel : IModel, new() {
      string className = string.Format("{0}Exporter", typeof(TModel).Name);
      Type type = _moduleBuilder.GetType(className);

      if (type == null) {
        TypeBuilder typeBuilder = _moduleBuilder.DefineType(className, TypeAttributes.Public, typeof(Exporter<TModel>));
        ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { typeof(string), typeof(char) });
        ILGenerator constructorIl = constructorBuilder.GetILGenerator();
        ConstructorInfo constructor = typeof(Exporter<TModel>).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(char) }, null);

        GenerateConstructorCode(constructorIl, constructor);

        MethodBuilder methodBuilder = typeBuilder.DefineMethod("WriteModel", MethodAttributes.Public | MethodAttributes.Virtual, typeof(void), new[] { typeof(StreamWriter), typeof(TModel), typeof(char) });
        ILGenerator methodIl = methodBuilder.GetILGenerator();
        MethodInfo method = typeof(Exporter<TModel>).GetMethod("WriteModel");

        GenerateMethodCode(methodIl, typeof(TModel), delimiter);

        typeBuilder.DefineMethodOverride(methodBuilder, method);

        type = typeBuilder.CreateType();
      }

      object instance = Activator.CreateInstance(type, new object[] { outputPath, delimiter });

      return (IExporter<TModel>)instance;
    }

    private static void GenerateConstructorCode(ILGenerator il, ConstructorInfo constructor) {
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Ldarg_1);
      il.Emit(OpCodes.Ldarg_2);
      il.Emit(OpCodes.Call, constructor);
      il.Emit(OpCodes.Ret);
    }

    private static void GenerateMethodCode(ILGenerator il, Type type, char delimiter) {
      PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);

      for (int i = 0; i < properties.Length; i++) {
        il.Emit(OpCodes.Ldarg_1);
        il.Emit(OpCodes.Ldarg_2);
        il.Emit(OpCodes.Callvirt, properties[i].GetGetMethod());

        if (properties[i].PropertyType == typeof(double)) {
          il.Emit(OpCodes.Callvirt, typeof(StreamWriter).GetMethod("Write", new[] { typeof(double) }));
        } else if (properties[i].PropertyType == typeof(int)) {
          il.Emit(OpCodes.Callvirt, typeof(StreamWriter).GetMethod("Write", new[] { typeof(int) }));
        } else if (properties[i].PropertyType == typeof(bool)) {
          il.Emit(OpCodes.Callvirt, typeof(StreamWriter).GetMethod("Write", new[] { typeof(bool) }));
        } else if (properties[i].PropertyType == typeof(long)) {
          il.Emit(OpCodes.Callvirt, typeof(StreamWriter).GetMethod("Write", new[] { typeof(long) }));
        } else {
          throw new NotSupportedException("Unsupported type. Valid types are double, int, long and bool.");
        }

        if (i == properties.Length - 1) {
          il.Emit(OpCodes.Ldarg_1);
          il.Emit(OpCodes.Callvirt, typeof(StreamWriter).GetMethod("WriteLine", Type.EmptyTypes));
        } else {
          il.Emit(OpCodes.Ldarg_1);
          il.Emit(OpCodes.Ldc_I4_S, delimiter);
          il.Emit(OpCodes.Callvirt, typeof(StreamWriter).GetMethod("Write", new[] { typeof(char) }));
        }
      }

      il.Emit(OpCodes.Ret);
    }
  }
}
