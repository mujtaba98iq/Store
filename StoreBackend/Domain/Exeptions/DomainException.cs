namespace Domain.Exeptions;

/// <summary>
/// Base class for the rules the API breaks on purpose: a name already taken, a
/// coupon past its window, a cart asking for stock that is not there.
/// </summary>
/// <remarks>
/// Deriving from this is a statement about the message, not just about the type.
/// The API answers one of these with the message it carries, so every message here
/// has to be written for whoever made the request - never a stack, a file path, a
/// provider's own error text, or anything else from inside the server. An
/// exception that cannot promise that belongs on <see cref="Exception"/>, where it
/// is reported as an unexpected failure and its detail stays in the log.
/// </remarks>
public abstract class DomainException(string message) : Exception(message);
