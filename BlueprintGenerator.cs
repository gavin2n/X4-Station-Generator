using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace X4StationBlueprintGen
{
    public class BlueprintGenerator
    {
        public static string Generate(List<(string ModuleId, int Count)> modules, int dockCount = 0, int storageContainerCount = 0, int storageSolidCount = 0, int storageLiquidCount = 0, string culture = "argon", string planName = "Imported Calculator Plan")
        {
            // Create XML document with declaration
            var doc = new XDocument(
                new XDeclaration("1.0", "UTF-8", null)
            );

            // Root element
            var plans = new XElement("plans");
            // Sanitize plan ID logic: replace spaces with underscores, lowercase
            string planId = planName.ToLower().Replace(" ", "_");
            
            var plan = new XElement("plan", new XAttribute("id", planId), new XAttribute("name", planName), new XAttribute("description", ""));
            plans.Add(plan);
            doc.Add(plans);

            int entryIndex = 1;
            
            // Simple grid layout
            int x = 0;
            int y = 0;
            int z = 0;
            int spacing = 5000; // 5km spacing
            int rowLimit = 10;
            int counter = 0;

            // Helper to add entry
            void AddEntry(string macroName) {
                // <entry index="1" macro="macro_name">
                var entry = new XElement("entry", new XAttribute("index", entryIndex), new XAttribute("macro", macroName));
                
                // <offset> wrapper required
                var offset = new XElement("offset");

                // <position x="0" y="0" z="0" />
                offset.Add(new XElement("position", 
                    new XAttribute("x", x), 
                    new XAttribute("y", y), 
                    new XAttribute("z", z)
                ));
                
                // <rotation yaw="0" pitch="0" roll="0" />
                // Only add if non-zero to match game files
                if (x != 0 || y != 0 || z != 0)
                {
                    offset.Add(new XElement("rotation", 
                        new XAttribute("yaw", 0), 
                        new XAttribute("pitch", 0), 
                        new XAttribute("roll", 0)
                    ));
                }

                entry.Add(offset);
                plan.Add(entry);
                entryIndex++;

                // Grid updates
                x += spacing;
                counter++;
                if (counter >= rowLimit)
                {
                    x = 0;
                    z += spacing;
                    counter = 0;
                }
            };

            // 1. Add Docks
            if (dockCount > 0)
            {
                string dockMacro = ModuleMapper.GetDockMacro(culture);
                for (int i = 0; i < dockCount; i++)
                {
                    AddEntry(dockMacro);
                }
            }

            // 2. Add Container Storage (L-size)
            if (storageContainerCount > 0)
            {
                string storageMacro = ModuleMapper.GetStorageContainerMacro(culture);
                for (int i = 0; i < storageContainerCount; i++)
                {
                    AddEntry(storageMacro);
                }
            }

            // 3. Add Solid Storage (L-size)
            if (storageSolidCount > 0)
            {
                string storageMacro = ModuleMapper.GetStorageSolidMacro(culture);
                for (int i = 0; i < storageSolidCount; i++)
                {
                    AddEntry(storageMacro);
                }
            }

            // 4. Add Liquid Storage (L-size)
            if (storageLiquidCount > 0)
            {
                string storageMacro = ModuleMapper.GetStorageLiquidMacro(culture);
                for (int i = 0; i < storageLiquidCount; i++)
                {
                    AddEntry(storageMacro);
                }
            }

            // 5. Add Calculated Modules
            foreach (var mod in modules)
            {
                string macroName = ModuleMapper.GetMacroName(mod.ModuleId);
                for (int i = 0; i < mod.Count; i++)
                {
                    AddEntry(macroName);
                }
            }
            
            // Return with XML declaration
            return doc.Declaration.ToString() + "\n" + doc.ToString();
        }
    }
}
