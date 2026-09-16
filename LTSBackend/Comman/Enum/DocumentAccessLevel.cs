namespace LTSBackend.Comman.Enum
{
    public enum DocumentAccessLevel
    {
        /// <summary>
        /// No access
        /// </summary>
        None = 0,

        /// <summary>
        /// Write-only (upload only, no view/download)
        /// </summary>
        WriteOnly = 1,

        /// <summary>
        /// Read-only (view only, no download)
        /// </summary>
        ReadOnly = 2,

        /// <summary>
        /// Read-Write (view and download)
        /// </summary>
        ReadWrite = 3,

        /// <summary>
        /// Full access (view, download, upload)
        /// </summary>
        FullAccess = 4
    }
}
