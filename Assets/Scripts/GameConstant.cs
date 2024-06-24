   public static class ProjectLayer
    {
        public const int Default = 1 << 0;
        public const int TransparentFX = 1 << 1;
        public const int Ignore_Raycast = 1 << 2;
        // public const int Default = 1 << 3;
        public const int Water = 1 << 4;
        public const int UI = 1 << 5;
        // public const int Default = 1 << 6;
        // public const int Default = 1 << 7;
        public const int Chip = 1 << 8;
        public const int Wire = 1 << 9;
        public const int Pin = 1 << 10;
        public const int Bus = 1 << 11;
        // public const int Default = 1 << 12;
        // public const int Default = 1 << 13;
        //public const int Default = 1 << 14;
        //public const int Default = 1 << 15;
        //public const int Default = 1 << 16;
        //public const int Default = 1 << 17;
        //public const int Default = 1 << 20;
        //public const int Default = 1 << 19;
        //public const int Default = 1 << 18;
        //public const int Default = 1 << 21;
        //public const int Default = 1 << 22;
        //public const int Default = 1 << 23;
        //public const int Default = 1 << 24;
        //public const int Default = 1 << 25;
        //public const int Default = 1 << 26;
        //public const int Default = 1 << 27;
        //public const int Default = 1 << 28;
        //public const int Default = 1 << 29;
        //public const int Default = 1 << 30;
        //public const int Default = 1 << 31;
    }



   public static class ProjectTags
    {
        public const string Untag = "Untagged";
        public const string Respawn = "Respawn";
        public const string Finish = "Finish";
        public const string EditorOnly = "EditorOnly";
        public const string MainCamera = "MainCamera";
        public const string Player = "Player";
        public const string MainController = "MainController";
        public const string ChipEditor = "ChipEditor";
        public const string InterfaceMask = "InterfaceMask";
    }

   public static class GameConstant
   {
        private const string MAJOR= "0";
        private const string MINOR= "40";
        private const string REVISION= "2";
        public static string GAMEVERSION =$"{MAJOR}.{MINOR}.{REVISION}";
        public static string GAMEVERSION_SAVE =$"{MAJOR}.{MINOR}-CE";

        public const string LASTEDIT= "20 Jan 2024";
   }