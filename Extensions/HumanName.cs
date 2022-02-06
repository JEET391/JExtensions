using System.Linq;

namespace JExtensions.Extensions
{
    public class HumanName
    {
        public HumanName(string fullName)
        {
            if (string.IsNullOrEmpty(fullName))
            {
                return;
            }
            var parts = fullName
                .Where(c => char.IsLetter(c)).ToString().Split(". ")
                .Select(p => p.Trim()).ToArray();

            switch (parts.Length)
            {
                case 1:
                    FirstName = parts[0];
                    break;

                case 2:
                    FirstName = parts[0];
                    LastName = parts[1];
                    break;

                case 3:
                    FirstName = parts[0];
                    MiddleName = parts[1];
                    LastName = parts[2];
                    break;

                default:
                    if (parts.Length >= 4)
                    {
                        Title = parts[0];
                        FirstName = parts[1];
                        MiddleName = parts[2];
                        LastName = parts[3];
                    }
                    break;
            }
            FullName = fullName;
        }

        public string FirstName { get; }
        public string FullName { get; }
        public string LastName { get; }
        public string MiddleName { get; }
        public string Title { get; }
    }
}