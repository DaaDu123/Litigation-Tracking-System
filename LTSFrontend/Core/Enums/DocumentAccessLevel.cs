namespace LTSFrontend.Core.Enums
{








    public enum DocumentAccessLevel
    {

        None = 0,


        WriteOnly = 1,


        ReadOnly = 2,


        ReadWrite = 3,


        FullAccess = 4
    }


    public static class DocumentAccessLevelExtensions
    {

        public static bool CanView(this DocumentAccessLevel level)
        {
            return level is DocumentAccessLevel.ReadOnly or DocumentAccessLevel.ReadWrite or DocumentAccessLevel.FullAccess;
        }


        public static bool CanDownload(this DocumentAccessLevel level)
        {
            return level is DocumentAccessLevel.ReadWrite or DocumentAccessLevel.FullAccess;
        }


        public static bool CanUpload(this DocumentAccessLevel level)
        {
            return level is DocumentAccessLevel.WriteOnly or DocumentAccessLevel.FullAccess;
        }


        public static string ToDisplayText(this DocumentAccessLevel level)
        {
            return level switch
            {
                DocumentAccessLevel.None => "No Access",
                DocumentAccessLevel.WriteOnly => "Upload Only",
                DocumentAccessLevel.ReadOnly => "View Only",
                DocumentAccessLevel.ReadWrite => "View & Download",
                DocumentAccessLevel.FullAccess => "Full Access",
                _ => "Unknown"
            };
        }
    }
}
