using System.Text.Json;

namespace SolitaireConsole.UI {
    /// <summary>
    /// Klasa odpowiedzialna za zarządzanie ustawieniami gry (ładowanie i zapisywanie)
    /// </summary>
    public class SettingsManager {
        private const string SETTINGS_FILE_NAME = "settings.json";
        private readonly string _settingsFilePath;

        public SettingsManager() {
            // Zapisz plik ustawień w katalogu z plikem exe
            _settingsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, SETTINGS_FILE_NAME);
        }

        /// <summary>
        /// Ładuje ustawienia z pliku lub tworzy domyślne ustawienia jeśli plik nie istnieje
        /// </summary>
        /// <returns>Załadowane lub domyślne ustawienia gry</returns>
        public GameSettings LoadSettings() {
            try {
                if (File.Exists(_settingsFilePath)) {
                    string json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonSerializer.Deserialize<GameSettings>(json);
                    return settings ?? new GameSettings();
                }
            } catch (Exception ex) {
                // Jeśli wystąpi błąd podczas ładowania, użyj domyślnych ustawień
                Console.WriteLine($"Ostrzeżenie: Nie można załadować ustawień ({ex.Message}). Używam domyślnych ustawień.");
            }

            return new GameSettings();
        }

        /// <summary>
        /// Zapisuje ustawienia do pliku
        /// </summary>
        /// <param name="settings">Ustawienia do zapisania</param>
        public void SaveSettings(GameSettings settings) {
            try {
                var options = new JsonSerializerOptions {
                    WriteIndented = true // Formatuj JSON dla lepszej czytelności
                };
                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(_settingsFilePath, json);
            } catch (Exception ex) {
                Console.WriteLine($"Ostrzeżenie: Nie można zapisać ustawień ({ex.Message}).");
            }
        }
    }
}
