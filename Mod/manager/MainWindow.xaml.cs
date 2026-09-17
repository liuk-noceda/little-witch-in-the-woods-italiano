using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using LiukNoceda.LittleWitchManager.Core;
using Microsoft.Win32;

using Module = LiukNoceda.LittleWitchManager.Core.Module;

namespace LiukNoceda.LittleWitchManager;

public partial class MainWindow : Window
{
    private readonly string _appDir = AppContext.BaseDirectory;
    private readonly StringBuilder _log = new();
    private readonly string _logPath;

    private bool _syncing;
    private bool _primoAvvio = true;
    private string _gamePath = "";

    private PercorsiGioco Paths => PercorsiGioco.Per(_gamePath);
    private bool PathOk => !string.IsNullOrWhiteSpace(_gamePath) && Paths.GiocoValido;

    public MainWindow()
    {
        InitializeComponent();
        _logPath = Path.Combine(Path.GetTempPath(), "LittleWitchItaliano_install.log");

        VersionText.Text = "v" + (Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0");
        LoadImages();

        var found = TrovaGioco.Cerca();
        _gamePath = found ?? "";
        Log(found is null
            ? "Little Witch in the Woods non trovato: indica la cartella con «cambia»."
            : $"Gioco trovato in {found}");

        RefreshState();
    }

    private void LoadImages()
    {
        var logoPath = Path.Combine(_appDir, "Resources", "logo.png");
        if (File.Exists(logoPath))
        {
            try { LogoImage.Source = new BitmapImage(new Uri(logoPath)); } catch { }
        }
    }

    private void RefreshState()
    {
        if (!PathOk)
        {
            _syncing = true;
            TransCheck.IsChecked = PadCheck.IsChecked = false;
            _syncing = false;

            TransCheck.IsEnabled = PadCheck.IsEnabled = false;
            ApplyButton.IsEnabled = false;
            RemoveAllButton.IsEnabled = false;
            TransState.Text = PadState.Text = "";

            PathIcon.Text = "!";
            PathIcon.Foreground = (System.Windows.Media.Brush)FindResource("Muted");
            PathText.Text = string.IsNullOrWhiteSpace(_gamePath)
                ? "Gioco non trovato — premi «cambia» e seleziona la cartella di Little Witch"
                : $"In «{_gamePath}» non c'è LWIW.exe";

            StatusText.Text = "Seleziona la cartella corretta del gioco per continuare.";
            return;
        }

        var p = Paths;
        var transInstalled = Module.Translation.IsInstalled(p);
        var padInstalled = Module.ControllerPrompts.IsInstalled(p);

        TransCheck.IsEnabled = PadCheck.IsEnabled = true;
        TransState.Text = transInstalled ? "installata" : "";
        PadState.Text = padInstalled ? "attivo" : "";

        PathIcon.Text = ""; // Segoe glyph check
        PathIcon.Foreground = (System.Windows.Media.Brush)FindResource("Ok");
        PathText.Text = _gamePath;

        if (_primoAvvio)
        {
            _syncing = true;
            if (!transInstalled && !padInstalled)
            {
                // Al primo avvio su gioco pulito, pre-seleziona entrambi
                TransCheck.IsChecked = true;
                PadCheck.IsChecked = true;
            }
            else
            {
                TransCheck.IsChecked = transInstalled;
                PadCheck.IsChecked = padInstalled;
            }
            _syncing = false;
            _primoAvvio = false;
        }

        RefreshButtonState();
    }

    private void RefreshButtonState()
    {
        if (!PathOk) return;

        var p = Paths;
        var transInstalled = Module.Translation.IsInstalled(p);
        var padInstalled = Module.ControllerPrompts.IsInstalled(p);

        bool wantTrans = TransCheck.IsChecked == true;
        bool wantPad = PadCheck.IsChecked == true;

        bool hasChanges = (wantTrans != transInstalled) || (wantPad != padInstalled);
        bool anyInstalled = transInstalled || padInstalled;

        RemoveAllButton.IsEnabled = anyInstalled;

        if (hasChanges)
        {
            ApplyButton.IsEnabled = true;
            ApplyButton.Content = anyInstalled ? "Applica modifiche" : "Installa";
            StatusText.Text = "Pronto ad applicare le modifiche selezionate.";
        }
        else
        {
            ApplyButton.IsEnabled = false;
            ApplyButton.Content = anyInstalled ? "Installato" : "Installa";
            StatusText.Text = anyInstalled
                ? "I moduli selezionati sono installati e pronti."
                : "Seleziona i moduli che desideri installare.";
        }
    }

    private void Module_Changed(object sender, RoutedEventArgs e)
    {
        if (_syncing) return;
        RefreshButtonState();
    }

    private async void Apply_Click(object sender, RoutedEventArgs e)
    {
        if (!PathOk) return;

        if (Installatore.GiocoAperto())
        {
            MessageBox.Show(this,
                "Little Witch in the Woods è attualmente in esecuzione.\n\nChiudi il gioco prima di procedere.",
                "Gioco aperto", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        SetBusy(true);
        bool wantTrans = TransCheck.IsChecked == true;
        bool wantPad = PadCheck.IsChecked == true;

        try
        {
            await Task.Run(() =>
            {
                var p = Paths;
                var transInstalled = Module.Translation.IsInstalled(p);
                var padInstalled = Module.ControllerPrompts.IsInstalled(p);

                if (wantTrans && !transInstalled)
                    Installatore.InstallaModulo(_gamePath, Module.Translation, Log);
                else if (!wantTrans && transInstalled)
                    Installatore.DisinstallaModulo(_gamePath, Module.Translation, Log);

                if (wantPad && !padInstalled)
                    Installatore.InstallaModulo(_gamePath, Module.ControllerPrompts, Log);
                else if (!wantPad && padInstalled)
                    Installatore.DisinstallaModulo(_gamePath, Module.ControllerPrompts, Log);
            });

            StatusText.Text = "Operazione completata con successo!";
        }
        catch (Exception ex)
        {
            Log("ERRORE: " + ex.Message);
            MessageBox.Show(this, "Si è verificato un errore:\n" + ex.Message,
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SetBusy(false);
            RefreshState();
        }
    }

    private async void RemoveAll_Click(object sender, RoutedEventArgs e)
    {
        if (!PathOk) return;

        if (Installatore.GiocoAperto())
        {
            MessageBox.Show(this,
                "Little Witch in the Woods è attualmente in esecuzione.\n\nChiudi il gioco prima di procedere.",
                "Gioco aperto", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var res = MessageBox.Show(this,
            "Vuoi rimuovere tutti i componenti della patch e ripristinare lo stato originale?",
            "Conferma rimozione", MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (res != MessageBoxResult.Yes) return;

        SetBusy(true);
        try
        {
            await Task.Run(() =>
            {
                Installatore.RipristinaOriginale(_gamePath, Log);
            });

            _syncing = true;
            TransCheck.IsChecked = false;
            PadCheck.IsChecked = false;
            _syncing = false;

            StatusText.Text = "Gioco ripristinato allo stato originale.";
        }
        catch (Exception ex)
        {
            Log("ERRORE: " + ex.Message);
            MessageBox.Show(this, "Si è verificato un errore durante la rimozione:\n" + ex.Message,
                "Errore", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            SetBusy(false);
            RefreshState();
        }
    }

    private void ChangePath_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFolderDialog
        {
            Title = "Seleziona la cartella di Little Witch in the Woods",
            Multiselect = false,
        };
        if (!string.IsNullOrWhiteSpace(_gamePath) && Directory.Exists(_gamePath))
            dlg.InitialDirectory = _gamePath;

        if (dlg.ShowDialog(this) == true)
        {
            _gamePath = dlg.FolderName;
            _primoAvvio = true;
            RefreshState();
        }
    }

    private void LogButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            File.WriteAllText(_logPath, _log.ToString(), Encoding.UTF8);
            Process.Start(new ProcessStartInfo("notepad.exe", $"\"{_logPath}\"") { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, "Impossibile aprire il log:\n" + ex.Message, "Log", MessageBoxButton.OK);
        }
    }

    private void Info_Click(object sender, RoutedEventArgs e) =>
        InfoOverlay.Visibility = Visibility.Visible;

    private void CloseInfo_Click(object sender, RoutedEventArgs e) =>
        InfoOverlay.Visibility = Visibility.Collapsed;

    private void SetBusy(bool busy)
    {
        Progress.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        ApplyButton.IsEnabled = !busy;
        RemoveAllButton.IsEnabled = !busy;
        TransCheck.IsEnabled = !busy;
        PadCheck.IsEnabled = !busy;
        ChangePathButton.IsEnabled = !busy;
    }

    private void Log(string s)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {s}";
        _log.AppendLine(line);
    }
}
