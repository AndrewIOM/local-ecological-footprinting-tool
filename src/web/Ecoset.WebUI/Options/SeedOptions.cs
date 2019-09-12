namespace Ecoset.WebUI.Options 
{
    public class SeedOptions {
        public AdminAccount AdminAccount { get; set; }
    }

    public class AdminAccount {
        public string DefaultAdminUserName { get; set; }
        public string DefaultAdminPassword { get; set; }
    }
}