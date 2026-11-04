using Ashbound.Gameplay.Pickups;
using Ashbound.Gameplay.Player;

namespace Ashbound.Gameplay.Meta
{
    /// <summary>Where Embers are spent on permanent unlocks.</summary>
    public sealed class Altar : Interactable
    {
        private Progression progression;

        public override string Prompt => "Use the altar";

        /// <summary>Sets where purchases go, once the room is part of a run.</summary>
        public void Bind(Progression runProgression)
        {
            progression = runProgression;
        }

        public override void Interact(PlayerCombat player)
        {
            progression.OpenAltar();
        }
    }
}
