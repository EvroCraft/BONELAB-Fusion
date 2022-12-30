﻿using System.Collections.Generic;
using System.IO;
using System;
using System.Reflection;
using System.Linq;

using MelonLoader;

using LabFusion.Data;
using LabFusion.Utilities;
using LabFusion.Representation;

namespace LabFusion.SDK.Modules {
    public static class ModuleHandler {
        internal static readonly List<Module> _loadedModules = new List<Module>();

        internal static void Internal_HookAssemblies() {
            AppDomain.CurrentDomain.AssemblyLoad += Internal_AssemblyLoad;
        }

        internal static void Internal_UnhookAssemblies() {
            AppDomain.CurrentDomain.AssemblyLoad -= Internal_AssemblyLoad;
        }

        private static void Internal_AssemblyLoad(object sender, AssemblyLoadEventArgs args) {
            LoadModule(args.LoadedAssembly);
        }

        /// <summary>
        /// Searches for a module in an assembly and attempts to load it.
        /// </summary>
        /// <param name="moduleAssembly"></param>
        public static void LoadModule(Assembly moduleAssembly) {
            if (moduleAssembly != null) {
                var moduleInfo = moduleAssembly.GetCustomAttribute<ModuleInfo>();

                if (moduleInfo != null && moduleInfo.moduleType != null) {
                    Internal_SetupModule(moduleInfo);
                }
            }
        }

        private static void Internal_SetupModule(ModuleInfo info) {
            if (Activator.CreateInstance(info.moduleType) is Module module) {
                _loadedModules.Add(module);
                module.ModuleLoaded(info);

                Internal_PrintDescription(info);
            }
        }

        internal static void Internal_PrintDescription(ModuleInfo info) {
            FusionLogger.Log("--==== Loaded Fusion Module ====--", ConsoleColor.Magenta);

            FusionLogger.Log($"{info.name} - v{info.version}");

            if (!string.IsNullOrWhiteSpace(info.abbreviation))
                FusionLogger.Log($"aka [{info.abbreviation}]");

            FusionLogger.Log($"by {info.author}");

            FusionLogger.Log("--=============================--", ConsoleColor.Magenta);
        }
    }
}
