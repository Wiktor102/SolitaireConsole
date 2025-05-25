namespace SolitaireConsole.Input {
	/// <summary>
	/// Określa tryb wprowadzania danych dla aplikacji.
	/// </summary>
	/// <remarks>
	/// Tryb <see cref="Text"/> umożliwia wprowadzanie poleceń tekstowych,
	/// natomiast tryb <see cref="Arrow"/> pozwala na nawigację za pomocą klawiszy strzałek.
	/// </remarks>
    public enum InputMode {
        Text,
        Arrow
    }
}
