namespace ThaiX.Domain.Aggregates.TcbsTop10;

/// <summary>Kind of file attached to a TCBS portfolio update.</summary>
public enum TcbsAttachmentKind
{
    /// <summary>Image stored on the iwealthclub platform (rendered inline).</summary>
    Image = 0,

    /// <summary>PDF research file referenced by URL (rendered as a link).</summary>
    Pdf = 1
}
