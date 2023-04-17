﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Winch.Config;

namespace Winch.Core
{
    class ModAssembly
    {
        public readonly string BasePath;
        public Dictionary<string, object> Metadata;
        public Assembly LoadedAssembly;

        private ModAssembly(string basePath) {
            BasePath = basePath;

            string metaPath = Path.Combine(basePath, "mod_meta.json");
            if (!File.Exists(metaPath))
                throw new FileNotFoundException("Missing mod_meta.json file.");

            string metaText = File.ReadAllText(metaPath);
            Metadata = JsonConvert.DeserializeObject<Dictionary<string, object>>(metaText);
        }

        public static ModAssembly FromPath(string path)
        {
            return new ModAssembly(path);
        }



        public void LoadAssembly()
        {
            if (!Metadata.ContainsKey("ModAssembly"))
                throw new MissingFieldException("Property 'ModAssembly' not found in mod_meta.json");

            string assemblyName = Metadata["ModAssembly"].ToString();
            string assemblyPath = Path.Combine(BasePath, assemblyName);
            if(!File.Exists(assemblyPath))
                throw new FileNotFoundException($"Could not find mod assembly '{assemblyPath}'");

            LoadedAssembly = Assembly.LoadFrom(assemblyPath);

            WinchCore.Log.Debug($"Loaded Assembly '{LoadedAssembly.GetName().Name}'.");
        }

        public void ExecuteAssembly()
        {
            if (LoadedAssembly == null)
                throw new NullReferenceException("Cannot execute assembly as LoadedAssembly is null");

            if(Metadata.ContainsKey("DefaultConfig"))
            {
                string defaultConfig = JsonConvert.SerializeObject(Metadata["DefaultConfig"], Formatting.Indented);
                string modName = Path.GetFileName(BasePath);
                ModConfig.RegisterDefaultConfig(modName, defaultConfig);
            }

            if(Metadata.ContainsKey("Entrypoint"))
            {
                string entrypointSetting = Metadata["Entrypoint"].ToString();
                if (!entrypointSetting.Contains("/"))
                    throw new ArgumentException("Malformed Entrypoint in mod_meta.json");

                string entrypointTypeName = entrypointSetting.Split('/')[0];
                string entrypointMethodName = entrypointSetting.Split('/')[1];

                Type entrypointType = LoadedAssembly.GetType(entrypointTypeName);
                if (entrypointType == null)
                    throw new EntryPointNotFoundException($"Could not find type {entrypointTypeName} in Mod Assembly");

                MethodInfo entrypoint = entrypointType.GetMethod(entrypointMethodName);
                if(entrypoint == null)
                    throw new EntryPointNotFoundException($"Could not find method {entrypointTypeName} in type {entrypointTypeName} in Mod Assembly");

                WinchCore.Log.Debug($"Invoking entrypoint {entrypointType}.{entrypointMethodName}...");
                entrypoint.Invoke(null, new object[0]);
            }
        }
    }
}
