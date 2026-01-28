# DiscordAvatars – Agent Notes

## Purpose
WinUI 3 desktop app to select up to four Discord users from a server (via bot token) and export their names/avatars to files.

## Requirements
- Windows 10/11 with .NET SDK installed.
- Discord bot token with **Server Members Intent** enabled.
- Bot must be added to the target server(s).

## Environment
Set:
- `DISCORD_BOT_TOKEN`

## Build / Run
- `dotnet build`
- `dotnet run`

## State & Output
- App state is saved to `%LocalAppData%\\DiscordAvatars\\appstate.json`.
- Exported files are written to the user-selected folder.

## Notes
- Member list comes from Discord bot API (`/guilds/{guildId}/members`).
- Export uses placeholders when a slot is inactive or a user has no avatar.
