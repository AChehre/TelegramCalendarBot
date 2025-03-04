namespace TelegramCalendarBot
{
    public static class Messages
    {
        public const string CALENDAR_CALLBACK = "CALENDAR";
        public const string JCALENDAR_CALLBACK = "JCALENDAR";

        public const string StartMessage = 
            "Hey {0}! I am calendar bot.\n\nPlease type /calendar or /jcalendar to try out my features.";

        public const string CalendarPrompt = "Please select a date:";
        public const string CalendarResponse = "You selected {0}";

        public const string JCalendarPrompt = "لطفا تاریخی را انتخاب کنید:";
        public const string JCalendarResponse = "شما تاریخ {0} را انتخاب کردید";
    }
}
