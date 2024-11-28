using System;

#pragma warning disable IDE1006 // Naming Styles
namespace Unity.AI.Navigation
{
    public partial class NavMeshLink
    {
        /// <summary> Gets or sets whether the world positions of the link's edges update whenever
        /// the GameObject transform, the start transform or the end transform change at runtime. </summary>
        [Obsolete("autoUpdatePositions has been deprecated. Use autoUpdate instead. (UnityUpgradable) -> autoUpdate")]
        public bool autoUpdatePositions
        {
            get => autoUpdate;
            set => autoUpdate = value;
        }

        /// <summary> Gets or sets whether agents can traverse the link in both directions. </summary>
        /// <remarks> When a link connects to NavMeshes at both ends, agents can always traverse that link from the start position to the end position. When this property is set to `true` it allows the agents to traverse the link from the end position to the start position as well. When the value is `false` the agents will not traverse the link from the end position to the start position. </remarks>
        [Obsolete("biDirectional has been deprecated. Use bidirectional instead. (UnityUpgradable) -> bidirectional")]
        public bool biDirectional
        {
            get => bidirectional;
            set => bidirectional = value;
        }

        /// <summary> Gets or sets a value that determines the cost of traversing the link.</summary>
        /// <remarks> A negative value implies that the cost of traversing the link is obtained based on the area type.
        /// A positive or zero value overrides the cost associated with the area type.</remarks>
        [Obsolete("costOverride has been deprecated. Use costModifier instead. (UnityUpgradable) -> costModifier")]
        public float costOverride
        {
            get => costModifier;
            set => costModifier = value;
        }

        /// <summary> Replaces the link with a new one using the current settings. </summary>
        [Obsolete("UpdatePositions() has been deprecated. Use UpdateLink() instead. (UnityUpgradable) -> UpdateLink(*)")]
        public void UpdatePositions()
        {
            UpdateLink();
        }
    }
}
#pragma warning restore IDE1006 // Naming Styles
