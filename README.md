# AquaScale

1. Install .NET 8 SDK, Node.js (LTS), Git
2. Clone this repo
3. `cd AquaScale.Api && dotnet restore`
4. `cd AquaScale.Client && npm install`
5. Copy `appsettings.Development.json.example` → `appsettings.Development.json`, 
   fill in real Supabase connection string (ask [your name] for credentials — do not share in group chat)

## Status (as of July 22, 2026)
- Mirror sync (WEBS ↔ Supabase, pull + push) — working, tested, in AquaScale.Syncer repo
- billing_statements — created manually in Supabase, not yet EF-managed here
- Open questions pending client/mentor: billing void/reversal flow, SeqNo ownership
- ERD needs a pass to match real WEBS schema discoveries — see AquaScale.Syncer's schema-notes/
