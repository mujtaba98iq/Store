using Domain.Data;
using Domain.Orders;
using Domain.Products;
using Domain.Users;

namespace Domain.Reviews;

/// <summary>
/// What a customer thought of a product, in stars and optionally in words.
///
/// A review carries the order it came out of as well as the product it is about. That is
/// what separates it from an opinion anybody could leave: the order is the proof the
/// customer actually bought the thing, and it is checked before the review is accepted.
///
/// The review sits on the product rather than on the variant that was shipped, because a
/// customer's opinion of a shirt is not an opinion of its size. All variants of a product
/// therefore share one set of reviews.
/// </summary>
public class Review : IAuditableEntity
{
    /// <summary>
    /// The star scale, held here rather than in the service so the domain rule and the
    /// request validation cannot drift apart.
    /// </summary>
    public const int MinRating = 1;
    public const int MaxRating = 5;

    public Guid Id { get; set; }

    /// <summary>
    /// The customer whose opinion this is. One review per customer per product, so an
    /// average rating cannot be moved by one person posting repeatedly.
    /// </summary>
    public Guid UserId { get; set; }

    public Guid ProductId { get; set; }

    /// <summary>
    /// The purchase that entitles the review. Kept for good after the fact rather than
    /// discarded once the check passes: it is the only thing that can later answer why this
    /// customer was allowed to review this product at all.
    /// </summary>
    public Guid OrderId { get; set; }

    /// <summary>
    /// One to five stars, and the only part of a review that is required. It is what a
    /// product's score is built from, so a review without one would say nothing measurable.
    /// </summary>
    public int Rating { get; set; }

    /// <summary>
    /// The customer's own words. Optional: plenty of people rate without writing, and a bare
    /// score is still a review.
    /// </summary>
    public string? Comment { get; set; }

    /// <summary>
    /// Whether staff have cleared this for the product page. False on arrival, so nothing a
    /// customer types is published on the strength of their own say-so, and false again after
    /// an edit: text cleared once is not the same text as the version that replaced it.
    /// </summary>
    public bool IsApproved { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string CreatedById { get; set; }
    public string? UpdatedById { get; set; }
    public string? DeletedById { get; set; }

    /// <summary>
    /// The rows a review points at. Empty unless a read asked for them, which the review's
    /// own reads do not: everything a reader of a review needs is on the review itself.
    /// </summary>
    public User? User { get; set; }
    public Product? Product { get; set; }
    public Order? Order { get; set; }
}
