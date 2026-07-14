namespace Librex.Domain.Entities;

public class Payment : BaseEntity
{
    public int FolioNumber { get; set; }
    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? Reference { get; set; }
    public string? Notes { get; set; }

    // Datos del recibo de pago (formato físico, a nivel cliente).
    public string? ReceivedFrom { get; set; } // "Recibimos de" (quien entrega el pago)
    public string? Concept { get; set; }      // "Por concepto de"
    public string? CollectedBy { get; set; }  // "Vendedor ó Cobrador"
    public string? City { get; set; }         // "Municipio" (lugar de expedición)

    public ICollection<PaymentAllocation> Allocations { get; set; } = [];
}
