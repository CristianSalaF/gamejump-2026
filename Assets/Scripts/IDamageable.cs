/// <summary>
/// Interface segregation: anything in the world that can receive damage implements this.
/// </summary>
public interface IDamageable
{
    void TakeDamage(float amount);
}
