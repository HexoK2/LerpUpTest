using System.Security.Cryptography;
using System.Text;

namespace LerpUp.Testeur
{
    // Le code donné par le Testeur : LUP-<numéro>-<4 caractères>.
    // Les 4 caractères viennent d'un SHA-256 de "idSalle:numéro:sel", en base 32 sans caractères ambigus.
    // Le même calcul existe côté site, dans js/testeur.js : les deux doivent donner exactement le même résultat.
    //
    // ATTENTION : ce n'est PAS inviolable. Le sel est dans ce fichier, donc quelqu'un qui lit le code peut calculer
    // un faux code. Ça suffit pour un carnet de progression personnel. Pour de vrais certificats, il faudra passer
    // par un serveur qui garde le secret.
    public static class CodeLerpUp
    {
        const string Sel = "lerpup-testeur-v1";
        const string Alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // 32 caractères, sans I ni O (ni 0 ni 1)

        public static string Calculer(string idSalle, string numero)
        {
            byte[] h;
            using (var sha = SHA256.Create())
                h = sha.ComputeHash(Encoding.UTF8.GetBytes(idSalle + ":" + numero + ":" + Sel));
            // 20 bits au début du hachage = 4 groupes de 5 bits
            int bits = (h[0] << 12) | (h[1] << 4) | (h[2] >> 4);
            var fin = new StringBuilder();
            for (int i = 0; i < 4; i++)
                fin.Append(Alphabet[(bits >> (15 - 5 * i)) & 31]);
            return "LUP-" + numero + "-" + fin;
        }
    }
}
