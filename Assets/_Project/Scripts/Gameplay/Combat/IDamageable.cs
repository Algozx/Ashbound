namespace Ashbound.Gameplay.Combat
{
    /// <summary>Anything a <see cref="Hitbox"/> can hit.</summary>
    public interface IDamageable
    {
        /// <summary>Receives a hit and reports whether it landed.</summary>
        bool TakeHit(in HitInfo hit);
    }
}
