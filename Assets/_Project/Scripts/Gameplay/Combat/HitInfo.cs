namespace Ashbound.Gameplay.Combat
{
    /// <summary>One hit, as delivered by a <see cref="Hitbox"/>.</summary>
    public struct HitInfo
    {
        public int Damage;
        public float Knockback;
        public float SourceX;
    }
}
