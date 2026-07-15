# 🎛️ ThePixelSoundboard

<div align="center">

**La soundboard definitiva per i ThePixelBoys — Semplice, leggera e potente**

![Windows](https://img.shields.io/badge/Windows-10%2F11-blue?logo=windows)
![.NET 8](https://img.shields.io/badge/.NET-8.0-purple?logo=dotnet)
![License](https://img.shields.io/badge/License-MIT-green)
![Version](https://img.shields.io/badge/Version-2.5.0-orange)

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
| 🎧 **Configurazione Audio** | Supporta cuffie, VB-Cable per Discord e microfono di loopback |
| 📂 **Cartelle** | Organizza i suoni in cartelle separate |
| 🚀 **Bassa Latenza** | Motore audio NAudio con overlay fluido dei suoni |

---

## 🎙️ Come funziona il routing audio su Discord?

ThePixelSoundboard funziona misando l'audio della soundboard e la tua voce in un cavo virtuale (es. VB-Cable) che puoi impostare come microfono su Discord.

```
[Microfono Reale] ──┐
                    ├──► [ThePixelSoundboard Mixer] ──► [Cavo Virtuale (es. CABLE Input)]
[Suoni Soundboard] ─┘                                            │
                                                                 ▼
                                                  [Discord legge "CABLE Output"]
                                                                 │
                                                                 ▼
                                                      👥 I tuoi amici sentono tutto!
```

---

## 📥 Installazione e Configurazione

1. Scarica `ThePixelSoundboard_v2.5.0_Setup.exe` dalla sezione [**Releases**](../../releases).
2. Esegui l'installazione guidata sul tuo PC.
3. Al primo avvio, l'**Onboarding Setup Wizard** ti aiuterà a configurare in pochi secondi:
   - Il tuo dispositivo di ascolto principale (le tue cuffie).
   - Il cavo virtuale (es. CABLE Input / VB-Cable) per trasmettere ai tuoi amici.
   - Il tuo microfono reale per il loopback della voce.
4. Su Discord → Impostazioni → Voce e video → Imposta come dispositivo di ingresso lo stesso cavo virtuale (es. CABLE Output).

*Se sei un utente esperto, puoi saltare l'introduzione guidata usando il tasto dedicato **"Sono un Esperto"** per andare subito alla selezione dei dispositivi.*

---

## 🛠️ Compilare da Sorgente

### Requisiti
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- Windows 10 / 11 (64-bit)
- [Inno Setup 6](https://jrsoftware.org/isinfo.php) (opzionale, solo per creare l'installer)

### Build
```bash
git clone https://github.com/marcolinovalorantoso-source/ThePixelSoundboard.git
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
│   ├── Views/                   # Finestre XAML (Main, Settings, Onboarding, ecc.)
│   ├── Services/                # AudioEngine, SettingsService, StartupService
│   └── app_icon.ico             # Icona dell'applicazione
├── installer.iss                # Script Inno Setup
├── .gitignore
└── README.md
```

---

## 🔧 Tecnologie Utilizzate

- **C# / WPF (.NET 8)** — UI nativa Windows con MVVM
- **NAudio** — Motore audio: mixing, playback, registrazione, loopback
- **NVorbis** — Supporto file `.ogg`
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
