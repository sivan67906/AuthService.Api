namespace AuthService.Application.Features.UserAddresses.GetAllAddresses;

public class AllAddressItemDto
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Line1 { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
}
