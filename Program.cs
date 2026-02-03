using System;
using System.IO;
using Spectre.Console;

namespace X4StationBlueprintGen
{
    class Program
    {
        static void Main(string[] args)
        {
            AnsiConsole.Write(
                new FigletText("X4 Blueprint Gen")
                    .Color(Color.Blue));

            string url;
            int docksCount = 0;
            int storageContainerCount = 0;
            int storageSolidCount = 0;
            int storageLiquidCount = 0;
            string culture = "Argon";
            string planName = "Imported Plan";

            // Argument parsing logic: allow fully automated call
            // Usage: <url> [docks] [storageContainer] [storageSolid] [storageLiquid] [culture] [planName]
            if (args.Length > 0)
            {
                url = args[0];
                if (args.Length > 1) int.TryParse(args[1], out docksCount);
                if (args.Length > 2) int.TryParse(args[2], out storageContainerCount);
                if (args.Length > 3) int.TryParse(args[3], out storageSolidCount);
                if (args.Length > 4) int.TryParse(args[4], out storageLiquidCount);
                if (args.Length > 5) culture = args[5];
                if (args.Length > 6) planName = args[6];
            }
            else
            {
                url = AnsiConsole.Ask<string>("Enter [green]Share Link[/]:");
                url = url.Trim('\'').Trim(); 
                
                docksCount = AnsiConsole.Ask<int>("How many [green]Standard Docks (M)[/] to add?");
                storageContainerCount = AnsiConsole.Ask<int>("How many [green]Container Storage (L)[/] to add?");
                storageSolidCount = AnsiConsole.Ask<int>("How many [green]Solid Storage (L)[/] to add?");
                storageLiquidCount = AnsiConsole.Ask<int>("How many [green]Liquid Storage (L)[/] to add?");
                
                culture = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select [green]Culture[/] for modules:")
                        .PageSize(5)
                        .AddChoices(new[] {
                            "Argon", "Paranid", "Teladi", "Split", "Terran"
                        }));

                planName = AnsiConsole.Ask<string>("Enter [green]Blueprint Name[/]:");
            }
            
            // Clean URL input 
            url = url.Trim('\'').Trim();

            AnsiConsole.Status()
                .Start("Parsing URL...", ctx => 
                {
                    AnsiConsole.MarkupLine($"Parsing: [blue]{url}[/]");
                });

            var modules = UrlParser.Parse(url);

            if (modules.Count == 0)
            {
                AnsiConsole.MarkupLine("[red]No modules found or parsing failed.[/]");
                return;
            }

            // Display table of modules
            var table = new Table();
            table.AddColumn("Module ID");
            table.AddColumn("Count");
            foreach (var m in modules)
            {
                table.AddRow(m.ModuleId, m.Count.ToString());
            }
            AnsiConsole.Write(table);

            string xml = BlueprintGenerator.Generate(modules, docksCount, storageContainerCount, storageSolidCount, storageLiquidCount, culture, planName);
            
            // Logic to find Documents folder
            string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string savePath = Path.Combine(docPath, "Egosoft", "X4");
            
            string targetFolder = null;
            if (Directory.Exists(savePath))
            {
                var dirs = Directory.GetDirectories(savePath);
                foreach (var d in dirs)
                {
                    var dirName = Path.GetFileName(d);
                    if (long.TryParse(dirName, out _))
                    {
                        // Found a numeric folder, likely Steam ID
                        targetFolder = Path.Combine(d, "constructionplan");
                        break;
                    }
                }
            }

            if (targetFolder == null)
            {
                // Fallback to local
                targetFolder = Directory.GetCurrentDirectory();
                AnsiConsole.MarkupLine("[yellow]Could not find X4 save folder. Saving locally.[/]");
            }
            else
            {
                    if (!Directory.Exists(targetFolder))
                    {
                        Directory.CreateDirectory(targetFolder);
                    }
            }

            // Sanitize plan name for filename
            string safeName = string.Join("_", planName.Split(Path.GetInvalidFileNameChars()));
            string fileName = $"{safeName}.xml";
            string fullPath = Path.Combine(targetFolder, fileName);
            
            // Duplicate check
            bool fileExists = File.Exists(fullPath);
            bool overwrite = false;

            if (fileExists)
            {
                // If interactive (no args or prompted), ask user
                if (args.Length == 0) 
                {
                    overwrite = AnsiConsole.Confirm($"File [yellow]{fileName}[/] already exists. Overwrite?");
                    if (!overwrite)
                    {
                        // Loop to get new name? Or just exit?
                        // Let's offer rename
                         while (File.Exists(fullPath) && !overwrite)
                         {
                             planName = AnsiConsole.Ask<string>("Enter a [green]New Blueprint Name[/] (or existing one to overwrite):");
                             safeName = string.Join("_", planName.Split(Path.GetInvalidFileNameChars()));
                             fileName = $"{safeName}.xml";
                             fullPath = Path.Combine(targetFolder, fileName);
                             
                             if (File.Exists(fullPath)) 
                             {
                                 overwrite = AnsiConsole.Confirm($"File [yellow]{fileName}[/] still exists. Overwrite?");
                             }
                             else
                             {
                                 // New name is free
                                 overwrite = true; // functionally true as we proceed to write
                             }
                         }
                         
                         // Regenerate XML with new internal name if changed
                         xml = BlueprintGenerator.Generate(modules, docksCount, storageContainerCount, storageSolidCount, storageLiquidCount, culture, planName);
                    }
                }
                else
                {
                    // CLI Mode: Fail safely to prevent accidental overwrite
                    AnsiConsole.MarkupLine($"[red]Error: File '{fileName}' already exists. Use interactive mode to rename or overwrite manually.[/]");
                    return; 
                }
            }

            File.WriteAllText(fullPath, xml);
            AnsiConsole.MarkupLine($"[green]Successfully saved blueprint to:[/] {fullPath}");
        }
    }
}
