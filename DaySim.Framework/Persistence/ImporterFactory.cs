﻿// Copyright 2005-2008 Mark A. Bradley and John L. Bowman
// Copyright 2011-2013 John Bowman, Mark Bradley, and RSG, Inc.
// You may not possess or use this file without a License for its use.
// Unless required by applicable law or agreed to in writing, software
// distributed under a License for its use is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using DaySim.Framework.Core;
using DaySim.Framework.DomainModels.Models;

namespace DaySim.Framework.Persistence {
  public sealed class ImporterFactory {
    private readonly ModuleBuilder _moduleBuilder;

    public ImporterFactory() {
      AppDomain appDomain = AppDomain.CurrentDomain;
      AssemblyName assemblyName = new AssemblyName("DaySim.Dynamic.Importer");
      AssemblyBuilder assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

      _moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
    }

    public IImporter GetImporter<TModel>(string inputPath, char delimiter) where TModel : IModel, new() {
      string className = string.Format("{0}Importer", typeof(TModel).Name);
      Type type = _moduleBuilder.GetType(className);

      if (type == null) {
        Dictionary<string, int> index = GetIndex(delimiter, inputPath);
        TypeBuilder typeBuilder = _moduleBuilder.DefineType(className, TypeAttributes.Public, typeof(Importer<TModel>));
        ConstructorBuilder constructorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, new[] { typeof(string), typeof(char) });
        ILGenerator constructorIl = constructorBuilder.GetILGenerator();
        ConstructorInfo constructor = typeof(Importer<TModel>).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(char) }, null);

        GenerateConstructorCode(constructorIl, constructor);

        MethodBuilder methodBuilder = typeBuilder.DefineMethod("SetModel", MethodAttributes.Public | MethodAttributes.Virtual, typeof(void), new[] { typeof(TModel), typeof(string[]) });
        ILGenerator methodIl = methodBuilder.GetILGenerator();
        MethodInfo method = typeof(Importer<TModel>).GetMethod("SetModel");

        GenerateMethodCode(methodIl, typeof(TModel), index, inputPath);

        typeBuilder.DefineMethodOverride(methodBuilder, method);

        type = typeBuilder.CreateType();
      }

      object instance = Activator.CreateInstance(type, new object[] { inputPath, delimiter });

      return (IImporter)instance;
    }

    private static Dictionary<string, int> GetIndex(char delimiter, string inputPath) {
      FileInfo file = new FileInfo(inputPath);

      string header;

      using (CountingReader reader = new CountingReader(file.Open(FileMode.Open, FileAccess.Read, FileShare.Read))) {
        header = reader.ReadLine();
      }

      return header == null ? new Dictionary<string, int>() : header.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries).Select((t, i) => new { Name = t, Index = i }).ToDictionary(t => t.Name, t => t.Index);
    }

    private static void GenerateConstructorCode(ILGenerator il, ConstructorInfo constructor) {
      il.Emit(OpCodes.Ldarg_0);
      il.Emit(OpCodes.Ldarg_1);
      il.Emit(OpCodes.Ldarg_2);
      il.Emit(OpCodes.Call, constructor);
      il.Emit(OpCodes.Ret);
    }

    private static void GenerateMethodCode(ILGenerator il, Type type, Dictionary<string, int> index, string inputPath) {
      PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

      foreach (PropertyInfo property in properties) {
        ColumnNameAttribute attribute = property.GetCustomAttributes(typeof(ColumnNameAttribute), true).Cast<ColumnNameAttribute>().SingleOrDefault();

        if (attribute == null) {
          continue;
        }

        int element;

        try {
          element = index[attribute.ColumnName];
        } catch (KeyNotFoundException keyNotFoundException) {
          string message = string.Format("The column '{0}' for object of type {1} was not found in filename: {2}", attribute.ColumnName, type, inputPath);
          Console.WriteLine(message);
          if (Global.PrintFile != null) {
            Global.PrintFile.WriteLine(message);
          }
          throw new KeyNotFoundException(message, keyNotFoundException);
        }

        if (property.PropertyType == typeof(double)) {
          il.Emit(OpCodes.Ldarg_1);
          il.Emit(OpCodes.Ldarg_2);

          SetLdc(il, element);

          il.Emit(OpCodes.Ldelem_Ref);
          il.Emit(OpCodes.Call, typeof(double).GetMethod("Parse", new[] { typeof(string) }));
          il.Emit(OpCodes.Call, property.GetSetMethod());
        } else if (property.PropertyType == typeof(int)) {
          il.Emit(OpCodes.Ldarg_1);
          il.Emit(OpCodes.Ldarg_2);

          SetLdc(il, element);

          il.Emit(OpCodes.Ldelem_Ref);
          il.Emit(OpCodes.Call, typeof(int).GetMethod("Parse", new[] { typeof(string) }));
          il.Emit(OpCodes.Call, property.GetSetMethod());
        } else if (property.PropertyType == typeof(bool)) {
          il.Emit(OpCodes.Ldarg_1);
          il.Emit(OpCodes.Ldarg_2);

          SetLdc(il, element);

          il.Emit(OpCodes.Ldelem_Ref);
          il.Emit(OpCodes.Call, typeof(int).GetMethod("Parse", new[] { typeof(string) }));
          il.Emit(OpCodes.Ldc_I4_1);
          il.Emit(OpCodes.Ceq);
          il.Emit(OpCodes.Call, property.GetSetMethod());
        } else {
          throw new NotSupportedException("Unsupported type. Valid types are double, int, and bool.");
        }
      }

      il.Emit(OpCodes.Ret);
    }

    private static void SetLdc(ILGenerator il, int element) {
      switch (element) {
        case 0:
          il.Emit(OpCodes.Ldc_I4_0);

          return;
        case 1:
          il.Emit(OpCodes.Ldc_I4_1);

          return;
        case 2:
          il.Emit(OpCodes.Ldc_I4_2);

          return;
        case 3:
          il.Emit(OpCodes.Ldc_I4_3);

          return;
        case 4:
          il.Emit(OpCodes.Ldc_I4_4);

          return;
        case 5:
          il.Emit(OpCodes.Ldc_I4_5);

          return;
        case 6:
          il.Emit(OpCodes.Ldc_I4_6);

          return;
        case 7:
          il.Emit(OpCodes.Ldc_I4_7);

          return;
        case 8:
          il.Emit(OpCodes.Ldc_I4_8);

          return;
        default:
          il.Emit(OpCodes.Ldc_I4_S, element);

          return;
      }
    }
  }
}
