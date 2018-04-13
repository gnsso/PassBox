using NDesk.Options;
using PassBox.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PassBox
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            bool showHelp = false;
            string getByName = null;
            string format = null;
            bool nonRepeat = false;
            string name = null, userName = null, email = null;
            string defaultEmail = null;
            string deleteByName = null;
            string value = null;

            var optionSet = new OptionSet
            {
                { "f|format=", Strings.Format, v => format = v },
                { "r|nonrepeat", Strings.NonRepeat, v => nonRepeat = v != null },
                { "n|name=", Strings.Name, v => name = v },
                { "u|username=", Strings.UserName, v => userName = v },
                { "e|email=", Strings.Email, v => email = v },
                { "t|defaultemail=", Strings.DefaultEmail, v => defaultEmail = v },
                { "g|get=", Strings.Get, v => getByName = v },
                { "d|delete=", Strings.Delete, v => deleteByName = v },
                { "v|value=", Strings.Value, v => value = v },
                { "h|?|help", Strings.Help, v => showHelp = v != null  },
            };

            List<string> extra;
            try
            {
                extra = optionSet.Parse(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Strings.Error}: {ex.Message}");
                ShowHelp(optionSet);
                return;
            }

            if (showHelp)
            {
                ShowHelp(optionSet);
                return;
            }

            if (!string.IsNullOrWhiteSpace(defaultEmail))
            {
                Settings.Default.DefaultEmail = defaultEmail;
                Settings.Default.Save();
                Console.WriteLine(string.Format(Strings.SetDefaultEmail, defaultEmail));
                return;
            }

            var passwords = IO.ReadPasswords();

            if (!string.IsNullOrWhiteSpace(getByName))
            {
                var exactMatchedPassword = passwords.SingleOrDefault(s => s.Name == getByName);
                if (exactMatchedPassword != null)
                {
                    Clipboard.SetText(exactMatchedPassword.Value);
                    Console.WriteLine(string.Format(Strings.CopiedToClipboard, exactMatchedPassword.Name));
                    return;
                }
                var matchedPasswords = passwords.Where(w => w.Name.Contains(getByName)).ToArray();
                if (matchedPasswords.Count() > 0)
                {
                    for (int i = 0; i < matchedPasswords.Length; i++)
                    {
                        var password = matchedPasswords[i];
                        Console.WriteLine($"{i}. {password.Name} {password.Email}");
                    }
                    Console.WriteLine("--------------------");
                    Console.WriteLine(Strings.SelectToCopy);
                    if (int.TryParse(Console.ReadLine(), out var index) && index < passwords.Length)
                    {
                        var password = matchedPasswords[index];
                        Clipboard.SetText(password.Value);
                        Console.WriteLine(string.Format(Strings.CopiedToClipboard, password.Name));
                    }
                }
                else
                {
                    Console.WriteLine(Strings.NotFound);
                }
                return;
            }

            if (!string.IsNullOrWhiteSpace(deleteByName))
            {
                if (IO.DeletePassword(deleteByName))
                {
                    Console.WriteLine(string.Format(Strings.DeletedByName, deleteByName));
                }
                else Console.WriteLine(string.Format(Strings.NotFoundByName, deleteByName));
                return;
            }

            if ((!string.IsNullOrWhiteSpace(name) &&
                (!string.IsNullOrWhiteSpace(email) || !string.IsNullOrWhiteSpace(email = Settings.Default.DefaultEmail))) ||
                (extra.Count > 0 && !string.IsNullOrWhiteSpace(name = extra[0])))
            {
                if (passwords.Any(a => a.Name == name))
                {
                    Console.WriteLine(Strings.AlreadyExists);
                    return;
                }

                var password = new Password
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Name = name,
                    Username = userName ?? "",
                    Email = email,
                    Value = value ?? (format != null ? Random.GeneratePassword(format, nonRepeat) : Random.GeneratePassword(nonRepetitive: nonRepeat))
                };
                IO.WritePassword(password);
                Clipboard.SetText(password.Value);
                return;
            }

            Console.WriteLine(Strings.ErrorOneOrMoreParameters);
        }

        static void ListPasswords(Password[] passwords, out bool exit)
        {
            if (passwords.Length == 0)
            {
                Console.WriteLine(Strings.NotFoundByAnyName);
                exit = false;
            }
            else if (passwords.Length == 1)
            {
                Clipboard.SetText(passwords.Single().Value);
                exit = true;
            }
            else
            {
                for (int i = 0; i < passwords.Length; i++)
                {
                    var password = passwords[i];
                    Console.WriteLine($"{i}. {password.Name} {password.Email}");
                }
                Console.WriteLine("----------");
                Console.WriteLine(Strings.SelectToCopy);
                if (int.TryParse(Console.ReadLine(), out var index) && index < passwords.Length)
                    Clipboard.SetText(passwords[index].Value);
                exit = true;
            }
        }

        static void ShowHelp(OptionSet optionSet)
        {
            Console.WriteLine(Strings.Help);
            optionSet.WriteOptionDescriptions(Console.Out);
        }
    }
}

