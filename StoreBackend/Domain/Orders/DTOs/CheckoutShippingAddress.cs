namespace Domain.Orders;

/// <summary>
/// The address a checkout is to be delivered to. Supplied per checkout rather than read
/// from the account, because the order keeps its own copy: see
/// <see cref="OrderShippingAddress"/>.
/// </summary>
public class CheckoutShippingAddress
{
    public required string FullName { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Country { get; set; }
    public required string City { get; set; }
    public string? Area { get; set; }
    public required string Street { get; set; }
    public required string Building { get; set; }
}
