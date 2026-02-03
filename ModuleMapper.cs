using System;

namespace X4StationBlueprintGen
{
    public class ModuleMapper
    {
        public static string GetMacroName(string calculatorModuleId)
        {
            // Heuristic #1: Often the ID is "module_X", and macro is "X_macro"
            // Example: module_gen_prod_hullparts_01 -> prod_gen_hullparts_macro
            // IMPORTANT: Game macros do NOT include the "_01" suffix!
            
            string baseId = calculatorModuleId;
            
            if (baseId.StartsWith("module_"))
            {
                baseId = baseId.Substring(7); // remove "module_"
            }
            
            // SPECIAL CASE: "gen_prod" vs "prod_gen".
            if (baseId.StartsWith("gen_prod_"))
            {
                baseId = "prod_gen_" + baseId.Substring(9);
            }
            
            // CRITICAL: Remove "_01", "_02", "_03" suffixes that exist in calculator but not in game
            // Working examples use "prod_gen_hullparts_macro" not "prod_gen_hullparts_01_macro"
            if (baseId.EndsWith("_01"))
            {
                baseId = baseId.Substring(0, baseId.Length - 3);
            }
            else if (baseId.EndsWith("_02"))
            {
                baseId = baseId.Substring(0, baseId.Length - 3);
            }
            else if (baseId.EndsWith("_03"))
            {
                baseId = baseId.Substring(0, baseId.Length - 3);
            }
            
            return baseId + "_macro";
        }

        public static string GetDockMacro(string culture)
        {
            // Based on working examples: dockarea_arg_m_02_tradestation_01_macro
            // Using M-size tradestation docks (T dock not E dock)
            
            return culture.ToLower() switch
            {
                "argon" => "dockarea_arg_m_02_tradestation_01_macro",
                "paranid" => "dockarea_par_m_02_tradestation_01_macro",
                "teladi" => "dockarea_tel_m_02_tradestation_01_macro",
                "split" => "dockarea_spl_m_02_tradestation_01_macro",
                "terran" => "dockarea_ter_m_02_tradestation_01_macro",
                _ => "dockarea_arg_m_02_tradestation_01_macro" // Default to Argon
            };
        }

        public static string GetPierMacro(string culture)
        {
            // Based on working examples: pier_arg_harbor_03_macro
            // 03 variant = 3-dock pier for L/XL ships
            
            return culture.ToLower() switch
            {
                "argon" => "pier_arg_harbor_03_macro",
                "paranid" => "pier_par_harbor_03_macro",
                "teladi" => "pier_tel_harbor_03_macro",
                "split" => "pier_spl_harbor_03_macro",
                "terran" => "pier_ter_harbor_03_macro",
                _ => "pier_arg_harbor_03_macro"
            };
        }

        public static string GetStorageContainerMacro(string culture)
        {
            // Based on working examples: storage_arg_l_container_01_macro
            // L-size container storage (stores container goods)
            
            return culture.ToLower() switch
            {
                "argon" => "storage_arg_l_container_01_macro",
                "paranid" => "storage_par_l_container_01_macro",
                "teladi" => "storage_tel_l_container_01_macro",
                "split" => "storage_spl_l_container_01_macro",
                "terran" => "storage_ter_l_container_01_macro",
                _ => "storage_arg_l_container_01_macro"
            };
        }

        public static string GetStorageSolidMacro(string culture)
        {
            // Based on working examples: storage_arg_l_solid_01_macro
            // L-size solid storage (stores solid goods like ore, silicon, etc.)
            
            return culture.ToLower() switch
            {
                "argon" => "storage_arg_l_solid_01_macro",
                "paranid" => "storage_par_l_solid_01_macro",
                "teladi" => "storage_tel_l_solid_01_macro",
                "split" => "storage_spl_l_solid_01_macro",
                "terran" => "storage_ter_l_solid_01_macro",
                _ => "storage_arg_l_solid_01_macro"
            };
        }

        public static string GetStorageLiquidMacro(string culture)
        {
            // Based on working examples: storage_arg_l_liquid_01_macro
            // L-size liquid storage (stores liquid/gas goods)
            
            return culture.ToLower() switch
            {
                "argon" => "storage_arg_l_liquid_01_macro",
                "paranid" => "storage_par_l_liquid_01_macro",
                "teladi" => "storage_tel_l_liquid_01_macro",
                "split" => "storage_spl_l_liquid_01_macro",
                "terran" => "storage_ter_l_liquid_01_macro",
                _ => "storage_arg_l_liquid_01_macro"
            };
        }
    }
}
