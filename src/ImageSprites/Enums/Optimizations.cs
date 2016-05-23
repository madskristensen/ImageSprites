namespace ImageSprites
{
    /// <summary>
    /// Image optimization types used to compress and optimmize the output sprite image.
    /// </summary>
    public enum Optimizations
    {
        /// <summary>No optimization will by applied.</summary>
        None,

        /// <summary>No quality loss but the image might benefit from more compression.</summary>
        Lossless,

        /// <summary>Maximum optimization with some but limited quality loss.</summary>
        Lossy
    }
}
