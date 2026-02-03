using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace X4StationBlueprintGen
{
    public class UrlParser
    {
        public static List<(string ModuleId, int Count)> Parse(string url)
        {
            var modules = new List<(string, int)>();

            try
            {
                // Decode URL just in case, though the format usually has raw chars
                // Example: https://x4-game.com/#/station-calculator?l=@$module-module_gen_prod_hullparts_01,count:20;,$module-module_gen_prod_energycells_01,count:4;...
                
                var uri = new Uri(url);
                var query = uri.Query;
                if (string.IsNullOrEmpty(query) && url.Contains("#"))
                {
                   // Handle hash routing if parameters are after #
                   var parts = url.Split('?');
                   if (parts.Length > 1)
                   {
                       query = "?" + parts[1];
                   }
                }

                var queryParams = HttpUtility.ParseQueryString(query);
                var lParam = queryParams["l"];

                if (string.IsNullOrEmpty(lParam))
                {
                    // Fallback manual parse if HttpUtility fails on some hash formats
                    var match = Regex.Match(url, @"[?&]l=([^&]+)");
                    if (match.Success)
                    {
                        lParam = match.Groups[1].Value;
                    }
                }

                if (string.IsNullOrEmpty(lParam))
                {
                    Console.WriteLine("Error: Could not find 'l' parameter in URL.");
                    return modules;
                }

                // Decode the parameter value
                lParam = HttpUtility.UrlDecode(lParam);

                // Format seems to be: @$module-ID,count:N;,$module-ID,count:N;...
                // Remove the leading '@' if present
                if (lParam.StartsWith("@")) lParam = lParam.Substring(1);

                // Split by ";," which seems to be the separator between modules
                // Or just split by semicolon if that's safer, then handle the comma
                var entries = lParam.Split(new[] { ";," }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var entry in entries)
                {
                    // entry looks like: $module-module_gen_prod_hullparts_01,count:20
                    // or last one might just be ...;
                    
                    var cleanEntry = entry.TrimEnd(';');
                    
                    // Regex to extract module ID and count
                    // $module-(ID),count:(N)
                    var moduleMatch = Regex.Match(cleanEntry, @"\$module-([^,]+),count:(\d+)");
                    
                    if (moduleMatch.Success)
                    {
                        string id = moduleMatch.Groups[1].Value;
                        int count = int.Parse(moduleMatch.Groups[2].Value);
                        modules.Add((id, count));
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing URL: {ex.Message}");
            }

            return modules;
        }
    }
}
