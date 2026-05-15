/// <summary>
/// Interface segregation: exposes only stamina-related concerns.
/// </summary>
public interface IStaminaSystem
{
    float CurrentStamina { get; }
    float MaxStamina { get; }
    bool HasStamina { get; }

    /// <summary>Tries to consume <paramref name="amount"/> stamina. Returns false if not enough.</summary>
    bool TryConsume(float amount);

    void Regenerate(float amount);
}
