using Assets.Scripts.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Localization.Settings;

namespace Assets.Scripts.Training.Data
{
    public class DictionaryUtilities
    {
        private static readonly string LOCTABLE = "TranslateTable";

        //public static string RemoveDiacritics(string text)
        //{
        //    string formD = text.Normalize(NormalizationForm.FormD);
        //    StringBuilder sb = new StringBuilder();

        //    foreach (char ch in formD)
        //    {
        //        UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(ch);
        //        if (uc != UnicodeCategory.NonSpacingMark)
        //        {
        //            sb.Append(ch);
        //        }
        //    }

        //    return sb.ToString().Normalize(NormalizationForm.FormC);
        //}

        public static string RemoveDiacritics(string text)
        {
            // Reemplazar 'ñ' y 'Ñ' con marcadores temporales
            string tempText = text.Replace("ñ", "##n##").Replace("Ñ", "##N##");

            string formD = tempText.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char ch in formD)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(ch);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(ch);
                }
            }

            // Recuperar 'ñ' y 'Ñ' reemplazando los marcadores temporales
            string result = sb.ToString().Normalize(NormalizationForm.FormC);
            result = result.Replace("##n##", "ñ").Replace("##N##", "Ñ");

            return result;
        }

        public static float GetWordComplexity(string wordNodiacritics)
        {
            var abecedary = new SpanishLang().Abecedary;

            //2. Tasa complejidad por letra
            List<LangLetter> wordletter = new List<LangLetter>();
            decimal[] lettersComplex = new decimal[wordNodiacritics.Length];

            int iter = 0;
            foreach (char letter in wordNodiacritics.ToUpper())
            {
                var abcLetter = abecedary.Where(c => c.Letter == letter).First();
                wordletter.Add(abcLetter);

                double[] x = { abecedary.Min(c => c.Frequency), abecedary.Max(c => c.Frequency) };
                double[] y = { 0.99, 0.01 };
                double xi = abcLetter.Frequency; ;
                double yi = calcula(x, y, xi);

                lettersComplex[iter] = Convert.ToDecimal(Math.Round(yi, 2));
                iter++;
            }

            var letterComplexRes = lettersComplex.Sum() / lettersComplex.Length;


            //2.Tasa constantes
            var vocalCons = (decimal)wordletter.Where(c => !c.IsVocal).Count() / (decimal)wordletter.Count;
            vocalCons = vocalCons > 0.99m ? 0.99m : (vocalCons <= 0 ? 0.1m : vocalCons);
            var vocalConsComplexRes = Convert.ToDecimal(Math.Round(vocalCons, 2));

            //3.Tasa por nº de letras
            var numLettersComplexRes = 0.01m;

            if (wordNodiacritics.Length <= 15)
            {
                double[] lx = { 3, 15 };
                double[] ly = { 0.01, 0.99 };
                double lxi = wordNodiacritics.Length;
                double lyi = calcula(lx, ly, lxi);

                numLettersComplexRes = Convert.ToDecimal(Math.Round(lyi, 2));
            }
            else
            {
                numLettersComplexRes = 0.99m;
            }


            var finalRes = Math.Round(((letterComplexRes + vocalConsComplexRes + numLettersComplexRes) / (decimal)3), 2);

            return (float)finalRes;
        }


        public static double calcula(double[] x, double[] y, double xi)
        {
            bool encontrado = false;
            double yi = 0;
            int i = 0;

            while (i < x.Length && encontrado == false)
            {
                if (x[i] <= xi && xi <= x[i + 1])
                {
                    yi = y[i] + (y[i + 1] - y[i]) / (x[i + 1] - x[i]) * (xi - x[i]);
                    encontrado = true;
                }
                i++;
            }
            return yi;
        }


        // Ejemplo para obtener una traducción
        public static string GetTranslation(string key, string texto)
        {
            var stringTable = LocalizationSettings.StringDatabase.GetTableAsync(LOCTABLE).WaitForCompletion();
            var entry = stringTable.GetEntry(key);

            if (entry != null)
            {
                return entry.GetLocalizedString();
            }
            else
            {

#if UNITY_EDITOR

                stringTable.AddEntry(key, texto);


                // Asegúrate de guardar los cambios si modificas la tabla
                UnityEditor.EditorUtility.SetDirty(stringTable);
                UnityEditor.AssetDatabase.SaveAssets();

#endif


                return texto;
            }

            
        }

    }

    class WordInfo
    {
        //public double Complexity { get; set; }
        public string NoDiacritics { get; set; }
        //public int NumLetters { get; set; }
        public long CreationDate { get; set; }
    }
}
