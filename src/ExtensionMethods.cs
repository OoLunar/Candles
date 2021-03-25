using Gtk;

namespace Candles
{
	public static class ExtensionMethods
	{
		public static ListBoxRow CreateRow(string text)
		{
			Label label = new(text);
			label.LineWrap = true;

			ListBoxRow row = new();
			row.Selectable = true;
			row.CanFocus = true;
			row.Margin = 6;
			row.Add(label);

			return row;
		}
	}
}
