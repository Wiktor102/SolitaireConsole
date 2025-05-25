namespace SolitaireConsole {
    /// <summary>
    /// Klasa do zarządzania najlepszymi wynikami.
    /// </summary>
    public class HighScoreManager {
        private readonly string filePath; // Ścieżka do pliku z wynikami
        private List<(string Name, int Score)> highScores; // Lista wyników (Imię, Liczba ruchów)

        /// <summary>
        /// Tworzy nowy obiekt HighScoreManager i wczytuje wyniki z pliku.
        /// </summary>
        /// <param name="fileName">Nazwa pliku z wynikami.</param>
        public HighScoreManager(string fileName) {
            filePath = Path.Combine(Environment.CurrentDirectory, fileName); // Zapisuje w katalogu aplikacji
            highScores = LoadScores();
        }

        /// <summary>
        /// Wczytuje wyniki z pliku.
        /// </summary>
        /// <returns>Lista wyników (Imię, Liczba ruchów).</returns>
        private List<(string Name, int Score)> LoadScores() {
            var scores = new List<(string Name, int Score)>();
            if (!File.Exists(filePath)) {
                return scores; // Zwraca pustą listę, jeśli plik nie istnieje
            }

            try {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines) {
                    string[] parts = line.Split(','); // Zakładamy format: INICJAŁY,WYNIK
                    if (parts.Length == 2 && int.TryParse(parts[1], out int score)) {
                        scores.Add((parts[0].Trim(), score));
                    }
                }
            } catch (Exception ex) {
                Console.WriteLine($"Błąd podczas wczytywania rankingu: {ex.Message}");
                // Kontynuuje z pustą listą lub tym, co udało się wczytać
            }

            // Sortuje wyniki (im mniej ruchów, tym lepiej)
            scores.Sort((a, b) => a.Score.CompareTo(b.Score));
            return scores;
        }

        /// <summary>
        /// Dodaje nowy wynik i zapisuje do pliku.
        /// </summary>
        /// <param name="name">Imię gracza.</param>
        /// <param name="score">Wynik (liczba ruchów).</param>
        public void AddScore(string name, int score) {
            highScores.Add((name, score));
            // Sortuje ponownie po dodaniu nowego wyniku
            highScores.Sort((a, b) => a.Score.CompareTo(b.Score));
            // Opcjonalnie: ogranicz liczbę zapisanych wyników, np. do Top 10
            // if (highScores.Count > 10) highScores = highScores.Take(10).ToList();

            SaveScores(); // Zapisuje zaktualizowaną listę do pliku
        }

        /// <summary>
        /// Zapisuje wyniki do pliku.
        /// </summary>
        private void SaveScores() {
            try {
                // Tworzy listę linii w formacie "INICJAŁY,WYNIK"
                var lines = highScores.Select(score => $"{score.Name},{score.Score}");
                File.WriteAllLines(filePath, lines); // Nadpisuje plik nowymi wynikami
            } catch (Exception ex) {
                Console.WriteLine($"Błąd podczas zapisywania rankingu: {ex.Message}");
            }
        }

        /// <summary>
        /// Zwraca ranking do wyświetlenia w konsoli.
        /// </summary>
        /// <returns>Tablica stringów z wynikami.</returns>
        public string[] GetDisplayStrings() {
			if (highScores.Count == 0) {
                return ["Brak zapisanych wyników"];
            }

			List<string> s = [];
            int rank = 1;
            foreach (var (Name, Score) in highScores) {
				s.Add($"{rank}. {Name,-8}: {Score}");
                rank++;
            }

			return s.ToArray();
        }
    }
}