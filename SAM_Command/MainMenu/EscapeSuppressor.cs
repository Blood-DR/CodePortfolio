using Sandbox;

/// Consumes the escape key every frame so the S&Box platform overlay
/// never opens.
public sealed class EscapeSuppressor : Component
{
	protected override void OnUpdate()
	{
		if ( Input.EscapePressed )
			Input.EscapePressed = false;
	}
}
