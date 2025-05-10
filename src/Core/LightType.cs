namespace GameFramework
{
    /// <summary>
    /// Defines the types of light sources.
    /// </summary>
    public enum LightType
    {
        /// <summary>
        /// Ambient light that illuminates all objects in the scene uniformly.
        /// </summary>
        Ambient,

        /// <summary>
        /// Light emitted from a single point in all directions.
        /// </summary>
        Point,

        /// <summary>
        /// Light emitted in a specific direction, as if from an infinitely distant source.
        /// </summary>
        Directional,

        /// <summary>
        /// Light emitted from a point in a specific direction, confined to a cone.
        /// </summary>
        Spot
    }
}
