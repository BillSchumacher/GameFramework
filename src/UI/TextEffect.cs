namespace GameFramework.UI
{
    /// <summary>
    /// Defines different visual effects that can be applied to text in a LabelWidget.
    /// </summary>
    public enum TextEffect
    {
        /// <summary>
        /// No special effect is applied. Text renders normally.
        /// </summary>
        None,

        /// <summary>
        /// Characters in the text bounce up and down.
        /// </summary>
        Bounce,

        /// <summary>
        /// Text characters bounce independently with random offsets, speeds, and heights.
        /// </summary>
        RandomBounce
    }
}
