namespace RestApi.Orders;

/// <summary>
/// Where the order should be delivered. Sent with the checkout rather than looked up,
/// because the order keeps its own copy: changing an address afterwards must not rewrite
/// where an order that has already gone out was sent.
/// </summary>
public class ShippingAddressRequest
{
    /// <summary>
    /// Who the parcel is addressed to, which is not always the account holder.
    /// </summary>
    public required string FullName { get; set; }

    /// <summary>
    /// The number the courier calls on arrival.
    /// </summary>
    public required string PhoneNumber { get; set; }

    public required string Country { get; set; }
    public required string City { get; set; }

    /// <summary>
    /// District or neighbourhood. Optional: not every address is given with one.
    /// </summary>
    public string? Area { get; set; }

    public required string Street { get; set; }

    /// <summary>
    /// House or building number.
    /// </summary>
    public required string Building { get; set; }
}
