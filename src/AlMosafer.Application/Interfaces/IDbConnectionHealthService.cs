namespace AlMosafer.Application.Interfaces;

public interface IDbConnectionHealthService
{
    Task<(bool CanConnect, string Message, string DatabaseName)> CheckConnectionAsync();
}
