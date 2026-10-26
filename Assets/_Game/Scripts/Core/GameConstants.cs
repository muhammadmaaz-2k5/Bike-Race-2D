namespace DirtDash.Core
{
    /// <summary>
    /// Global constants and configuration values for Dirt Dash.
    /// </summary>
    public static class GameConstants
    {
        public const string GameTitle = "Dirt Dash";
        public const string Version = "0.1.0";

        // Physics Layers and Tags
        public const string LayerGround = "Ground";
        public const string LayerTrack = "Track";
        public const string LayerBikeBody = "BikeBody";
        public const string LayerWheel = "Wheel";
        public const string LayerRider = "Rider";

        public const string TagPlayer = "Player";
        public const string TagFinish = "Finish";
        public const string TagCoin = "Coin";
        public const string TagHazard = "Hazard";

        // Physics Timestep Recommendation
        public const float RecommendedFixedDeltaTime = 0.015f; // ~66.7 Hz for rock-solid 2D physics

        // World Out-of-bounds kill Y threshold
        public const float DefaultKillY = -25f;
    }
}
