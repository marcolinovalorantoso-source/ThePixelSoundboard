# 🎛️ ThePixelSoundboard

<div align="center">

**La soundboard definitiva per i ThePixelBoys — con driver audio virtuale integrato**

![Windows](https://img.shields.io/badge/Windows-10%2F11-blue?logo=windows)
![.NET 8](https://img.shields.io/badge/.NET-8.0-purple?logo=dotnet)
![License](https://img.shields.io/badge/License-MIT-green)
![Version](https://img.shields.io/badge/Version-1.0.0-orange)

</div>

---

## ✨ Caratteristiche Principali

| Feature | Descrizione |
|---|---|
| 🎙️ **Registratore Istantaneo** | Registra dal microfono e crea un pulsante in pochi secondi |
| 🗣️ **Text-To-Speech** | Voci divertenti tramite Windows TTS o Google Translate |
| 🎵 **Drag & Drop** | Importa `.mp3`, `.wav`, `.ogg` trascinandoli nella board |
| 🎨 **Personalizzazione Totale** | Colore, emoji, icona e nome per ogni suono |
| ⌨️ **Global Hotkeys** | Tasti rapidi che funzionano anche in gioco a schermo intero |
| 🎧 **Driver Audio Virtuale** | Mix in tempo reale di voce + meme su Discord (come Soundpad!) |
| 📂 **Cartelle** | Organizza i suoni in cartelle separate |
| 🚀 **Bassa Latenza** | Motore audio NAudio con overlay fluido dei suoni |

---

## 🎙️ Come funziona il Driver Audio Virtuale?

ThePixelSoundboard funziona **esattamente come Soundpad**: installa un driver audio virtuale (canale ponte) che permette ai tuoi amici su Discord di sentire **sia la tua voce che i meme** contemporaneamente.

```
[Microfono Reale] ──┐
                    ├──► [ThePixelSoundboard Mixer] ──► [ThePixelSoundboard Audio (Virtual)]
[Suoni Soundboard] ─┘                                            │
                                                                 ▼
                                              [Discord legge "ThePixelSoundboard Mic"]
                                                                 │
                                                                 ▼
                                                      👥 I tuoi amici sentono tutto!
```

---

## 📥 Installazione

### Versione Consigliata (con Driver Audio)
1. Scarica `ThePixelSoundboard_Setup.exe` dalla sezione [**Releases**](../../releases)
2. Avvia il setup e scegli **"Installazione completa (con Driver Audio Virtuale)"**
3. Accetta il permesso amministratore (richiesto per il driver)
4. Riavvia il PC
5. Apri Discord → Impostazioni → Voce e video → Seleziona **"ThePixelSoundboard Mic"** come microfono

### Versione Libera (senza Driver)
1. Scarica `ThePixelSoundboard_Setup.exe` dalla sezione [**Releases**](../../releases)
2. Scegli **"Installazione libera (senza Driver)"**
3. Nelle impostazioni dell'app, seleziona manualmente i due output audio

---

## ⚙️ Impostazioni Audio

| Con Driver Virtuale | Senza Driver |
|---|---|
| ✅ Driver rilevato automaticamente | ⚠️ Nessun driver |
| 🎧 Output → Tue cuffie | 🎧 Output → Tue cuffie |
| 🎤 Input → Tuo microfono reale | 🎧 Output Amici → Selezione manuale |
| 💬 Guida Discord integrata | — |

---

## 🛠️ Compilare da Sorgente

### Requisiti
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- Windows 10 / 11 (64-bit)
- [Inno Setup 6](https://jrsoftware.org/isinfo.php) (opzionale, solo per creare l'installer)

### Build
```bash
git clone https://github.com/TUO_USERNAME/ThePixelSoundboard.git
cd ThePixelSoundboard
dotnet build SoundBoard/SoundBoard.csproj
dotnet run --project SoundBoard/SoundBoard.csproj
```

### Creare l'Installer
```bash
# Compila in Release
dotnet publish SoundBoard/SoundBoard.csproj -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true

# Compila l'installer con Inno Setup
ISCC.exe installer.iss
```

---

## 🗂️ Struttura del Progetto

```
ThePixelSoundboard/
├── SoundBoard/                  # Progetto principale WPF
│   ├── Models/                  # Modelli dati (AppSettings, SoundButton, ecc.)
│   ├── ViewModels/              # ViewModel principale (MVVM)
│   ├── Views/                   # Finestre XAML (Main, Settings, Recorder, ecc.)
│   ├── Services/                # AudioEngine, SettingsService, StartupService
│   └── app_icon.ico             # Icona dell'applicazione
├── installer/                   # File per l'installer
│   ├── vbcable/                 # Driver VB-Cable (incluso nella versione completa)
│   └── rename_device.ps1        # Script rinominazione dispositivi audio
├── installer.iss                # Script Inno Setup
├── .gitignore
└── README.md
```

---

## 🔧 Tecnologie Utilizzate

- **C# / WPF (.NET 8)** — UI nativa Windows con MVVM
- **NAudio** — Motore audio: mixing, playback, registrazione, loopback
- **NVorbis** — Supporto file `.ogg`
- **VB-Audio Virtual Cable** — Driver audio virtuale (incluso nell'installer completo)
- **Inno Setup 6** — Wizard di installazione

---

## 🐣 Easter Eggs

> Alcune sorprese sono nascoste nell'app... 👀 Prova a cliccare il logo 5 volte, oppure digita qualcosa di speciale sulla tastiera!

---

## 📄 Licenza

Distribuito sotto licenza **MIT**. Vedi [LICENSE](LICENSE) per i dettagli.

---

<div align="center">
Fatto con ❤️ per i <b>ThePixelBoys</b> 🎮
</div>
