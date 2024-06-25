namespace Project.Backend.Server.Database
{
    public static class DatabaseExtensions
    {
        public static async Task AddEntityAsync<TTable>(this DatabaseContext context, TTable table)
            where TTable : class
        {
            await context.AddAsync(table);
            await context.SaveChangesAsync();
        }
    }
}
