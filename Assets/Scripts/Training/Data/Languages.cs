
using Assets.Scripts.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Localization.Settings;

namespace Assets.Scripts.Data
{

    public class LanguageCodes
    {
        public const string ES_es = "es-ES";
        public const string EN_en = "en-EN";
    }

    public abstract class Language
    {
        public Language()
        {

        }

        public void Initialize(string userLang, float complexity)
        {
            // Validar complejidad
            if (complexity < 0f || complexity > 1f)
            {
                throw new ArgumentException("La complejidad debe estar entre 0 y 1");
            }

            var sampleSize = 200;

            float minDifficultyGroup1Ratio = 0.55f; 
            float maxDifficultyGroup1Ratio = 0.70f; 

            float minDifficultyGroup2Ratio = 0.75f; 
            float maxDifficultyGroup2Ratio = 0.85f;


            float rangeDifficultyGroup1 = maxDifficultyGroup1Ratio - minDifficultyGroup1Ratio;
            float rangeDifficultyGroup2 = maxDifficultyGroup2Ratio - minDifficultyGroup2Ratio;

            // Ajustar complexity para que esté dentro del rango deseado
            float adjustedComplexityDiffGroup1 = (1-complexity) * rangeDifficultyGroup1 + minDifficultyGroup1Ratio;
            float adjustedComplexityDiffGroup2 = (1-complexity) * rangeDifficultyGroup2 + minDifficultyGroup2Ratio;

            // Calcular cantidades de letras            
            var DifficultyGroup1Ratio = adjustedComplexityDiffGroup1;
            var DifficultyGroup2Ratio = adjustedComplexityDiffGroup2;

            var numLettersDifficultyGroup1 = (int)(sampleSize * DifficultyGroup1Ratio);
            var adjustedSampleSize = sampleSize - numLettersDifficultyGroup1;
            var numLettersDifficultyGroup2 = (int)(adjustedSampleSize * DifficultyGroup2Ratio);
            var numLettersDifficultyGroup3 = adjustedSampleSize - numLettersDifficultyGroup2;
            

            var _abecedary = GetLangAbecedary(userLang);

            // Generar arrays de muestreo
            var DiffGroup1Sample = ReservoirSampling(_abecedary.Where(c => c.DifficultyGroup == 1).ToList(), numLettersDifficultyGroup1, complexity);
            var DiffGroup2Sample = ReservoirSampling(_abecedary.Where(c => c.DifficultyGroup == 2).ToList(), numLettersDifficultyGroup2, complexity);
            var DiffGroup3Sample = ReservoirSampling(_abecedary.Where(c => c.DifficultyGroup == 3).ToList(), numLettersDifficultyGroup3, complexity);

            var sum = DiffGroup1Sample.Length + DiffGroup2Sample.Length + DiffGroup3Sample.Length;

            // Mezclar arrays de muestreo (Fisher-Yates)
            MapLetters = new char[sampleSize];
            int[] shuffledIndexes = Enumerable.Range(0, sampleSize).ToArray();
            shuffledIndexes.Shuffle();

            try
            {
                int i = 0;
                foreach (var index in shuffledIndexes)
                {
                    if (index < numLettersDifficultyGroup1)
                    {
                        MapLetters[i] = DiffGroup1Sample[index];
                    }
                    else if (index >= numLettersDifficultyGroup1 && index < (numLettersDifficultyGroup1 + numLettersDifficultyGroup2))
                    {
                        var arrayIndex = index - numLettersDifficultyGroup1;
                        
                        if(arrayIndex <= (DiffGroup2Sample.Length -1))
                        {
                            MapLetters[i] = DiffGroup2Sample[arrayIndex];
                        }
                        else
                        {
                            throw new Exception("Index out of bounds");
                        }
                    }
                    else
                    {
                        var arrayIndex = index - (numLettersDifficultyGroup1 + numLettersDifficultyGroup2);

                        if (arrayIndex <= (DiffGroup3Sample.Length - 1))
                        {
                            MapLetters[i] = DiffGroup3Sample[arrayIndex];
                        }
                        else
                        {
                            throw new Exception("Index out of bounds");
                        }

                    }

                    i++;
                }
            }
            catch (Exception)
            {
                throw;
            }


        }

        public char GetRandomLetter()
        {
            var index = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, MapLetters.Length);
            return MapLetters[index];
        }


