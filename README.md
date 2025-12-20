# MagusTui v0.2 (Terminal.Gui)

A local-first **terminal workspace** for Magus Search / Codex Research.

## Features
- Tabs: **Chat / Search / Notes / Logs**
- **Themes** (JSON-driven) + **F4** cycle
- **Backend** selector + **F3** cycle (Ollama / LM Studio / OpenAI / Offline)
- **Mode** selector + **F2** cycle (Explore / Build / Reflect)
- Startup **splash screen**
- **Command Palette** (Ctrl+P) — fzf-ish filter + run
- Notes **filesystem sandbox** under `~/.magus/sandbox` (Open/Save/SaveAs)

## Run
```bash
dotnet restore
dotnet run
```

## Themes
Theme JSON lives in `Themes/*.json` and is copied to the output directory on build.

## Roadmap
- Real backend adapters (Ollama / OpenAI)
- Split panes (Navigator / Chat / Search / Notes)
- Search indexing & embeddings
- Export reports
