namespace Librex.Domain.Entities;

// Documentos que llevan folio consecutivo visible al usuario: remisiones, devoluciones y pagos.
// Existe para que el cálculo del siguiente folio viva una sola vez, en DocumentRepository.
public interface IFolioEntity
{
    int FolioNumber { get; set; }
}
