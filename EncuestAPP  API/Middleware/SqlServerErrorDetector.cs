using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace EncuestAPP__API.Middleware;

public static class SqlServerErrorDetector
{
    public static bool EsRestriccionUnica(DbUpdateException exception) =>
        exception.GetBaseException() is SqlException { Number: 2601 or 2627 };
}
