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
            public const string EmailNotConfirmed = "Email address has not been verified.";
            public const string InvalidOrExpiredEmailVerificationToken = "Invalid or expired email verification token.";
            public const string InvalidOrExpiredPasswordResetToken = "Invalid or expired password reset token.";
            public const string CurrentPasswordIncorrect = "Current password is incorrect.";
            public const string PasswordResetRequested = "If the account exists, a password reset email has been sent.";
            public const string PasswordChangedSuccessfully = "Password changed successfully.";
        }

        public static class User
        {
            public const string UserNotFound = "User not found.";
            public const string UserUpdatedSuccessfully = "User updated successfully";
            public const string UserDeletedSuccessfully = "User deleted successfully";
        }

        public static class Review
        {
            public const string CatalogIdMustBeGreaterThanZero = "Game catalog ID must be greater than zero.";
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
            public const string CatalogIdMustBeGreaterThanZero = "Game catalog ID must be greater than 0.";
            public const string CatalogGameIdMustBePositive = "Game catalog ID must be a positive integer.";
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
            public const string ExternalApiErrorFetchingGameDetailsTemplate = "External API error occurred while fetching game details. please try again later.";
            public const string ExternalApiErrorFetchingFeedTemplate = "External API error occurred during game feed fetch. please try again later.";
            public const string GameNotFoundTemplate = "Game not found.";
            public const string GameDetailsNotFoundTemplate = "Game details not found.";
            public const string FailedToDeserializeGameDetailsTemplate = "Failed to deserialize game details";
            public const string GameNotFoundOnCatalog = "Game not found in the external catalog.";
            public const string CatalogGameNotFoundTemplate = "Game not found.";
            public const string CatalogServiceTemporarilyUnavailable = "The game catalog is temporarily unavailable. Please try again later.";
            public const string CatalogServiceTimeout = "The game catalog did not respond in time. Please try again later.";
            public const string CatalogServiceUnreachable = "The game catalog could not be reached. Please try again later.";
            public const string IgdbClientIdNotConfigured = "IGDB client ID is not configured.";
            public const string IgdbClientCredentialsInvalid = "Unable to authenticate with the IGDB service.";
            public const string IgdbRequestFailed = "The IGDB request failed. Please try again later.";
            public const string IgdbResponseInvalid = "The IGDB service returned an invalid response.";
            public const string IgdbGameNotFound = "Game not found in IGDB.";
            public const string IgdbServiceTemporarilyUnavailable = "The IGDB service is temporarily unavailable. Please try again later.";
            public const string IgdbServiceTimeout = "The IGDB service did not respond in time. Please try again later.";
            public const string IgdbServiceUnreachable = "The IGDB service could not be reached. Please try again later.";
            public const string RawgServiceTemporarilyUnavailable = "The RAWG service is temporarily unavailable. Please try again later.";
            public const string RawgServiceTimeout = "The RAWG service did not respond in time. Please try again later.";
            public const string RawgServiceUnreachable = "The RAWG service could not be reached. Please try again later.";
        }
    }
}
