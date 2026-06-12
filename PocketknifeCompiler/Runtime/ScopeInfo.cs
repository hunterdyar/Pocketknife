namespace PocketknifeCore;

// Marker on the scope stack identifying where in the timeline a scope opened and what kind of merge to apply when it closes.
public struct ScopeInfo
{
	// Index in _timeline of the layer that was current when the scope opened.
	// On PopFrame, descendants are gathered from the current top layer back to items in _timeline[StartLayerIndex].
	public int StartLayerIndex;

	// Optional name (`.@x`) — when present, the merged value is bound on each produced item so it (and its later descendants) can resolve `@x`.
	public string? Name;

	// True if this scope was opened by a `>`-style input expansion (PushStreamWithGenerator on a non-empty timeline).
	// Affects whether PopFrame collapses parent-by-parent (expansion) or 1-to-1 (cloned branch).
	// AKA expansion is > or |> operators, 1-1 is . operator.
	public bool IsExpansionScope;
	
	
}
