namespace ToDoList.Helpers
{
    public class JwtOption
    {
        public string Issuer {  get; set; }
        public string Audience {  get; set; }
        public int Lifetime {  get; set; }
        public string Signingkey {  get; set; }
    }
}
