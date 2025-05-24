using SolitaireConsole.Input;

namespace SolitaireConsole.UI {
    /// <summary>
    /// Abstract base class for settings items
    /// </summary>
    public abstract class SettingsItem(string name) {
		public string Name { get; } = name;
		public abstract string CurrentValueDisplay { get; }

		public abstract void ChangeValue(bool increase);
    }

	/// <summary>
	/// Settings item for enum values
	/// </summary>
	public class EnumSettingsItem<T>(string name, GameSettings gameSettings,
		Func<GameSettings, T> getter, Action<GameSettings, T> setter) : SettingsItem(name) where T : struct, Enum {
		private readonly GameSettings _gameSettings = gameSettings;
		private readonly Func<GameSettings, T> _getter = getter;
		private readonly Action<GameSettings, T> _setter = setter;
		private readonly T[] _values = Enum.GetValues<T>();

		public override string CurrentValueDisplay {
			get {
				T currentValue = _getter(_gameSettings);
				return currentValue switch {
					InputMode.Text => "Tekstowy",
					InputMode.Arrow => "Strzałkowy",
					_ => currentValue.ToString()
				};
			}
		}

		public override void ChangeValue(bool increase) {
			T currentValue = _getter(_gameSettings);
			int currentIndex = Array.IndexOf(_values, currentValue);

			if (increase) {
				currentIndex = (currentIndex + 1) % _values.Length;
			} else {
				currentIndex = (currentIndex - 1 + _values.Length) % _values.Length;
			}

			_setter(_gameSettings, _values[currentIndex]);
		}
	}

	/// <summary>
	/// Settings item for boolean values
	/// </summary>
	public class BoolSettingsItem(string name, GameSettings gameSettings,
		Func<GameSettings, bool> getter, Action<GameSettings, bool> setter) : SettingsItem(name) {
		private readonly GameSettings _gameSettings = gameSettings;
		private readonly Func<GameSettings, bool> _getter = getter;
		private readonly Action<GameSettings, bool> _setter = setter;

		public override string CurrentValueDisplay => _getter(_gameSettings) ? "TAK" : "NIE";

		public override void ChangeValue(bool increase) {
			_setter(_gameSettings, !_getter(_gameSettings));
		}
	}
}
