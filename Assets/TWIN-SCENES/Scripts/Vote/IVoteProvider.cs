using System;

namespace TWIN.Vote
{
    /// <summary>
    /// Source of the audience's decision at a fork. The greybox ships KeyboardVoteProvider
    /// (keys 1/2 or on-screen buttons). A live-audience provider (Twitch chat, web buttons, ...)
    /// implements this interface and is assigned to ForkController.providerBehaviour.
    /// </summary>
    public interface IVoteProvider
    {
        /// <summary>Start collecting votes. Call onResult exactly once with true = STRANGE won.</summary>
        void StartVote(ForkDefinition fork, Action<bool> onResult);

        /// <summary>Stop collecting; onResult must not be called afterwards.</summary>
        void CancelVote();
    }
}
