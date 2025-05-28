using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeWalker.GameFiles;
using static rpf2fivem.Main;

namespace rpf2fivem.src
{
    public abstract class HelperScriptGeneratorBase
    {
        public bool IsEnabled = false;

        public abstract void Generate(string basePath);
    }

    public class AddonSpawnerConfigGenerator : HelperScriptGeneratorBase
    {
        public AddonSpawnerConfigGenerator()
        {
            IsEnabled = true;
        }

        public override void Generate(string basePath)
        {
            var finalPath = Path.Combine(basePath, "config.lua");

            foreach(var vehicle in HelperScriptRegistry.DataRegistry)
            {
                AddVehicleToGroupedLuaFile(finalPath, vehicle.Value, vehicle.Value.Model); //I feel like i implemented some of your logic wrong here.
            }
        }

        void AddVehicleToGroupedLuaFile(string filePath, VehicleData vehicle, string modelName)
        {
            // Read existing lines (or create new list if file doesn't exist)
            List<string> lines = File.Exists(filePath)
                ? File.ReadAllLines(filePath).ToList()
                : new List<string> { "config.cars = {", "}" };

            string brandKey = $"['{vehicle.Brand}']";
            string vehicleEntry = $"        {{ name = '{vehicle.Name}', model = '{modelName}' }},";

            int brandStartIndex = lines.FindIndex(l => l.TrimStart().StartsWith(brandKey));
            if (brandStartIndex != -1)
            {
                // Insert just before the closing brace of this brand
                int insertIndex = brandStartIndex + 1;
                while (insertIndex < lines.Count && !lines[insertIndex].TrimStart().StartsWith("},"))
                    insertIndex++;
                lines.Insert(insertIndex, vehicleEntry);
            }
            else
            {
                // Insert new brand section before the final closing brace
                int closingIndex = lines.FindLastIndex(l => l.Trim() == "}");
                lines.Insert(closingIndex, $"    {brandKey} = {{");
                lines.Insert(closingIndex + 1, vehicleEntry);
                lines.Insert(closingIndex + 2, "    },");
                lines.Insert(closingIndex + 3, "");
            }

            Console.WriteLine("[AddonSpawnerGenerator]:" + vehicle.Model);
            File.WriteAllLines(filePath, lines);
        }
    }


    public static class HelperScriptRegistry
    {
        public static List<HelperScriptGeneratorBase> Generators = new List<HelperScriptGeneratorBase>();

        public static Dictionary<string, VehicleData> DataRegistry = new Dictionary<string, VehicleData>();

        public static void SetVehicleBaseName(string internalRef, string baseName)
        {
            if (!DataRegistry.ContainsKey(internalRef))
            {
                Console.WriteLine($"{internalRef} : {baseName} couldnt be inserted!");
                //DataRegistry.Add(internalRef, bas)
                return;
            }

            var vehc = DataRegistry[internalRef];
            vehc.Model = baseName;
            DataRegistry[internalRef] = vehc;
        }

        public static void RegisterVehicleData(string rpf, VehicleData data)
        {
            Console.WriteLine($"Registered: {rpf} : {data.Name}");
            DataRegistry.Add(rpf, data);
        }

        public static void Generate(string exportFolder)
        {
            foreach(var generator in Generators)
            {
                if (generator.IsEnabled)
                {
                    generator.Generate(exportFolder);
                }
            }
        }
    }
}
