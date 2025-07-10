namespace KorRaporOnline.API.Models.DTOs
{
    public static class ValidationConstants
    {
        // Report Related
        public const int MAX_REPORT_TITLE_LENGTH = 100;
        public const int MAX_REPORT_DESCRIPTION_LENGTH = 500;
        public const int MAX_QUERY_LENGTH = 4000;
        public const int MIN_TITLE_LENGTH = 3;

        // Database Related
        public const int MAX_CONNECTION_NAME_LENGTH = 100;
        public const int MAX_SERVER_NAME_LENGTH = 255;
        public const int MAX_DATABASE_NAME_LENGTH = 255;
        public const int MAX_USERNAME_LENGTH = 50;

        // Parameters
        public const int MAX_PARAMETER_NAME_LENGTH = 50;
        public const int MAX_PARAMETER_VALUE_LENGTH = 500;

        // Query Execution
        public const int MAX_EXECUTION_TIMEOUT_SECONDS = 300;
        public const int DEFAULT_PAGE_SIZE = 50;
        public const int MAX_PAGE_SIZE = 200;
    }
}