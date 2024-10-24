namespace LinkDev.Talabat.Infrastructure.Presistence._Common
{
    [AttributeUsage(AttributeTargets.Class)]
    public class DbContextTypeAttribute : Attribute
    {
        public Type DbContextType { get;  }

        public DbContextTypeAttribute(Type dbContextType)
        {
            DbContextType = dbContextType;
        }
    }
}