        private static char[] ReservoirSampling(List<LangLetter> letters, int sampleSize, float complexity)
        {
            var reservoir = new char[sampleSize];
            //var numComplexConsonants = 0;

            //if (!isVowel)
            //{
            //    float minComplexConstantsRatio = 0.05f; // Definir el mínimo de vocales deseado
            //    float maxComplexConstantsRatio = 0.45f; // Definir el máximo de vocales deseado
            //    float range = maxComplexConstantsRatio - minComplexConstantsRatio;

            //    // Ajustar complexity para que esté dentro del rango deseado
            //    float adjustedConstantsComplexity = complexity * range;

            //    //var complexConsonantsRatio = (1 - adjustedConstantsComplexity);
            //    numComplexConsonants = (int)(sampleSize * adjustedConstantsComplexity);

            //    for (int j = 0; j < numComplexConsonants; j++)
            //    {
            //        var conplexLetters = letters.Where(c => c.Normalized < 0.5f).ToList();
            //        var randomIndex = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, conplexLetters.Count);

            //        reservoir[j] = conplexLetters[randomIndex].Letter;
            //    }
            //}            

            for (int j = 0; j < sampleSize; j++)
            {
                var accumulatedFrequency = 0f;
                var randomValue = UnityEngine.Random.Range(0f, 1f);
                LangLetter letter = null;

                while (randomValue > accumulatedFrequency)
                {
                    var randomIndex = System.Security.Cryptography.RandomNumberGenerator.GetInt32(0, letters.Count);
                    letter = letters[randomIndex];

                    accumulatedFrequency += letter.Normalized * (1-complexity);                    
                }

                reservoir[j] = letter.Letter;
            }


            return reservoir;
        }

        public static List<LangLetter> GetLangAbecedary(string userLang)
        {
            var lang = Language.GetLanguages().Where(c => c.Code == userLang).FirstOrDefault();
            return lang.Abecedary;
        }

        public string Code { get; set; }
        public List<LangLetter> Abecedary { get; set; }

        public int NumMaxLettersWord { get; set; }

        public static char[] MapLetters { get; set; }

        public static List<Language> GetLanguages()
        {
            return new List<Language>() { new SpanishLang(), new EnglishLang() };
        }

