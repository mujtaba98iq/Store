namespace RestApi.Orders;

/// <summary>
/// The address the order was sent to, as it read at checkout. A client should render this
/// and not the customer's current address: the two are allowed to differ, and on an old
/// order they usually do.
/// </summary>
public class OrderShippingAddressResponse
{
    public required string Id { get; set; }
    public required string OrderId { get; set; }
    public required string FullName { get; set; }
    public required string PhoneNumber { get; set; }
    public required string Country { get; set; }
    public required string City { get; set; }
    public string? Area { get; set; }
    public required string Street { get; set; }
    public required string Building { get; set; }
}
