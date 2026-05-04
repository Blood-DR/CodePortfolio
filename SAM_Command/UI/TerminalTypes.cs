public enum LineStyle
{
	Input,
	Output,
	Success,
	Warning,
	Error,
	System,
	Divider,
}

public class TerminalLine
{
	public string Text  { get; }
	public string Style { get; }

	public TerminalLine( string text, LineStyle style )
	{
		Text  = text;
		Style = style.ToString().ToLowerInvariant();
	}
}
