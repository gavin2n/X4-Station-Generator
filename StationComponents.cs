using System.Collections.Generic;

namespace X4StationBlueprintGen
{
    public struct ConnectionNode
    {
        public int OffsetX;
        public int OffsetY;
        public int OffsetZ;
        
        // Direction vector (1,0,0 means pointing +X)
        public int DirX;
        public int DirY;
        public int DirZ;

        public ConnectionNode(int x, int y, int z, int dx, int dy, int dz)
        {
            OffsetX = x; OffsetY = y; OffsetZ = z;
            DirX = dx; DirY = dy; DirZ = dz;
        }
    }

    public struct ModuleInfo
    {
        // Dimensions in meters (approximate bounding box)
        public int SizeX;
        public int SizeY;
        public int SizeZ;

        // Ideal spacing buffer around the module
        public int Buffer;
        
        public List<ConnectionNode> Connections;

        // Constructor
        public ModuleInfo(int x, int y, int z, int buffer = 50)
        {
            SizeX = x;
            SizeY = y;
            SizeZ = z;
            Buffer = buffer;
            Connections = new List<ConnectionNode>();
            
            // Default connection point: Center bottom? Or just center for generic logic?
            // For this graph logic, let's assume we connect to the "Bottom" (0, -Y/2, 0) pointing Down (0, -1, 0)
            // AND the "Top" (0, Y/2, 0) pointing Up for vertical stacking?
            // Let's add 6 cardinal connection points for generic cubes to allow flexibility
            
            int halfX = x / 2;
            int halfY = y / 2;
            int halfZ = z / 2;

            Connections.Add(new ConnectionNode(halfX, 0, 0, 1, 0, 0));  // +X
            Connections.Add(new ConnectionNode(-halfX, 0, 0, -1, 0, 0)); // -X
            Connections.Add(new ConnectionNode(0, halfY, 0, 0, 1, 0));  // +Y
            Connections.Add(new ConnectionNode(0, -halfY, 0, 0, -1, 0)); // -Y
            Connections.Add(new ConnectionNode(0, 0, halfZ, 0, 0, 1));  // +Z
            Connections.Add(new ConnectionNode(0, 0, -halfZ, 0, 0, -1)); // -Z
        }

        // Helper to get total occupied space
        public (int X, int Y, int Z) GetTotalSize()
        {
            return (SizeX + Buffer * 2, SizeY + Buffer * 2, SizeZ + Buffer * 2);
        }
    }

    public static class StationComponents
    {
        // Dictionary to hold module stats
        public static Dictionary<string, ModuleInfo> Library { get; private set; } = new Dictionary<string, ModuleInfo>();

        static StationComponents()
        {
            InitializeLibrary();
        }

        private static void InitializeLibrary()
        {
            // --- CONNECTORS ---
            // Cross Connector 01: Assumed 200m cube roughly?
            // "structures_arg_connector_cross_01_macro"
            // Has connections on all 6 sides.
            Add("structures_arg_connector_cross_01_macro", 200, 200, 200);

            // --- DOCKS ---
            // 1M6S Standard Dock
            Add("dockarea_arg_m_02_tradestation_01_macro", 400, 200, 400); 
            // 3-Pier L/XL
            Add("pier_arg_harbor_03_macro", 1200, 400, 2000); 

            // --- STORAGE (L) ---
            Add("storage_arg_l_container_01_macro", 800, 800, 800);
            Add("storage_arg_l_solid_01_macro", 800, 800, 800);
            Add("storage_arg_l_liquid_01_macro", 800, 800, 800);

            // --- PRODUCTION (Generic Sizes) ---
            Add("prod_gen_hullparts_macro", 1000, 400, 1000);
            Add("prod_gen_energycells_macro", 1200, 200, 1200);
            Add("prod_gen_refinedmetals_macro", 600, 800, 600);
            Add("prod_gen_graphene_macro", 600, 600, 600);
            Add("prod_gen_siliconwafers_macro", 600, 600, 800);
            Add("prod_gen_superfluidcoolant_macro", 600, 800, 600);
            Add("prod_gen_microchips_macro", 800, 400, 800);
            Add("prod_gen_smartchips_macro", 600, 400, 600);
            Add("prod_gen_advancedelectronics_macro", 900, 500, 900);
            Add("prod_gen_antimattercells_macro", 700, 700, 700);
            Add("prod_gen_engineparts_macro", 900, 600, 900);
            Add("prod_gen_shieldcomponents_macro", 900, 500, 900);
            Add("prod_gen_turretcomponents_macro", 800, 500, 800);
            Add("prod_gen_weaponcomponents_macro", 800, 500, 800);
            Add("prod_gen_fieldcoils_macro", 700, 500, 700);
            Add("prod_gen_scanningarrays_macro", 700, 800, 700);
            Add("prod_gen_quantumtubes_macro", 600, 800, 600);
            Add("prod_gen_plasmaconductors_macro", 700, 700, 700);
            Add("prod_gen_claytronics_macro", 1000, 600, 1000);
            Add("prod_arg_meat_macro", 800, 600, 800);
            Add("prod_arg_wheat_macro", 1000, 200, 1000); 
            Add("prod_arg_foodrations_macro", 800, 600, 800);
            Add("prod_arg_medicalsupplies_macro", 800, 800, 800);
        }

        private static void Add(string macro, int x, int y, int z)
        {
            if (!Library.ContainsKey(macro))
            {
                Library.Add(macro, new ModuleInfo(x, y, z));
            }
        }

        public static ModuleInfo GetInfo(string macroName)
        {
            if (Library.TryGetValue(macroName, out var info))
            {
                return info;
            }
            
            // Fallback for unknown modules
            return new ModuleInfo(500, 500, 500); 
        }
    }
}
