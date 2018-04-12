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
                { "f|format=", "Parola formati (kucuk harf: 'l', buyuk harf: 'L', rakam: 'd', noktalama: 'p') örn: 'dddlllpL'", v => format = v },
                { "r|nonrepeat", "Parola icindeki art arda gelen ayni tip karakterlerin tekrarlamamasi", v => nonRepeat = v != null },
                { "n|name=", "Parolayla iliskilendirilecek isim", v => name = v },
                { "u|username=", "Parolaya ait kullanici adi", v => userName = v },
                { "e|email=", "Parolaya ait email", v => email = v },
                { "t|defaultemail=", "Parola uretilirken kullanilacak ontanimli email", v => defaultEmail = v },
                { "g|get=", "Parolayla iliskilendirilmis isime göre bul", v => getByName = v },
                { "d|delete=", "Parolayla iliskilendirilmis isimle sil", v => deleteByName = v },
                { "v|value=", "Yeni parola uretmeden, var olan parolayi kaydet", v => value = v },
                { "h|?|help", "Yardım", v => showHelp = v != null  },
            };

            List<string> extra;
            try
            {
                extra = optionSet.Parse(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata: {ex.Message}");
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
                Console.WriteLine($"Parola uretilirken kullanilacak email '{defaultEmail}' olarak ayarlandi");
                return;
            }

            var passwords = IO.ReadPasswords();

            if (!string.IsNullOrWhiteSpace(getByName))
            {
                var exactMatchedPassword = passwords.SingleOrDefault(s => s.Name == getByName);
                if (exactMatchedPassword != null)
                {
                    Clipboard.SetText(exactMatchedPassword.Value);
                    Console.WriteLine($"'{exactMatchedPassword.Name}' ile iliskilendirilmis parola panoya kopyalandi");
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
                    Console.WriteLine("Parolayi panoya kopyalamak icin sec: ");
                    if (int.TryParse(Console.ReadLine(), out var index) && index < passwords.Length)
                    {
                        var password = matchedPasswords[index];
                        Clipboard.SetText(password.Value);
                        Console.WriteLine($"'{password.Name}' ile iliskilendirilmis parola panoya kopyalandi");
                    }
                }
                else
                {
                    Console.WriteLine("Parola bulunamadi");
                }
                return;
            }

            if (!string.IsNullOrWhiteSpace(deleteByName))
            {
                if (IO.DeletePassword(deleteByName))
                {
                    Console.WriteLine($"'{deleteByName}' ile iliskilendirlmis parola silindi");
                }
                else Console.WriteLine($"'{deleteByName}' ile iliskilendirlmis parola bulunamadi");
                return;
            }

            if ((!string.IsNullOrWhiteSpace(name) &&
                (!string.IsNullOrWhiteSpace(email) || !string.IsNullOrWhiteSpace(email = Settings.Default.DefaultEmail))) ||
                (extra.Count > 0 && !string.IsNullOrWhiteSpace(name = extra[0])))
            {
                if (passwords.Any(a => a.Name == name))
                {
                    Console.WriteLine("Bu isim ile iliskilendirilmis parola zaten var. Farkli bir isim deneyin.");
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

            Console.WriteLine("Bir veya daha fazla parametre hatali. Yardim icin: 'passbox --help'");
        }

        static void ListPasswords(Password[] passwords, out bool exit)
        {
            if (passwords.Length == 0)
            {
                Console.WriteLine("Bu isim ile iliskilendirilmis parola bulunamadi");
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
                Console.WriteLine("Parolayi panoya kopyalamak icin sec: ");
                if (int.TryParse(Console.ReadLine(), out var index) && index < passwords.Length)
                    Clipboard.SetText(passwords[index].Value);
                exit = true;
            }
        }

        static void ShowHelp(OptionSet optionSet)
        {
            Console.WriteLine("Yardim:");
            optionSet.WriteOptionDescriptions(Console.Out);
        }
    }
}