        public abstract string GetGameModeText(GameMode gameMode);

    }

    public class LangLetter
    {
        public LangLetter(char letter, float frequency, bool isVocal = false, float normalized = 0f, short difficultyGroup = 0)
        {
            Letter = letter;
            Frequency = frequency;
            IsVocal = isVocal;
            Normalized = normalized;
            DifficultyGroup = difficultyGroup;
        }
        public char Letter { get; set; }
        public float Frequency { get; set; }
        public bool IsVocal { get; set; }
        public float Normalized { get; set; }
        public short DifficultyGroup { get; set; }
        public float ObsNormalized { get; set; }

    }

    public class SpanishLang : Language
    {
        public SpanishLang()
        {
            Code = LanguageCodes.ES_es;
            Abecedary = new List<LangLetter>() {
                new LangLetter('A', 12.53f, true, 0.95f, 1), //
                new LangLetter('B', 1.42f, false, 0.6f, 2),//
                new LangLetter('C', 4.68f, false, 0.90f, 2), //
                new LangLetter('D', 5.86f, false, 0.95f, 2),//
                new LangLetter('E', 13.68f, true, 0.95f, 1), //
                new LangLetter('F', 0.69f, false, 0.85f, 3), //
                new LangLetter('G', 1.01f, false, 0.6f, 2), //
                new LangLetter('H', 0.7f, false, 0.26f, 3),//
                new LangLetter('I', 6.25f, true, 0.8f, 1), //
                new LangLetter('J', 0.44f, false, 0.4f, 3), //
                new LangLetter('K', 0.02f, false, 0.1f, 3),//
                new LangLetter('L', 4.97f, false, 0.9f, 2), //
                new LangLetter('M', 3.15f, false, 0.70f, 2), //
                new LangLetter('N', 6.71f, false, 0.45f, 1),//
                new LangLetter('Ñ', 0.31f, false, 0.2f, 3), //
                new LangLetter('O', 8.68f, true, 0.8f, 1),// 
                new LangLetter('P', 2.51f, false, 0.75f, 2),//
                new LangLetter('Q', 0.88f,   false, 0.2f, 3),// 
                new LangLetter('R', 6.87f, false, 0.65f, 1), //
                new LangLetter('S', 7.98f, false, 0.45f, 1),//
                new LangLetter('T', 4.63f,  false, 0.79f, 2), //
                new LangLetter('U', 3.93f, true, 0.69f, 1), //
                new LangLetter('V', 0.9f, false, 0.7f, 3),//
                new LangLetter('W', 0.01f, false, 0.1f, 3), //
                new LangLetter('X', 0.22f, false, 0.1f, 3), //
                new LangLetter('Y', 0.9f, false, 0.1f, 3),//
                new LangLetter('Z', 0.52f, false, 0.15f, 3) };//
            NumMaxLettersWord = 23;
        }

        public override string GetGameModeText(GameMode gameMode)
        {
            switch(gameMode)
            {
                case GameMode.CatchLetter:
                    return "Atrapa la letra!";
                case GameMode.PointsChallenge:
                    return "Desafío de puntos!";
                case GameMode.NLetterWordChallenge:
                    return "Desafío de palabras!";
                case GameMode.NLetterChallenge:
                    return "Desafío de letras!";
                case GameMode.HiddenWord:
                    return "Palabra oculta!";
                case GameMode.Flash:
                    return "Desafíoflash!";
                case GameMode.Puzze:
                    return "Puzzle!";
                case GameMode.VsAlgorithm:
                    return "Supera a mi algoritmo!";
                default:
                    return "";

            }
        }
    }

    public class EnglishLang : Language
    {
        //public EnglishLang()
        //{
        //    Code = LanguageCodes.EN_en;
        //    Abecedary = new List<LangLetter>() { 
        //        new LangLetter('A', 12.53f,  new float[]{ 0.70f, 1f }, true, 0f), new LangLetter('B', 1.42f, new float[]{ 0.70f, 1f }, false, 0.04f),
        //        new LangLetter('C', 4.68f, new float[]{ 0.70f, 1f }, false, 0.08f), new LangLetter('D', 5.86f, new float[]{ 0.70f, 1f }, false, 0.12f), new LangLetter('E', 13.68f, new float[]{ 0.70f, 1f }, true, 0.16f),
        //        new LangLetter('F', 0.69f, new float[]{ 0.70f, 1f }, false, 0.2f), new LangLetter('G', 1.01f, new float[]{ 0.70f, 1f }, false, 0.24f), new LangLetter('H', 0.7f, new float[]{ 0.70f, 1f }, false, 0.28f),
        //        new LangLetter('I', 6.25f, new float[]{ 0.70f, 1f }, true, 0.32f), new LangLetter('J', 0.44f, new float[]{ 0.70f, 1f }, false, 0.36f), new LangLetter('K', 0.02f, new float[]{ 0.70f, 1f }, false, 0.4f),
        //        new LangLetter('L', 4.97f, new float[]{ 0.70f, 1f }, false, 0.44f), new LangLetter('M', 3.15f, new float[]{ 0.70f, 1f }, false, 0.48f), new LangLetter('N', 6.71f, new float[]{ 0.70f, 1f }, false, 0.52f),
        //        new LangLetter('O', 8.68f, new float[]{ 0.70f, 1f }, true, 0.56f), new LangLetter('P', 2.51f, new float[]{ 0.70f, 1f }, false, 0.6f), new LangLetter('Q', 0.88f, new float[]{ 0.70f, 1f }, false, 0.64f),
        //        new LangLetter('R', 6.87f, new float[]{ 0.70f, 1f }, false, 0.68f), new LangLetter('S', 7.98f, new float[]{ 0.70f, 1f }, false, 0.72f), new LangLetter('T', 4.63f, new float[]{ 0.70f, 1f }, false, 0.76f),
        //        new LangLetter('U', 3.93f, new float[]{ 0.70f, 1f }, true, 0.8f), new LangLetter('V', 0.9f, new float[]{ 0.70f, 1f }, false, 0.84f), new LangLetter('W', 0.01f, new float[]{ 0.70f, 1f }, false, 0.88f),
        //        new LangLetter('X', 0.22f, new float[]{ 0.70f, 1f }, false, 0.92f), new LangLetter('Y', 0.9f, new float[]{ 0.70f, 1f }, false, 0.96f), new LangLetter('Z', 0.52f, new float[]{ 0.70f, 1f }, false, 1f) };
        //    NumMaxLettersWord = 30;
        //}

        public override string GetGameModeText(GameMode gameMode)
        {
            switch (gameMode)
            {
                case GameMode.CatchLetter:
                    return "Catch the letter!";
                case GameMode.PointsChallenge:
                    return "Points challenge!";
                case GameMode.NLetterWordChallenge:
                    return "Word challenge!";
                case GameMode.NLetterChallenge:
                    return "Letter challenge!";
                case GameMode.HiddenWord:
                    return "Hidden word!";
                case GameMode.Flash:
                    return "Flash challenge!";
                case GameMode.Puzze:
                    return "Puzzle!";
                case GameMode.VsAlgorithm:
                    return "Algotithm challenge!";
                default:
                    return "";

            }
        }
    }
}
