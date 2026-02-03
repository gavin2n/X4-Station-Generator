# X4 Station Blueprint Generator

A command-line tool that converts [X4 Station Calculator](https://x4-game.com/#/station-calculator) designs into valid X4: Foundations blueprint XML files. Quickly import your carefully planned station designs directly into the game!

## Features

✨ **Direct Import** - Parse station calculator URLs and generate game-ready blueprints  
🏗️ **Module Support** - Correctly maps all production modules with proper macro names  
🚢 **Docking** - Adds M-size tradestation docks automatically  
📦 **Storage** - Supports all three storage types (Container, Solid, Liquid)  
🎨 **Multi-Race** - Supports Argon, Paranid, Teladi, Split, and Terran modules  
💾 **Auto-Save** - Direct save to X4's constructionplan folder  
🔄 **Duplicate Detection** - Prevents accidental overwrites with rename options

## Requirements

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) or later
- X4: Foundations game (for using the generated blueprints)

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/gavin2n/X4-Station-Generator.git
   cd X4-Station-Generator/X4StationBlueprintGen
   ```

2. Build the project:
   ```bash
   dotnet build
   ```

## Usage

### Interactive Mode (Recommended)

Simply run the tool and follow the prompts:

```bash
dotnet run
```

You'll be prompted for:
1. **Station Calculator URL** - Your share link from x4-game.com
2. **Number of Docks** - How many M-size tradestation docks to add
3. **Storage Modules** - Separate counts for Container, Solid, and Liquid storage
4. **Culture** - Choose between Argon, Paranid, Teladi, Split, or Terran
5. **Blueprint Name** - What to name your blueprint file

### CLI Mode (For Automation)

```bash
dotnet run <url> [docks] [containerStorage] [solidStorage] [liquidStorage] [culture] [planName]
```

**Example:**
```bash
dotnet run "https://x4-game.com/#/station-calculator?l=@$module-module_gen_prod_hullparts_01,count:10" 2 5 3 2 Argon "Hull Parts Factory"
```

This creates a blueprint with:
- 10 Hull Parts production modules
- 2 M-size docks
- 5 Container storage (L)
- 3 Solid storage (L)
- 2 Liquid storage (L)
- Argon-style modules
- Named "Hull Parts Factory"

## Where Are Blueprints Saved?

Blueprints are automatically saved to your X4 constructionplan folder:

```
Documents\Egosoft\X4\{userid}\constructionplan\
```

After generation, the blueprint will be immediately available in-game via the station construction menu.

## How It Works

1. **URL Parsing** - Extracts module data from the X4 Station Calculator share URL
2. **Macro Mapping** - Converts calculator IDs to proper X4 game macro names
   - Strips suffixes like `_01`, `_02` from module IDs
   - Maps race-specific module variants
3. **XML Generation** - Creates valid X4 blueprint XML with:
   - Docks placed first
   - Storage modules (Container → Solid → Liquid)
   - Production modules from the calculator
4. **Auto-Grid Layout** - Positions modules in a grid pattern for easy station building

## Supported Modules

✅ **All Production Modules** - Hull Parts, Energy Cells, Refined Metals, etc.  
✅ **M-Size Tradestation Docks** - Standard trading docks  
✅ **L-Size Storage** - Container, Solid, and Liquid variants  
❌ **Piers** - Not included (add manually in-game for more control)

## Known Limitations

- **Piers are not generated** - Different pier types (2-dock, 3-dock, etc.) exist. Add these manually in-game for better control.
- **Module positioning** - Uses a simple grid layout. Refine the station design in-game.
- **Habitation modules** - Not yet supported by the calculator parser.

## Troubleshooting

**"Modules not appearing in game"**
- Ensure you're using the latest version of the tool
- Check that the blueprint file was created in the correct folder
- Verify the calculator URL is valid and properly formatted

**"Duplicate file name"**
- The tool will prompt to rename or overwrite in interactive mode
- In CLI mode, it will fail to prevent accidental overwrites

## Contributing

Contributions are welcome! Feel free to:
- Report bugs via GitHub Issues
- Submit pull requests for new features
- Suggest improvements to module mapping

## License

This project is open source and available under the MIT License.

## Credits

- X4 Station Calculator: https://x4-game.com/#/station-calculator
- X4: Foundations by Egosoft
