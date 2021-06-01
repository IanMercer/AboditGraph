namespace Abodit.Graph
{
    /// <summary>
    /// Style for a node
    /// </summary>
    public enum DotGraphNodeStyle
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// default should be mapped to "" or better omitted
        /// </summary>
        @default,

        dashed,
        dotted,
        solid,
        invis,
        bold,
        filled,
        striped,
        wedged,
        diagonals,
        rounded
    }

    /// <summary>
    /// Node shapes
    /// </summary>
    public enum DotGraphNodeShape
    {
        box, polygon, ellipse, oval, circle, point, egg, triangle, plaintext, plain,
        diamond, trapezium, parallelogram, house, pentagon, hexagon, septagon, octagon,
        doublecircle, doubleoctagon, tripleoctagon, invtriangle, invtrapezium, invhouse,
        Mdiamond, Msquare, Mcircle, rect, rectangle, square, star, none, underline, cylinder,
        note, tab, folder, box3d, component, promoter, cds, teminator, utr, primersite, restrictionsite,
        fivepowerhang, threepoverhang, noverhang, assembly, signature, insulator, ribosite, rnastab,
        proteasesite, proteinstab, rpromoter, rarrow, larrow, lpromoter
    }

#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}