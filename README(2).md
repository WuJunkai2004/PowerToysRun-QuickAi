# PowerToys Run QuickAI

A PowerToys Run plugin that brings AI assistance directly to your launcher with multi-provider support and streaming responses.

## Features

- 🚀 **Fast Streaming Responses**: Real-time token-by-token display
- 🔄 **Multi-Provider Support**: Choose from Groq, Together, Fireworks, OpenRouter, or Cohere
- 🔑 **Dual API Key Support**: Primary key with automatic secondary key fallback
- ⚙️ **Highly Configurable**: Customize model, temperature, and max tokens via PowerToys Settings
- 📋 **One-Click Copy**: Press Enter to copy responses to clipboard
- 🎨 **Theme Support**: Includes icons for both dark and light themes

## Supported AI Providers

| Provider | Endpoint | Default Model |
|----------|----------|---------------|
| Groq | `https://api.groq.com/openai/v1/chat/completions` | llama-3.1-8b-instruct |
| Together | `https://api.together.xyz/v1/chat/completions` | llama-3.1-8b-instruct |
| Fireworks | `https://api.fireworks.ai/inference/v1/chat/completions` | llama-3.1-8b-instruct |
| OpenRouter | `https://openrouter.ai/api/v1/chat/completions` | llama-3.1-8b-instruct |
| Cohere | `https://api.cohere.com/v1/chat` | command |

## Installation

### Option 1: Build from Source

1. Clone this repository
2. Build the project for .NET 9:
   ```bash
   dotnet build -c Release
   ```
3. Copy the output folder to your PowerToys plugins directory:
   ```
   %LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\Plugins\
   ```

### Option 2: Integration with PowerToys Source

1. Clone the PowerToys repository
2. Copy this plugin folder to `src/modules/launcher/Plugins/Community.PowerToys.Run.Plugin.QuickAI/`
3. Add project reference to the PowerLauncher solution
4. Build PowerToys

## Configuration

1. Open PowerToys Settings
2. Navigate to PowerToys Run → Plugins
3. Find "QuickAI" in the list
4. Click the plugin settings icon to configure:

### Settings

- **API Provider**: Select your preferred AI provider (Groq, Together, Fireworks, OpenRouter, Cohere)
- **Primary API Key**: Your main API key for authentication
  - ⚠️ **Security Note**: Keys are stored in plain text in PowerToys settings JSON
- **Secondary API Key** (Optional): Fallback key if primary fails
- **Model Name**: The AI model to use (default: `llama-3.1-8b-instruct`)
- **Max Tokens**: Maximum response length (default: 128 for fast responses)
- **Temperature**: Response creativity 0.0-2.0 (default: 0.2 for focused answers)

## Usage

1. Open PowerToys Run with `Alt+Space`
2. Type the action keyword: `ai <your question>`
3. Watch the response stream in real-time
4. Press `Enter` to copy the full response to clipboard

### Examples

```
ai what is recursion?
ai explain quantum computing in simple terms
ai write a haiku about coding
ai how do I reverse a linked list?
```

## Technical Details

### Architecture

- **Target Framework**: .NET 9.0 Windows
- **Plugin Type**: Dynamic Loading Library
- **HTTP Client**: Static singleton with 30-second timeout
- **Streaming**: Server-Sent Events (SSE) parsing
- **JSON**: System.Text.Json for serialization

### Implementation Highlights

- Implements `IPlugin` for PowerToys Run integration
- Implements `ISettingProvider` for GUI configuration via `AdditionalOptions`
- Async HTTP requests with `Task.Run()` for non-blocking queries
- Real-time result updates during streaming
- Automatic retry with secondary API key on failure
- Thread-safe response building with `StringBuilder`

### Error Handling

- Validates API key presence before requests
- Handles network failures gracefully
- Shows user-friendly error messages
- Timeout protection (30 seconds)
- JSON parsing error recovery

## API Key Acquisition

### Groq
1. Visit [https://console.groq.com](https://console.groq.com)
2. Sign up or log in
3. Navigate to API Keys section
4. Create a new API key

### Together
1. Visit [https://api.together.xyz](https://api.together.xyz)
2. Create an account
3. Go to Settings → API Keys
4. Generate a new key

### Fireworks
1. Visit [https://fireworks.ai](https://fireworks.ai)
2. Sign up for an account
3. Navigate to API Keys
4. Create a new key

### OpenRouter
1. Visit [https://openrouter.ai](https://openrouter.ai)
2. Sign up or log in
3. Go to Keys section
4. Generate an API key

### Cohere
1. Visit [https://cohere.com](https://cohere.com)
2. Create an account
3. Navigate to API Keys
4. Create a new key

## Security Considerations

⚠️ **Important**: API keys are currently stored in plain text in PowerToys settings JSON file at:
```
%LOCALAPPDATA%\Microsoft\PowerToys\PowerToys Run\settings.json
```

**Recommendations**:
- Use API keys with limited permissions or quotas
- Do not use production/billing keys
- Consider rotating keys periodically
- Future enhancement: Implement Windows DPAPI encryption

## Troubleshooting

### Plugin doesn't appear in PowerToys Run
- Ensure DynamicLoading is enabled in plugin.json
- Check that all files (plugin.json, DLL, icons) are in the plugin folder
- Restart PowerToys

### "API Key not configured" error
- Open PowerToys Settings → PowerToys Run → Plugins
- Configure the Primary API Key for your selected provider

### No response received
- Verify your API key is valid
- Check your internet connection
- Try the secondary API key
- Verify the provider endpoint is accessible
- Check model name is correct for the selected provider

### Streaming not working
- Ensure you're using the latest version
- Check that the provider supports streaming
- Verify HTTP timeout isn't too short

## Development

### Build Requirements

- .NET 9 SDK
- Windows 10/11
- Visual Studio 2022 (recommended) or VS Code

### Testing

```bash
# Build
dotnet build

# Clean
dotnet clean

# Restore packages
dotnet restore
```

### Project Structure

```
PowerToysRunQuickAi/
├── Main.cs                          # Core plugin implementation
├── plugin.json                       # Plugin manifest
├── PowerToysRunQuickAi.csproj       # Project file
├── Images/
│   ├── ai.dark.png                  # Dark theme icon
│   └── ai.light.png                 # Light theme icon
└── README.md                        # This file
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

### Areas for Enhancement

- [ ] DPAPI encryption for API keys
- [ ] Response caching
- [ ] Custom system prompts
- [ ] Conversation history
- [ ] Token usage tracking
- [ ] More provider integrations
- [ ] Custom endpoint support
- [ ] Markdown rendering

## License

This project follows the PowerToys project license (MIT License).

## Credits

- Built for [Microsoft PowerToys](https://github.com/microsoft/PowerToys)
- Uses the PowerToys Run plugin SDK
- Supports multiple AI provider APIs

## Support

For issues, questions, or contributions, please visit the repository issues page.

## Changelog

### Version 1.0.0 (Initial Release)
- Multi-provider support (Groq, Together, Fireworks, OpenRouter, Cohere)
- Streaming responses with real-time display
- Configurable via PowerToys Settings UI
- Primary/secondary API key fallback
- Customizable model, temperature, and max tokens
- Clipboard integration
- Theme-aware icons

---

**Note**: This plugin requires valid API keys from your chosen provider(s). Most providers offer free tiers for testing and development.
