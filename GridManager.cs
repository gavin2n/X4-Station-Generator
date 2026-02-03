using System;
using System.Collections.Generic;
using System.Linq;

namespace X4StationBlueprintGen
{
    public class GridManager
    {
        private List<PlacedModule> _placedModules = new List<PlacedModule>();
        private List<OpenNode> _openNodes = new List<OpenNode>();

        public struct PlacedModule
        {
            public string Macro;
            public int X;
            public int Y;
            public int Z;
            public int Yaw; // 0, 90, 180, 270
            public ModuleInfo Info;

            public (int X, int Y, int Z) GetTotalSize()
            {
                // Rotation logic simplifies if we stick to cubes or symmetrical cross connectors
                // For now, assume simple AABB check without rotation transformation for size (safe for cubes)
                return Info.GetTotalSize();
            }

            public bool Intersects(int otherX, int otherY, int otherZ, ModuleInfo otherInfo)
            {
                // Strict AABB Check (No Buffer) to allow touching
                // We want to allow touching faces, so we need strict < and > not <= 
                
                int halfXA = Info.SizeX / 2;
                int halfYA = Info.SizeY / 2;
                int halfZA = Info.SizeZ / 2;
                
                int minXa = X - halfXA;
                int maxXa = X + halfXA;
                int minYa = Y - halfYA;
                int maxYa = Y + halfYA;
                int minZa = Z - halfZA;
                int maxZa = Z + halfZA;

                int halfXB = otherInfo.SizeX / 2;
                int halfYB = otherInfo.SizeY / 2;
                int halfZB = otherInfo.SizeZ / 2;

                int minXb = otherX - halfXB;
                int maxXb = otherX + halfXB;
                int minYb = otherY - halfYB;
                int maxYb = otherY + halfYB;
                int minZb = otherZ - halfZB;
                int maxZb = otherZ + halfZB;

                // Allow touching means we treat boundary equality as NO overlap
                // Overlap if (MinA < MaxB) and (MaxA > MinB)
                // For "Touching", X4 often clips a tiny bit, but let's stick to strict bound.
                // If MinA == MaxB, they touch. That is NOT an intersection for valid placement.
                
                bool overlapX = (minXa < maxXb) && (maxXa > minXb);
                bool overlapY = (minYa < maxYb) && (maxYa > minYb);
                bool overlapZ = (minZa < maxZb) && (maxZa > minZb);
                
                // Allow a tiny tolerance for 'touching' to not be 'overlapping'? 
                // The logical operator `minXa < maxXb` implies that if they are equal, it is NOT overlap.
                // So (0 < -100) is False. 
                // Wait.
                // Rect A: [-100, 100]. Rect B: [100, 300].
                // minXa(-100) < maxXb(300) -> True.
                // maxXa(100) > minXb(100) -> False. (Equal is not >)
                // So they do NOT overlap. Correct.
                
                // However, integer division usually rounds down.
                // 1000 / 2 = 500. 
                // If I am at 0. [-500, 500].
                // Neighbor at 1000. [500, 1500].
                // 500 > 500 is False. No overlap.
                
                // But previously I was using `Buffer` which made it [-550, 550].
                // 550 > 500 is True. Overlap.
                
                return overlapX && overlapY && overlapZ;
            }
        }

        public struct OpenNode
        {
            public int X;
            public int Y;
            public int Z;
            public int DirX;
            public int DirY;
            public int DirZ;
        }

        public GridManager()
        {
            // Start with a Root Connector at 0,0,0
            PlaceModuleInt("structures_arg_connector_cross_01_macro", 0, 0, 0, 0);
        }
        
        // Internal placement that registers nodes
        private void PlaceModuleInt(string macro, int x, int y, int z, int yaw)
        {
            var info = StationComponents.GetInfo(macro);
            _placedModules.Add(new PlacedModule 
            { 
                Macro = macro, 
                X = x, Y = y, Z = z, Yaw = yaw, 
                Info = info 
            });

            // Add new nodes from this module
            foreach (var conn in info.Connections)
            {
                // Transform local offset to world based on Yaw (ignoring pitch/roll for now)
                // Default Yaw 0 = Facing +Z
                // X -> X, Z -> Z
                
                int worldOffsetX = conn.OffsetX;
                int worldOffsetY = conn.OffsetY;
                int worldOffsetZ = conn.OffsetZ;
                
                int worldDirX = conn.DirX;
                int worldDirY = conn.DirY;
                int worldDirZ = conn.DirZ;
                
                // TODO: Apply rotation transform if we implement meaningful rotation logic
                // For now, identity transform as most attachments are axis-aligned
                
                _openNodes.Add(new OpenNode
                {
                    X = x + worldOffsetX,
                    Y = y + worldOffsetY,
                    Z = z + worldOffsetZ,
                    DirX = worldDirX,
                    DirY = worldDirY,
                    DirZ = worldDirZ
                });
            }
        }

