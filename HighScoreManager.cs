namespace SolitaireConsole {
    // Klasa do zarządzania najlepszymi wynikami
    public class HighScoreManager {
        private readonly string filePath; // Ścieżka do pliku z wynikami
        private List<(string Name, int Score)> highScores; // Lista wyników (Imię, Liczba ruchów)

        // Konstruktor
        public HighScoreManager(string fileName) {
            filePath = Path.Combine(Environment.CurrentDirectory, fileName); // Zapisuje w katalogu aplikacji
            highScores = LoadScores();
        }

        // Wczytuje wyniki z pliku
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

        // Dodaje nowy wynik i zapisuje do pliku
        public void AddScore(string name, int score) {
            highScores.Add((name, score));
            // Sortuje ponownie po dodaniu nowego wyniku
            highScores.Sort((a, b) => a.Score.CompareTo(b.Score));
            // Opcjonalnie: ogranicz liczbę zapisanych wyników, np. do Top 10
            // if (highScores.Count > 10) highScores = highScores.Take(10).ToList();

            SaveScores(); // Zapisuje zaktualizowaną listę do pliku
        }

        // Zapisuje wyniki do pliku
        private void SaveScores() {
            try {
                // Tworzy listę linii w formacie "INICJAŁY,WYNIK"
                var lines = highScores.Select(score => $"{score.Name},{score.Score}");
                File.WriteAllLines(filePath, lines); // Nadpisuje plik nowymi wynikami
            } catch (Exception ex) {
                Console.WriteLine($"Błąd podczas zapisywania rankingu: {ex.Message}");
            }
        }

        // Wyświetla ranking w konsoli
        public void DisplayScores() {
            if (highScores.Count == 0) {
                Console.WriteLine("Brak zapisanych wyników.");
                return;
            }

            Console.WriteLine(" # | Inicjały | Ruchy");
            Console.WriteLine("---|----------|-------");
            int rank = 1;
            foreach (var score in highScores) {
                Console.WriteLine($"{rank,2} | {score.Name,-8} | {score.Score}");
                rank++;
            }
        }
    }
}