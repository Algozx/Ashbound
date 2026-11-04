using System;

namespace Ashbound.Logic.Levels
{
    /// <summary>How much harder each room of a run gets.</summary>
    public sealed class DifficultyCurve
    {
        private readonly float statStep;
        private readonly double baseSpawnChance;
        private readonly double spawnStep;

        public DifficultyCurve(float statStep, double baseSpawnChance, double spawnStep)
        {
            this.statStep = statStep;
            this.baseSpawnChance = baseSpawnChance;
            this.spawnStep = spawnStep;
        }

        /// <summary>Multiplier for enemy health and damage. The first room is 1.</summary>
        public float StatScale(int roomIndex)
        {
            return 1f + statStep * roomIndex;
        }

        public double SpawnChance(int roomIndex)
        {
            return Math.Min(1.0, baseSpawnChance + spawnStep * roomIndex);
        }
    }
}
