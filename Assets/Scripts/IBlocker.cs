/// <summary>
/// Interface segregation: exposes only the blocking state that PlayerHealth needs.
/// This breaks the concrete dependency on BlockHandler (DIP fix).
/// </summary>
public interface IBlocker
{
    bool IsBlocking { get; }
    float DamageReductionPercent { get; }
}
