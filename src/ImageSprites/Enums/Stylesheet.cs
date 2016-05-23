namespace ImageSprites
{
    /// <summary>
    /// The type of stylesheets that the sprite exporter can produce.
    /// </summary>
    public enum Stylesheet
    {
        /// <summary>No stylesheet should be produced.</summary>
        None,

        /// <summary>Produce a .css file.</summary>
        Css,

        /// <summary>Produce a .less file.</summary>
        Less,

        /// <summary>Produce a .scss file.</summary>
        Scss
    }
}
