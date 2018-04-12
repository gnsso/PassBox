using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PassBox
{
    static class IO
    {
        private static readonly string AppDataDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        private static readonly string CompanyDataDirectory = Path.Combine(AppDataDirectory, "Gnsso");
        private static readonly string ProjectDataDirectory = Path.Combine(CompanyDataDirectory, "PassBox");
        private static readonly string PasswordsFile = Path.Combine(ProjectDataDirectory, "passdata.json");

        static IO()
        {
            EnsureDirectoryExists(CompanyDataDirectory);
            EnsureDirectoryExists(ProjectDataDirectory);
            EnsureFileExists(PasswordsFile, "[]");
        }

        public static Password[] ReadPasswords()
        {
            return JsonConvert.DeserializeObject<Password[]>(File.ReadAllText(PasswordsFile));
        }

        public static void WritePassword(Password password)
        {
            var passwords = ReadPasswords();
            File.WriteAllText(PasswordsFile, JsonConvert.SerializeObject(passwords.Concat(new[] { password }).ToArray()));
        }

        public static bool DeletePassword(string name)
        {
            var passwords = ReadPasswords().ToList();
            var password = passwords.SingleOrDefault(s => s.Name == name);
            if (password != null)
            {
                passwords.Remove(password);
                File.WriteAllText(PasswordsFile, JsonConvert.SerializeObject(passwords));
                return true;
            }
            else return false;
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

        private static void EnsureFileExists(string path, string contents)
        {
            if (!File.Exists(path))
                File.WriteAllText(path, contents);
        }
    }
}
