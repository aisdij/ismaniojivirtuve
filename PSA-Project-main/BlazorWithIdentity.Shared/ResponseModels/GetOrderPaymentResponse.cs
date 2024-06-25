namespace Project.Shared.ResponseModels;

public class GetOrderPaymentResponse
{
    public string Id { get; set; }

    public double Price { get; set; }

    public double DiscountPercentage { get; set; }

    public int DiscountPointsUse { get; set; }
}
