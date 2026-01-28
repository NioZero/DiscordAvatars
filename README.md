# DiscordAvatars

Aplicación de escritorio WinUI 3 para seleccionar hasta cuatro usuarios de un servidor de Discord y exportar sus nombres y avatares a archivos locales.

## Características
- Login basado en bot (solo `DISCORD_BOT_TOKEN`).
- Selección de servidor y usuarios con búsqueda local.
- 4 slots tipo “Player” con avatar placeholder por slot.
- Exportación manual a archivos `.txt` y `.png`.
- Persistencia de selección (servidor, usuarios y carpeta).

## Requisitos
- Windows 10/11
- .NET SDK
- Bot de Discord con **Server Members Intent** habilitado y agregado al servidor

## Configuración
Define la variable de entorno:
- `DISCORD_BOT_TOKEN`

## Ejecutar
```
dotnet run
```

## Uso rápido
1. Selecciona carpeta de salida.
2. Elige servidor y usuarios.
3. Ajusta nombres de archivos si lo deseas.
4. Presiona **Actualizar Archivos**.
