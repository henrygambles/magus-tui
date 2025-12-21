Magus Home Folder (~/.magus)

1) Extract this zip into your home directory so it becomes:
   ~/.magus/config.json
   ~/.magus/personas/persona1.md
   ~/.magus/themes/
   ~/.magus/memory/

2) Start Ollama and run a model, e.g.:
   ollama serve
   ollama run qwen2.5:7b

3) Run MagusTui:
   dotnet run --project MagusTui/MagusTui.csproj

Notes:
- Edit personas/persona1.md to change Tara's personality.
- conversation is stored at memory/conversation.jsonl
