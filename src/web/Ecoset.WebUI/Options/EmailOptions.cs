namespace Ecoset.WebUI.Options 
{
    public class EmailOptions {
        public string Host {get; set; }
        public string FromAddress {get; set; }
        public string FromName { get; set; }
        public int Port { get; set; }
        public bool UseSSL { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}