namespace CloudConfiguration.WebAPI.Helpers
{
    public class FileLocation
    {
        public string FullName { get; set; }
        public string Name { get; set; }

        public FileLocation(string fullName, string name)
        {
            FullName = fullName;
            Name = name;
        }
    }
}