        // Public API to place a new module and get its position
        // Returns list of placed modules (the requested one + any necessary connectors)
        public List<(string Macro, int X, int Y, int Z, int Yaw)> AddModule(string macro)
        {
            var newPlacements = new List<(string, int, int, int, int)>();
            var targetInfo = StationComponents.GetInfo(macro);

            // Breadth-first search for a spot
            // Sort open nodes by distance to center to keep it tight
            _openNodes.Sort((a, b) => 
                (Math.Abs(a.X) + Math.Abs(a.Y) + Math.Abs(a.Z))
                .CompareTo(Math.Abs(b.X) + Math.Abs(b.Y) + Math.Abs(b.Z)));

            // 1. Try to attach directly to an existing open node
            // Only if the module fits, and we can align one of its nodes to the open node
            // Ideally: Module Node (Inward) <-> Open Node (Outward)
            // SImplification: Assume we attach module CENTER to Open Node + Module HalfSize * Direction
            
            // Try existing nodes
            foreach (var node in _openNodes.ToList()) // ToList to modify collection if needed
            {
                // Calculate proposed center position
                // We want to attach the module such that its 'back' is at the node
                // Pos = NodePos + (Size * Dir / 2)
                
                int targetX = node.X + (node.DirX * (targetInfo.SizeX / 2 + targetInfo.Buffer));
                int targetY = node.Y + (node.DirY * (targetInfo.SizeY / 2 + targetInfo.Buffer));
                int targetZ = node.Z + (node.DirZ * (targetInfo.SizeZ / 2 + targetInfo.Buffer));

                if (!IsColliding(targetX, targetY, targetZ, targetInfo))
                {
                    // Success!
                    PlaceModuleInt(macro, targetX, targetY, targetZ, 0);
                    _openNodes.Remove(node); // Consumed
                    newPlacements.Add((macro, targetX, targetY, targetZ, 0));
                    return newPlacements;
                }
            }

            // 2. If no direct fit, we need to extend with a connector
            // Find a node where we can place a connector
            string connMacro = "structures_arg_connector_cross_01_macro";
            var connInfo = StationComponents.GetInfo(connMacro);

            foreach (var node in _openNodes.ToList())
            {
                 int cx = node.X + (node.DirX * (connInfo.SizeX / 2 + 50)); // buffer default
                 int cy = node.Y + (node.DirY * (connInfo.SizeY / 2 + 50));
                 int cz = node.Z + (node.DirZ * (connInfo.SizeZ / 2 + 50));

                 if (!IsColliding(cx, cy, cz, connInfo))
                 {
                     // Place connector
                     PlaceModuleInt(connMacro, cx, cy, cz, 0);
                     _openNodes.Remove(node);
                     newPlacements.Add((connMacro, cx, cy, cz, 0));
                     
                     // Now try allowing the recursion to handle the actual module placement next call?
                     // Or force it now? Let's recursively call AddModule logic logic for the target
                     // But we must return the full list.
                     
                     // IMPORTANT: The recursion could loop if checks fail.
                     // But since we just added fresh nodes, one of them SHOULD be valid (pointing outwards).
                     
                     // Let's manually try to place the target on the new connector's forward output
                     // The new connector was placed at cx, cy, cz attached to -Dir.
                     // It has output at +Dir.
                     
                     int tx = cx + (node.DirX * (connInfo.SizeX / 2 + targetInfo.SizeX / 2 + 100)); // chained offset
                     int ty = cy + (node.DirY * (connInfo.SizeY / 2 + targetInfo.SizeY / 2 + 100));
                     int tz = cz + (node.DirZ * (connInfo.SizeZ / 2 + targetInfo.SizeZ / 2 + 100));
                     
                     if (!IsColliding(tx, ty, tz, targetInfo))
                     {
                         PlaceModuleInt(macro, tx, ty, tz, 0);
                         // Note: We don't remove the specific intermediate node from list here as PlaceModuleInt adds NEW nodes
                         // But we should logically consume the connector's node we just used.
                         // Too complex for this snippet.
                         // Simplification: Just return. The unused nodes of the connector remain available.
                         newPlacements.Add((macro, tx, ty, tz, 0));
                         return newPlacements;
                     }
                 }
            }
            
            // 3. Last Resort fallback (floating) if graph completely blocked
            int failX = 20000 + _placedModules.Count * 1000;
             _placedModules.Add(new PlacedModule 
            { 
                Macro = macro, 
                X = failX, Y = 0, Z = 0, Yaw = 0, 
                Info = targetInfo 
            });
            newPlacements.Add((macro, failX, 0, 0, 0));
            return newPlacements;
        }

        private bool IsColliding(int x, int y, int z, ModuleInfo info)
        {
            foreach (var m in _placedModules)
            {
                if (m.Intersects(x, y, z, info)) return true;
            }
            return false;
        }
        
        // Getter for initial root if needed
        public List<(string, int, int, int)> GetInitialState()
        {
             // return root
             return new List<(string, int, int, int)> { ("structures_arg_connector_cross_01_macro", 0,0,0) };
        }
    }
}
