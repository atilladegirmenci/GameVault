namespace GameDBWebApi.Entites
{
    /// <summary>
    /// Represents the status of a game in a user's personal library.
    /// </summary>
    public enum GameStatus
    {
        /// <summary>Planned to play in the future.</summary>
        PlansToPlay = 1,

        /// <summary>Currently playing.</summary>
        Playing = 2,

        /// <summary>Play session finished/completed.</summary>
        Finished = 3
    }
}
