using System.Security.Cryptography;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System;

public static class StringExtensions
{
    extension(string input)
    {
        public string ToHash()
        {
            if (input == null) throw new ArgumentNullException(paramName: nameof(input));

            // Erstellen eines SHA256-Hash-Algorithmus
            using var sha256Hash = SHA256.Create();
            // Konvertieren des Eingabestrings in ein Byte-Array und Berechnen des Hashes
            var data = sha256Hash.ComputeHash(buffer: Encoding.UTF8.GetBytes(input));

            // Erstellen eines neuen StringBuilder, um die Bytes zu sammeln und dann in einen String umzuwandeln
            var sBuilder = new StringBuilder();

            // Durchlaufen jedes Bytes des gehashten Daten-Arrays und Formatieren jedes Bytes als hexadezimale Zeichenfolge
            foreach (var t in data) sBuilder.Append(value: t.ToString(format: "x2"));

            // Rückgabe des hexadezimalen String
            return sBuilder.ToString();
        }
    }
}
