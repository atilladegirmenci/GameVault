namespace GameDBWebApi.Constants
{
    public static class ErrorMessages
    {
        public static class Common
        {
            public const string UnauthorizedAccess = "Unauthorized access.";
            public const string UnexpectedInternalServerError = "An unexpected internal server error occurred. Please try again later.";
            public const string ValidationFailed = "Validation failed.";
        }

        public static class Auth
        {
            public const string InvalidCredentials = "Invalid username or password.";
            public const string InvalidOrExpiredRefreshToken = "Invalid or expired refresh token.";
            public const string InvalidTokenClaims = "Invalid token claims.";
            public const string RegistrationSuccessful = "Registration successful";
            public const string LoginSuccessful = "Login successful.";
            public const string TokenRefreshedSuccessfully = "Token refreshed successfully.";
            public const string LogoutSuccessful = "Logout successful";
            public const string EmailAlreadyExists = "Email already exists.";
            public const string UsernameAlreadyTaken = "Username is already taken.";
            public const string JwtSecretKeyMissing = "JWT Secret Key is missing!";
            public const string JwtSecretKeyNotConfigured = "JWT SecretKey is not configured.";
        }

        public static class User
        {
            public const string UserNotFound = "User not found.";
            public const string UserUpdatedSuccessfully = "User updated successfully";
            public const string UserDeletedSuccessfully = "User deleted successfully";
        }

        public static class Review
        {
            public const string RawgIdMustBeGreaterThanZero = "RAWG game ID must be greater than zero.";
            public const string AlreadyReviewed = "You have already reviewed this game.";
            public const string ReviewIdMustBeGreaterThanZero = "Review ID must be greater than 0.";
            public const string ReviewNotFoundTemplate = "Review not found.";
            public const string DeleteNotAllowed = "You are not allowed to delete this review.";
            public const string PageMustBeGreaterThanOrEqualToOne = "Page must be greater than or equal to 1.";
            public const string PageSizeRange = "Page size must be between 1 and 100.";
        }

        public static class Game
        {
            public const string KeywordCannotBeNullOrEmpty = "Keyword cannot be null or empty";
            public const string RawgIdMustBeGreaterThanZero = "RAWG ID must be greater than 0.";
            public const string RawgGameIdMustBePositive = "RAWG game ID must be a positive integer.";
        }

        public static class UserList
        {
            public const string ListTitleCannotBeNullOrEmpty = "List title cannot be null or empty.";
            public const string UserListNotFound = "User list not found.";
            public const string NoPermissionForList = "You do not have permission to access this list.";
            public const string GameAlreadyInList = "This game is already in the list.";
            public const string ListMaxGamesTemplate = "Limit has reached the maximum number of games.";
            public const string GameNotFoundInList = "Game not found in this list.";
        }

        public static class External
        {
            public const string RawgApiKeyNotConfigured = "RAWG API key is not configured.";
            public const string ExternalApiErrorFetchingGameDetailsTemplate = "External API error occurred while fetching game details. please try again later.";
            public const string ExternalApiErrorFetchingFeedTemplate = "External API error occurred during game feed fetch. please try again later.";
            public const string GameNotFoundTemplate = "Game not found.";
            public const string GameDetailsNotFoundTemplate = "Game details not found.";
            public const string FailedToDeserializeGameDetailsTemplate = "Failed to deserialize game details";
            public const string GameNotFoundOnRawg = "Game not found on RAWG.";
            public const string RawgGameNotFoundTemplate = "Game not found.";
        }
    }
}
