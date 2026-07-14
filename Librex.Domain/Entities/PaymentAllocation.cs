namespace Librex.Domain.Entities;

// Aplicación de un pago a una remisión específica. Un pago (encabezado) puede repartirse
// entre varias remisiones; el remanente no asignado (Amount del pago − Σ asignaciones) es un anticipo.
public class PaymentAllocation : BaseEntity
{
    public int PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;
    public int RemissionId { get; set; }
    public Remission Remission { get; set; } = null!;
    public decimal Amount { get; set; }
}
